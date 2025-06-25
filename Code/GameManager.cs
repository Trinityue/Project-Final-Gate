using Godot;
using System;

public partial class GameManager : Node2D
{
    [Export] public PackedScene TowerScene;
    [Export] public int TowerCost = 50;
    [Export] public float Player_Health = 100f;
    [Export] public float Player_Max_Health = 100f;

    private bool mouseInNoBuildZone = false;

    public override void _Process(double delta)
    {
        if (Player_Health <= 0)
        {
            GetTree().ChangeSceneToFile("res://Game-Over_Screen.tscn");
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            if (mouseInNoBuildZone)
            {
                GD.Print("Hier darf kein Turm platziert werden!");
                return;
            }

            if (MoneyManagment.Instance.CanAfford(TowerCost))
            {
                var tower = TowerScene.Instantiate() as Node2D;
                tower.GlobalPosition = GetGlobalMousePosition();
                GetTree().Root.AddChild(tower);

                MoneyManagment.Instance.Spend(TowerCost);
            }
            else
            {
                GD.Print("Nicht genug Geld!");
            }
        }
    }

    // Diese Methoden werden von Area2D-Signalen aufgerufen:
    public void OnMouseEntered()
    {
        mouseInNoBuildZone = true;
        GD.Print("Mouse entered NoBuildZone");
    }

    public void OnMouseExited()
    {
        mouseInNoBuildZone = false;
        GD.Print("Mouse exited NoBuildZone");
    }
}

