using Godot;
using System;

public partial class Tower : Node2D
{
    [Export] public PackedScene BulletScene;
    [Export] public Node2D Spawner;
    [Export] public float bps = 1f; // bullets per sec 
    [Export] public Node2D[] PathNodes; // Ziehe im Editor deine Wegpunkte rein
    [Export] public float MinSpeed = 80f;   // Minimal einstellbare Geschwindigkeit
    [Export] public float MaxSpeed = 150f;  // Maximal einstellbare Geschwindigkeit
    [Export] public float AttackRange = 100f;     // Schussreichweite

    private float spawn_rate;
    private float tus = 0; // time until spawn

    public override void _Ready()
    {
        spawn_rate = 1 / bps;
    }

    public override void _Process(double delta)
    {
        if (tus > spawn_rate)
        {
            Spawn();
            tus = 0;
        }
        else
        {
            tus += (float)delta;
        }

        // Enemies im Umkreis hervorheben
        foreach (var enemy in GetTree().GetNodesInGroup("Enemies"))
        {
            if (enemy is Enemy e && GlobalPosition.DistanceTo(e.GlobalPosition) < AttackRange)
            {
                e.FlashColor(Colors.Red, 0.2f);
            }
        }
    }

    private void Spawn()
    {
        if (Spawner == null || BulletScene == null)
            return;

        // Nächsten Enemy suchen
        Enemy nearestEnemy = null;
        float nearestDist = float.MaxValue;
        foreach (var node in GetTree().GetNodesInGroup("Enemies"))
        {
            if (node is Enemy e)
            {
                float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestEnemy = e;
                }
            }
        }

        if (nearestEnemy == null)
            return; // Kein Ziel

        // Bullet instanziieren
        var bulletInstance = BulletScene.Instantiate();
        if (bulletInstance is Bullet bullet)
        {
            bullet.GlobalPosition = Spawner.GlobalPosition;
            bullet.Target = nearestEnemy;
            bullet.Speed = (float)GD.RandRange(MinSpeed, MaxSpeed);

            GetTree().Root.AddChild(bullet);
        }
    }
}