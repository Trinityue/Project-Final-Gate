using Godot;
using System;

public partial class TextureButton : Godot.TextureButton
{
    public void _on_TextureButton_pressed()
    {
        GetTree().ChangeSceneToFile("res://node_2d.tscn");
    }
}
