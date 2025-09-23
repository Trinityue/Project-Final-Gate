using Godot;
using System;

public partial class GameManager : Node2D

    // ...existing code...
{
    [Export] public PackedScene Tower1Scene;
    [Export] public PackedScene Tower2Scene;

    [Export] public int TowerCost = 50;
    [Export] public float Player_Health = 100f;
    [Export] public float Player_Max_Health = 100f;

    public bool PlacingDebug { get; set; } = false;

    private bool mouseInNoBuildZone = false;

    private float towerchoiche = 1f;
    private CommandCenter commandCenter;
    private bool _isPaused = false;
    public static GameManager Instance { get; private set; }

    public override void _Ready()
    {
    Instance = this;
        try
        {
            // First try: search under this GameManager node (user reported CommandCenter is a child here)
            var foundLocal = FindNodeRecursive(this, "CommandCenter");
            if (foundLocal != null)
            {
                commandCenter = foundLocal as CommandCenter;
                GD.Print("CommandCenter gefunden unter GameManager: " + commandCenter.Name);
            }

            // Try common relative path as a fallback
            if (commandCenter == null)
                commandCenter = GetNodeOrNull<CommandCenter>("CommandCenter");

            // Try under a root Node2D if not found
            if (commandCenter == null)
            {
                var rootNode2D = GetTree().Root.GetNodeOrNull<Node2D>("Node2D");
                if (rootNode2D != null)
                    commandCenter = rootNode2D.GetNodeOrNull<CommandCenter>("CommandCenter");
            }

            // Fallback: search the entire scene tree
            if (commandCenter == null)
            {
                var found = FindNodeRecursive(GetTree().Root, "CommandCenter");
                if (found != null)
                    commandCenter = found as CommandCenter;
            }

            if (commandCenter != null)
            {
                commandCenter.Visible = false;
            }
            else
            {
                GD.PrintErr("CommandCenter nicht gefunden! Bitte SceneTree prüfen.");
                // helpful debug: list top-level children under root
                foreach (Node child in GetTree().Root.GetChildren())
                    GD.PrintErr($"Root child: {child.Name} ({child.GetType()})");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr("Exception in GameManager._Ready: " + ex);
        }
    }

    // Soft pause control used by CommandCenter
    public void SetPaused(bool paused)
    {
        _isPaused = paused;

    }

    private Node FindNodeRecursive(Node parent, string name)
    {
        if (parent == null) return null;
        if (parent.Name == name) return parent;
        foreach (Node child in parent.GetChildren())
        {
            var found = FindNodeRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    // Public setter methods used by CommandCenter
    public void SetTowerCost(int cost)
    {
        TowerCost = cost;
    }

    public void SetPlayerHealth(float health)
    {
        Player_Health = health;
    }

    public void SetPlayerMaxHealth(float max)
    {
        Player_Max_Health = max;
    }

    public void SetTowerChoice(float choice)
    {
        towerchoiche = choice;
    }
    public override void _Process(double delta)
    {
        if (Player_Health <= 0)
        {
            GetTree().ChangeSceneToFile("res://Game-Over_Screen.tscn");
        }

        // Always allow toggling the CommandCenter even while paused
        if (Input.IsActionJustPressed("ui_CC") && commandCenter != null)
        {
            bool next = !commandCenter.Visible;
            commandCenter.ShowWithFocus(next);
            GD.Print("Command Center toggled: " + next);
            // set internal flag to reflect global state
            _isPaused = GameState.Paused;
        }

        if (GameState.Paused)
        {
            // Skip game logic while paused
            return;
        }

        if (Input.IsActionJustPressed("ui_1"))
        {
            towerchoiche = 1f;
            if (PlacingDebug == true)
            {
                GD.Print("Turm 1 ausgewählt");
            }

        }
            else if (Input.IsActionJustPressed("ui_2"))
            {
                towerchoiche = 2f;
                if (PlacingDebug == true)
                {
                    GD.Print("Turm 2 ausgewählt");
                }
            }
    }

    public override void _Input(InputEvent @event)
    {
        if (_isPaused) return;
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            if (mouseInNoBuildZone && PlacingDebug == true)
            {
                GD.Print("Hier darf kein Turm platziert werden!");
                return;
            }

            float minDistance = 46f; // Mindestabstand zwischen Tower und NoBuildZone
            float minDistanceTower = 108f; // Mindestabstand zwischen Tower und Tower
            Vector2 mousePos = GetGlobalMousePosition();
            bool canPlace = true;
            // Abstand zu anderen Türmen prüfen
            foreach (var node in GetTree().GetNodesInGroup("Towers"))
            {
                if (node is Node2D tower)
                {
                    if (tower.GlobalPosition.DistanceTo(mousePos) < minDistanceTower)
                    {
                        canPlace = false;
                        break;
                    }
                }
            }

            // Abstand zur NoBuildZone prüfen
            if (canPlace)
            {
                var noBuildZone = GetTree().Root.FindChild("NoBuildZone", true, false) as Area2D;
                if (noBuildZone != null)
                {
                    foreach (Node child in noBuildZone.GetChildren())
                    {
                        if (child is CollisionPolygon2D poly)
                        {
                            var points = poly.Polygon;
                            for (int i = 0; i < points.Length; i++)
                            {
                                Vector2 a = poly.ToGlobal(points[i]);
                                Vector2 b = poly.ToGlobal(points[(i + 1) % points.Length]);
                                float dist = DistancePointToSegment(mousePos, a, b);
                                if (dist < minDistance)
                                {
                                    canPlace = false;
                                    break;
                                }
                            }
                        }
                        if (!canPlace) break;
                    }
                }
            }

            if (!canPlace && PlacingDebug == true)
            {
                GD.Print("Zu nah an einem anderen Turm oder an der NoBuildZone!");
                return;
            }

            if (MoneyManagment.Instance.CanAfford(TowerCost) && towerchoiche == 1f && Tower1Scene != null)
            {
                var tower = Tower1Scene.Instantiate() as Node2D;
                tower.GlobalPosition = mousePos;
                GetTree().Root.AddChild(tower);
                tower.AddToGroup("Towers");
                MoneyManagment.Instance.Spend(TowerCost);
            }
            else if (MoneyManagment.Instance.CanAfford(TowerCost) && towerchoiche == 2f && Tower2Scene != null)
            {
                var tower = Tower2Scene.Instantiate() as Node2D;
                tower.GlobalPosition = mousePos;
                GetTree().Root.AddChild(tower);
                tower.AddToGroup("Towers");
                MoneyManagment.Instance.Spend(TowerCost);
            }
            else 
            {
                if (PlacingDebug == true)
                {
                    GD.Print("Nicht genug Geld!");
                }
            }
        }
    }

    // Diese Methoden werden von Area2D-Signalen aufgerufen:
    public void OnMouseEntered()
    {
        mouseInNoBuildZone = true;
        if (PlacingDebug == true)
        {
            GD.Print("Mouse entered NoBuildZone");
        }
        
    }

    public void OnMouseExited()
    {
        mouseInNoBuildZone = false;
        if (PlacingDebug == true)
        {
            GD.Print("Mouse exited NoBuildZone");
        }
    }

    // Hilfsfunktion für Abstand Punkt zu Linie
    private float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = ((p - a).Dot(ab)) / ab.LengthSquared();
        t = Mathf.Clamp(t, 0, 1);
        Vector2 closest = a + t * ab;
        return p.DistanceTo(closest);
    }

}

