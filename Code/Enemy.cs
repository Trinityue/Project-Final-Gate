using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Export] public Node2D[] PathNodes; // Im Editor: 2D-Nodes reinziehen
    [Export] public float Speed = 100f;
    [Export] public float Health = 100f; //Healthsystem 

    public int currentTarget = 0;

    public override void _Ready()
    {
        AddToGroup("Enemies");
    }

    public override void _Process(double delta)
    {
        if (PathNodes == null || PathNodes.Length == 0)
            return;

        // Wenn letzter Waypoint erreicht, nicht mehr weiterlaufen
        if (currentTarget >= PathNodes.Length)
        {
            QueueFree(); // Enemy verschwindet
            return;
        }

        Vector2 target = PathNodes[currentTarget].GlobalPosition;
        Vector2 direction = (target - GlobalPosition).Normalized();
        GlobalPosition += direction * Speed * (float)delta;

        if (GlobalPosition.DistanceTo(target) < 5f)
        {
            currentTarget++;
        }
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
