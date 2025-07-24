using Godot;
using System;

public partial class CommandCenter : Node2D
{
    private LineEdit commandInput;
    private Button executeButton;

    public override void _Ready()
    {
        commandInput = GetNode<LineEdit>("CommandInput");
    }
}
