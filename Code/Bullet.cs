using Godot;
using System;

public partial class Bullet : Node2D
{
    [Export] public Node2D[] PathNodes; // <-- Diese Zeile hinzufügen!
    [Export] public Enemy Target; // Ziel-Enemy

    [Export] public float Speed = 300f;

    public override void _PhysicsProcess(double delta)
    {
        if (Target == null || !IsInstanceValid(Target))
        {
            QueueFree();
            return;
        }

        Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
        GlobalPosition += direction * Speed * (float)delta;

        // Optional: Bullet verschwindet, wenn sie sehr nah am Enemy ist
        if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 10f)
        {
            QueueFree();
        }
    }
}