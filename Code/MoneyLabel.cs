using Godot;
using System;

public partial class MoneyLabel : Label
{
    private GameManager gameManager;

    public override void _Ready()
    {
        gameManager = GetNode<GameManager>("/root/Node2D/GameManager");
    }

    public override void _Process(double delta)
    {
        if (gameManager != null)
        {
            Text = gameManager.Player_Health.ToString();
        }
    }
}
