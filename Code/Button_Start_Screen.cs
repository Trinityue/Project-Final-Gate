using Godot;
using System;

public partial class Button_Start_Screen : Button
{
    public void pressed()
    {
        GetTree().ChangeSceneToFile("res://node_2d.tscn");
    }
}
