using Godot;
using System;

public partial class StartScreen : Node2D

{

    [Export] private bool Fullscreen = false;


    public override void _Draw()
    {
        var mode = DisplayServer.WindowGetMode();
        if (mode == DisplayServer.WindowMode.Fullscreen)
        {
            Fullscreen = true; 
        }
    }


    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_f1") && Fullscreen == false)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            Fullscreen = true;
        }
        else if (Input.IsActionJustPressed("ui_f1") && Fullscreen == true)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            Fullscreen = false;
        }

    }
}