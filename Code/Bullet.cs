using Godot;
using System;

public partial class Bullet : Node2D
{
    public Enemy Target; // Ziel-Enemy (kein [Export], da im Code gesetzt)
    [Export] public float Speed = 300f;

    public override void _PhysicsProcess(double delta)
    {
        // Wenn kein Ziel vorhanden oder Ziel nicht mehr gültig, Bullet entfernen
        if (Target == null || !IsInstanceValid(Target))
        {
            QueueFree();
            return;
        }

        // Richtung zum Ziel berechnen und bewegen
        Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
        GlobalPosition += direction * Speed * (float)delta;

        // Bullet verschwindet, wenn sie das Ziel erreicht
        if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 10f)
        {
            QueueFree();
        }
    }
}