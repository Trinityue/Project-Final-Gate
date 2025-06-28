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

    public override  void _Ready()
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

        var bulletInstance = BulletScene.Instantiate();
        if (bulletInstance is Bullet bullet)
        {
            bullet.GlobalPosition = Spawner.GlobalPosition;
            bullet.Target = nearestEnemy;
            bullet.Speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
            GetTree().Root.AddChild(bullet);
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

