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

    public void AddMoney(int amount)
    {
        Money += amount;
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        var label = GetNodeOrNull<Label>("/root/Node2D/MoneyManagment/MoneyLabel");
        if (label != null)
            label.Text = $"{Money:0}";
    }

    public override void _Process(double delta)
    {
        Money += MoneyPerSecond * (float)delta;
        UpdateLabel();
    }
}
