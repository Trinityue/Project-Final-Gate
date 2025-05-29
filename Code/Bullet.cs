using Godot;
using System;

public partial class Bullet : RigidBody2D
{
    [Export] public Node2D[] PathNodes;
    [Export] public float Speed = 100f;

    private int currentTarget = 0;

    public override void _PhysicsProcess(double delta)
    {
        if (PathNodes == null || PathNodes.Length == 0)
            return;

        if (currentTarget >= PathNodes.Length)
        {
            QueueFree(); // Bullet disappears
            return;
        }

        Vector2 target = PathNodes[currentTarget].GlobalPosition;
        Vector2 direction = (target - GlobalPosition).Normalized();
        // Für RigidBody2D: LinearVelocity statt GlobalPosition direkt setzen
        LinearVelocity = direction * Speed;

        if (GlobalPosition.DistanceTo(target) < 5f)
        {
            currentTarget++;
        }
    }
}