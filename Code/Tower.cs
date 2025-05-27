using Godot;
using System;

public partial class Tower : CharacterBody2D
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept")) // z.B. Enter-Taste
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();
            float x = rng.RandfRange(0, 1024); // Passe die Werte an deine Spielfeldgröße an
            float y = rng.RandfRange(0, 768);
            GlobalPosition = new Vector2(x, y);
        }
    }
}

    