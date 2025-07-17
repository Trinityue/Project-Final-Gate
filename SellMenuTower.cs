using Godot;
using System;

public partial class SellMenuTower : MenuButton
{
    public override void _Ready()
    {
        var popup = GetPopup();
        var Sell_Item = GD.Load<Texture2D>("res://Sell_Button.png.png");
        popup.AddIconItem(Sell_Item, "Sell", 0); // Icon und Text anzeigen
        popup.IdPressed += OnMenuItemPressed; // Signal verbinden
    }

    private void OnMenuItemPressed(long id)
    {
        if (id == 0)
        {
            GD.Print("Sell Tower");
            // Geld hinzufügen
            var moneyManagment = GetNodeOrNull<MoneyManagment>("/root/Node2D/MoneyManagment");
            if (moneyManagment != null) {
                moneyManagment.AddMoney(30);
                GD.Print("30 Geld hinzugefügt");
            } else {
                GD.Print("MoneyManagment NICHT gefunden!");
            }

            // Tower entfernen
            GetParent().QueueFree();
        }
    }
}
