using Godot;
using System;

public partial class ManualButton : Button
{
    [Export] public bool Manual = false;
    // reference to the tower this menu belongs to (set by the Tower when opening the menu)
    public Tower TargetTower { get; set; }

    public override void _Ready()
    {
        // wire the pressed signal if not already wired in the editor
        this.Pressed += OnPressed;
    }

    private void OnPressed()
    {
        if (TargetTower == null)
        {
            GD.PrintErr("ManualButton pressed but TargetTower is null");
            return;
        }

        // Toggle manual mode on the tower
        bool newState = !TargetTower.IsInManualMode();
        TargetTower.SetManualMode(newState);
        Manual = newState;
        Text = Manual ? "Manual: ON" : "Manual: OFF";

        // close the menu when manual mode enabled to avoid placement conflicts
        if (newState)
            TargetTower.CloseUpgradeMenu();
    }
}
