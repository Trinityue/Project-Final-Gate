using Godot;
using System;

public partial class GameLabel : Label
{
    private GameManager gameManager;

    public override void _Ready()
    {
        gameManager = GetNode<GameManager>("/root/Node2D/GameManager");
        AddThemeFontSizeOverride("font_size", 37); 
    }

    public override void _Process(double delta)
    {
        if (gameManager != null)
        {
            Text = gameManager.Player_Health.ToString();
        }
    }
}
