using Godot;
using System;

public partial class Tower : Node2D
{
    [Export] public PackedScene BulletScene;
    [Export] public Node2D Spawner;
    [Export] public float bps = 1f; // bullets per second
    [Export] public float MinSpeed = 80f;
    [Export] public float MaxSpeed = 150f;
    [Export] public float AttackRange = 100f;

    private float spawnRate;
    private float timeUntilSpawn = 0f;

    public override void _Ready()
    {
        spawnRate = 1f / bps;
    }

    public override void _Process(double delta)
    {
        timeUntilSpawn += (float)delta;
        if (timeUntilSpawn > spawnRate)
        {
            TryShootAtNearestEnemy();
            timeUntilSpawn = 0f;
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

    private void TryShootAtNearestEnemy()
    {
        if (Spawner == null || BulletScene == null)
            return;

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
            return;

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