using Godot;
using System;

public partial class Reset_Button : TextureButton
{
        public void _on_pressedA()
    { 
        GetTree().ChangeSceneToFile("res://node_2d.tscn");
    }
}
