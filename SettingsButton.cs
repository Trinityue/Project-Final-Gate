using Godot;
using System;

public partial class SettingsButton : TextureButton
{
    public void pressed_settings()
    {
        GD.Print("Settings Button pressed");
        GetTree().ChangeSceneToFile("res://settings_menu.tscn");
    }
}
