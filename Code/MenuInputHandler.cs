using Godot;
using System;

// Helper node attached to runtime menus so they can receive input while the SceneTree is paused.
public partial class MenuInputHandler : Node
{
    public Tower OwnerTower { get; set; }
    public Node2D MenuNode { get; set; }

    public override void _Ready()
    {
        // Ensure this node receives input
        SetProcessInput(true);
    }

    public override void _Input(InputEvent @event)
    {
        // close menu on ui_esc and restore unpause via owner
        if (@event is InputEventKey key && Input.IsActionJustPressed("ui_esc"))
        {
            if (OwnerTower != null)
                OwnerTower.CloseUpgradeMenu();
        }
    }
}
