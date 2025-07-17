using Godot;
using System;

public partial class MoneyLabel : Label
{
    public override void _Ready()
    {
        // Beispiel mit Standard-Font und Größe
        AddThemeFontSizeOverride("font_size", 37); // Schriftgröße anpassen
    }
}
