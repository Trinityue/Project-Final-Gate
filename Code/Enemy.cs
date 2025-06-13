using Godot;
using System;

public partial class Enemy : Node2D
{
    [Export] public float Speed = 100f;
    public Path2D Path; // Wird vom Spawner gesetzt
    private PathFollow2D pathFollow;

    [Export] public float Health = 100f; // Lebenspunkte

    public override void _Ready()
    {
        // PathFollow2D erzeugen und an den Pfad hängen
        AddToGroup("Enemies"); // Enemy zur Gruppe "Enemies" hinzufügen
        pathFollow = new PathFollow2D();
        if (Path != null)
        {
            Path.AddChild(pathFollow);
            pathFollow.Progress = 0;
            GlobalPosition = pathFollow.GlobalPosition;
        }
        else
        {
            QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        if (pathFollow == null || Path == null)
            return;

        pathFollow.Progress += Speed * (float)delta;

        GlobalPosition = pathFollow.GlobalPosition;

        // Enemy entfernen, wenn das Ende erreicht ist
        if (pathFollow.ProgressRatio >= 1.0)
            QueueFree();
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            QueueFree(); // Enemy verschwindet bei 0 HP
        }
    }
}
