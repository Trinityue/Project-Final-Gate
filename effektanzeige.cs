using Godot;
using System;


public partial class Enemy : CharacterBody2D
{
    public void FlashColor(Color color, float duration)
    {
        var sprite = GetNode<Sprite2D>("Sprite2D");
        sprite.Modulate = color;
        GetTree().CreateTimer(duration).Timeout += () => sprite.Modulate = Colors.White;
    }
}