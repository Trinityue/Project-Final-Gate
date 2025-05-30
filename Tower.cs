using Godot;
using System;

public partial class Tower : Node2D
{
    public float Range = 100f;

    public override void _Process(double delta)
    {
        foreach (var enemy in GetTree().GetNodesInGroup("Enemies"))
        {
            if (enemy is Enemy e && GlobalPosition.DistanceTo(e.GlobalPosition) < Range)
            {
                e.FlashColor(Colors.Red, 0.2f);
            }
        }
    }
}