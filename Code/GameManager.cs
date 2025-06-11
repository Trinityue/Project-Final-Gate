using Godot;
using System;

public partial class GameManager : Node2D
{
    [Export] public PackedScene TowerScene;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                // Tower instanziieren und an die Mausposition setzen
                var towerInstance = TowerScene.Instantiate() as Node2D;
                if (towerInstance != null)
                {
                    towerInstance.GlobalPosition = GetGlobalMousePosition();
                    GetTree().Root.AddChild(towerInstance);
                }
            }
        }
    }
}