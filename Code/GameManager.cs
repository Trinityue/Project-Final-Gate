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

    private bool mouseInNoBuildZone = false;

    private float towerchoiche = 1f;
    private CommandCenter commandCenter;

    public override void _Ready()
    {
        commandCenter = GetNodeOrNull<CommandCenter>("CommandCenter");
        if (commandCenter != null)
            commandCenter.Visible = false;
        else
            GD.PrintErr("CommandCenter nicht gefunden! Bitte SceneTree prüfen.");
    }

    public override void _Process(double delta)
    {
        if (Player_Health <= 0)
        {
            GetTree().ChangeSceneToFile("res://Game-Over_Screen.tscn");
        }
        if (Input.IsActionJustPressed("ui_1"))
        {
            towerchoiche = 1f;
            GD.Print("Turm 1 ausgewählt");

        }
        else if (Input.IsActionJustPressed("ui_2"))
        {
            towerchoiche = 2f;
            GD.Print("Turm 2 ausgewählt");
        }

        if (Input.IsActionJustPressed("ui_CC") && commandCenter != null)
        {
            commandCenter.Visible = !commandCenter.Visible; 
            GD.Print("Command Center toggled: " + commandCenter.Visible);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            if (mouseInNoBuildZone)
            {
                GD.Print("Hier darf kein Turm platziert werden!");
                return;
            }

            float minDistance = 46f; // Mindestabstand zwischen Tower und NoBuildZone
            float minDistanceTower = 90f; // Mindestabstand zwischen Tower und Tower
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

            if (!canPlace)
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
                GD.Print("Nicht genug Geld!");
            }
        }
    }

    // Diese Methoden werden von Area2D-Signalen aufgerufen:
    public void OnMouseEntered()
    {
        mouseInNoBuildZone = true;
        GD.Print("Mouse entered NoBuildZone");
    }

    public void OnMouseExited()
    {
        mouseInNoBuildZone = false;
        GD.Print("Mouse exited NoBuildZone");
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

