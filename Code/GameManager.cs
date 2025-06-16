using Godot;
using System;
using System.ComponentModel;

public partial class GameManager : Node2D
{
    [Export] public PackedScene TowerScene;
    [Export] public int TowerCost = 50;
    [Export] public float Player_Health = 100f;


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            if (MoneyManagment.Instance.CanAfford(TowerCost))
            {
                // Tower platzieren
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


    // alles hier ist nur debbuging maybe ist hier auch n fehler sollte aber nicht 
    public override void _Process(double delta)
    {
        GD.Print($"Player Health: {Player_Health}");
        if (Player_Health <= 0)
        {
            GetTree().ChangeSceneToFile("res://Game-Over_Screen.tscn"); // Pfad ggf. anpassen!
        }
    } 
}