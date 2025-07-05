using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class TextureButton : Godot.TextureButton
{
    public void _on_TextureButton_pressed()
    { 
        var AudioStreamPlayer2D = GetNode<Buttonclicksound>("Buttonclicksound");  
        AudioStreamPlayer2D.Play();
        GetTree().ChangeSceneToFile("res://node_2d.tscn");
    }
}
