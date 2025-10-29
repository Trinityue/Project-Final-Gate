using Godot;
using System;
using System.Runtime;

public partial class Tower : Node2D
{
    [Export] public PackedScene BulletScene;
    [Export] public Node2D Spawner;
    [Export] public float bps = 1f; // bullets per second
    [Export] public float MinSpeed = 80f;
    [Export] public float MaxSpeed = 150f;
    [Export] public float AttackRange = 100f;
    public Enemy Target { get; set; }

    public float spawnRate;
    public float timeUntilSpawn = 0f;
    [Export] public float ClickRadius = 48f; // radius to detect clicks on tower

    // manual control flag
    private bool manualMode = false;

    // reference to the opened upgrade menu instance (if any)
    private Node2D upgradeMenuInstance = null;
    [Export] public float ManualAimRadius = 64f; // how close to the mouse an enemy must be to be manually targeted

    public override  void _Ready()
    {
        spawnRate = 1f / bps;
    }

    public override void _Process(double delta)
    {
        if (GameState.Paused) return;
        // close upgrade menu on ESC if open
        if (upgradeMenuInstance != null && IsInstanceValid(upgradeMenuInstance) && Input.IsActionJustPressed("ui_esc"))
        {
            upgradeMenuInstance.QueueFree();
            upgradeMenuInstance = null;
        }
        // Manual mode: tower follows mouse and shoots when mouse pressed
        timeUntilSpawn += (float)delta;
        if (manualMode)
        {
            // orient tower towards mouse (do not move the tower)
            var mousePos = GetGlobalMousePosition();
            var towerGfx = GetNodeOrNull<Node2D>("TowerGfx");
            if (towerGfx != null)
                towerGfx.LookAt(mousePos);
            else
                LookAt(mousePos);

            // shoot when left mouse pressed and respects spawn rate
            if (Input.IsMouseButtonPressed(MouseButton.Left) && timeUntilSpawn > spawnRate)
            {
                TryShootAtManualPosition(mousePos);
                timeUntilSpawn = 0f;
            }
            return;
        }

        // default automatic behavior
        if (timeUntilSpawn > spawnRate)
        {
            TryShootAtNearestEnemy();
            timeUntilSpawn = 0f;
        }
    }

    public override void _Input(InputEvent @event)
    {
        // detect left mouse click on tower to open upgrade menu
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            var mousePos = GetGlobalMousePosition();
            // if manual mode active and user clicked the tower itself -> disable manual (back to auto)
            if (manualMode && mousePos.DistanceTo(GlobalPosition) <= ClickRadius)
            {
                SetManualMode(false);
                try { GetViewport().SetInputAsHandled(); } catch { }
                return;
            }

            // if manual mode active (but not clicking tower), handle manual shot
            if (manualMode)
            {
                TryShootAtManualPosition(mousePos);
                try { GetViewport().SetInputAsHandled(); } catch { }
                return;
            }

            // if not manual mode, open upgrade menu when clicking the tower
            if (mousePos.DistanceTo(GlobalPosition) <= ClickRadius)
            {
                OpenUpgradeMenu();
                // accept here so the click doesn't trigger placement elsewhere
                try { GetViewport().SetInputAsHandled(); } catch { }
            }
        }
    }

    public void SetManualMode(bool on)
    {
        manualMode = on;
        // set global flag so GameManager doesn't treat clicks as placement while manual controlling
        GameState.ManualControlActive = on;
        GD.Print($"Tower {Name} manualMode set to: {on}");
        // optionally close the upgrade menu when entering manual mode
        if (on && upgradeMenuInstance != null && IsInstanceValid(upgradeMenuInstance))
        {
            upgradeMenuInstance.QueueFree();
            upgradeMenuInstance = null;
        }
    }

    public bool IsInManualMode()
    {
        return manualMode;
    }

    private void TryShootAtNearestEnemy()
    {
        if (Spawner == null || BulletScene == null)
            return;

        Node2D nearestEnemy = null;
        float nearestDist = float.MaxValue;
        foreach (var node in GetTree().GetNodesInGroup("Enemies"))
        {
            if (node is Node2D enemyNode)
            {
                float dist = GlobalPosition.DistanceTo(enemyNode.GlobalPosition);
                if (dist < nearestDist && dist <= AttackRange)
                {
                    nearestDist = dist;
                    nearestEnemy = enemyNode;
                }
            }
        }

        if (nearestEnemy == null)
            return;

        GD.Print($"Tower {Name} shooting at {nearestEnemy.Name} (dist={nearestDist})");

        // Nur TowerGfx schaut den nächsten Gegner an
        var towerGfx = GetNodeOrNull<Node2D>("TowerGfx");
        if (towerGfx != null)
        towerGfx.LookAt(nearestEnemy.GlobalPosition);

        // Use pooled bullet when available to reduce allocations
    Bullet bullet = BulletPool.GetBullet(BulletScene);
    GD.Print($"BulletPool.GetBullet returned: {bullet}");
        if (bullet != null)
        {
            // ensure bullet is parented to the scene root (remove from pool root if necessary)
            var root = GetTree().Root as Node;
            if (bullet.GetParent() != root)
            {
                var oldParent = bullet.GetParent();
                oldParent?.RemoveChild(bullet);
                root.AddChild(bullet);
            }
            // initialize
            bullet.ResetForReuse(Spawner.GlobalPosition, nearestEnemy, BulletScene);
            bullet.Speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
        }
        else
        {
            // fallback to instantiation if pool returned null
            var bulletInstance = BulletScene.Instantiate();
            if (bulletInstance is Bullet b)
            {
                b.GlobalPosition = Spawner.GlobalPosition;
                b.Target = nearestEnemy;
                b.Speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
                GetTree().Root.AddChild(b);
            }
            else if (bulletInstance is Node2D bulletNode)
            {
                GD.PrintErr("BulletScene ist nicht vom Typ 'Bullet'.");
                GetTree().Root.AddChild(bulletNode);
            }
            else
            {
                GD.PrintErr("BulletScene Root ist nicht vom Typ 'Node2D'.");
            }
        }
    }

    private void TryShootAtManualPosition(Vector2 mousePos)
    {
        // find nearest enemy to the provided mouse position within ManualAimRadius
        Node2D nearest = null;
        float nearestDist = float.MaxValue;
        foreach (var node in GetTree().GetNodesInGroup("Enemies"))
        {
            if (node is Node2D e)
            {
                float d = mousePos.DistanceTo(e.GlobalPosition);
                if (d < nearestDist && d <= ManualAimRadius)
                {
                    nearestDist = d;
                    nearest = e;
                }
            }
        }

        if (nearest == null)
        {
            // no enemy near clicked position -> do nothing (could add arc-shot behavior here)
            GD.Print($"Manual shot: no enemy within {ManualAimRadius} of {mousePos}");
            return;
        }

        // spawn bullet targeting the selected enemy (use pool)
        if (Spawner == null || BulletScene == null)
            return;

        Bullet bullet = BulletPool.GetBullet(BulletScene);
        if (bullet != null)
        {
            var root = GetTree().Root as Node;
            if (bullet.GetParent() != root)
            {
                var oldParent = bullet.GetParent();
                oldParent?.RemoveChild(bullet);
                root.AddChild(bullet);
            }
            bullet.ResetForReuse(Spawner.GlobalPosition, nearest, BulletScene);
            bullet.Speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
        }
        else
        {
            var bulletInstance = BulletScene.Instantiate();
            if (bulletInstance is Bullet b)
            {
                b.GlobalPosition = Spawner.GlobalPosition;
                b.Target = nearest;
                b.Speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
                GetTree().Root.AddChild(b);
            }
        }
    }

    private void OpenUpgradeMenu()
    {
        // if menu already open, bring to front
        if (upgradeMenuInstance != null && IsInstanceValid(upgradeMenuInstance))
        {
            upgradeMenuInstance.ZIndex = 100; // try to bring to front
            return;
        }

        var menuScene = GD.Load<PackedScene>("res://Tower_Upgrade_Menu.tscn");
        if (menuScene == null)
        {
            GD.PrintErr("Tower_Upgrade_Menu.tscn not found at expected path");
            return;
        }

        var inst = menuScene.Instantiate();
        if (!(inst is Node2D menuNode))
        {
            GD.PrintErr("Tower_Upgrade_Menu root is not a Node2D");
            return;
        }

        // position menu to the right of tower (adjust as needed)
        menuNode.GlobalPosition = GlobalPosition + new Vector2(64, 0);

        // pause the game logic (use global flag) so only the menu is interactive
        GameState.Paused = true;

        // add to scene root so UI is visible
        var root = GetTree().Root as Node;
        root.AddChild(menuNode);
        upgradeMenuInstance = menuNode;

        // set the ManualButton.TargetTower so the button can toggle manual mode
        var manualBtn = menuNode.GetNodeOrNull<ManualButton>("Manual_Button");
        if (manualBtn != null)
        {
            manualBtn.TargetTower = this;
            // update button text to reflect current state
            manualBtn.Text = manualMode ? "Manual: ON" : "Manual: OFF";
        }

    // menu opened; game logic is paused via GameState.Paused = true (see above)
    }

    public void CloseUpgradeMenu()
    {
        if (upgradeMenuInstance != null && IsInstanceValid(upgradeMenuInstance))
        {
            upgradeMenuInstance.QueueFree();
            upgradeMenuInstance = null;
        }
        // unpause game logic
        GameState.Paused = false;
    }

    // (removed PauseMode helper) UI input while menu is open is handled by GameState.Paused and Control nodes.
}

