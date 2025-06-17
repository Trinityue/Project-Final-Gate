using Godot;
using System;

public partial class MoneyManagment : Node2D
{
    [Export] public float Money { get; set; } = 0.0f;
    [Export] public float MoneyPerSecond { get; set; } = 0.0f;

    public static MoneyManagment Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }


    public bool CanAfford(int cost)
    {
        return Money >= cost;
    }

    public void Spend(int cost)
    {
        Money -= cost;
    }
    public override void _Process(double delta)
    {
        GD.Print("Process läuft"); // Debug-Ausgabe
        Money += MoneyPerSecond * (float)delta;
        var label = GetNode<Label>("/root/Node2D/MoneyManagment/MoneyLabel");
        label.Text = $"Geld: {Money:0.00}";

    }

}
