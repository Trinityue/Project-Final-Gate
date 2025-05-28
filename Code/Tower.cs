using Godot;
using System;
using System.IO;

public partial class Tower : Node2D
{
    [Export] PackedScene bulletScene;
    [Export] public float bullet_speed = 500f; // Speed of the bullet
    [Export] public float bps = 3f; // Bullets per second
    [Export] public float bullet_damage = 10f; // Damage of the bullet

    float fire_rate;
    float time_until_fire = 0f; // Time until the next bullet can be fired

    public override void _Ready()
    {
        fire_rate = 1f / bps; // Calculate fire rate based on bullets per second
    }

    public override void _Process(double delta)
    {
        time_until_fire += (float)delta;

        if (time_until_fire > fire_rate)
        {
            // Nächsten Enemy finden
            Enemy nearestEnemy = GetNearestEnemy();
            if (nearestEnemy == null)
                return;

            RigidBody2D bullet = bulletScene.Instantiate<RigidBody2D>();
            bullet.Position = GlobalPosition;

            // Richtung zum Enemy berechnen
            Vector2 direction = (nearestEnemy.GlobalPosition - GlobalPosition).Normalized();
            bullet.LinearVelocity = direction * bullet_speed;

            GetTree().Root.AddChild(bullet);

            time_until_fire = 0f;
        }
    }

    // Hilfsmethode, um den nächsten Enemy zu finden
    private Enemy GetNearestEnemy()
    {
        Enemy nearest = null;
        float minDist = float.MaxValue;

        foreach (var node in GetTree().GetNodesInGroup("Enemies"))
        {
            if (node is Enemy enemy)
            {
                float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }
        }
        return nearest;
    }
}