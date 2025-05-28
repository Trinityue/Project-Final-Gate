using Godot;
using System;

public partial class Bullet : RigidBody2D
{
    public Enemy Target;
    public float Speed = 500f;

    public override void _PhysicsProcess(double delta)
    {
        if (Target == null || !IsInstanceValid(Target))
        {
            QueueFree();
            return;
        }

        Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
        LinearVelocity = direction * Speed;
    }
}