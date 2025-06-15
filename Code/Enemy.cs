using Godot;
using System;


public partial class Enemy : Node2D
{
    [Export] public float Speed = 100f; // Geschwindigkeit des Enemies
    public Path2D Path; // Wird vom Spawner gesetzt
    private PathFollow2D pathFollow; // PathFollow2D für die Bewegung des Enemies

    [Export] public float Health = 100f; // Lebenspunkte
    [Export] public float MaxHealth = 100f; // Maximale Lebenspunkte
    [Export] public int MoneyValue = 10; // Geldwert für diesen Enemy
    [Export] public float Damage = 10f; // Schaden, den dieser Enemy verursacht
    [Export] public float Money_Given = 10f; // Geld, das der Spieler erhält, wenn dieser Enemy besiegt wird
    public override void _Ready()
    {
        // PathFollow2D erzeugen und an den Pfad hängen
        AddToGroup("Enemies"); // Enemy zur Gruppe "Enemies" hinzufügen
        pathFollow = new PathFollow2D();
        if (Path != null)
        {
            Path.AddChild(pathFollow);
            pathFollow.Progress = 0;
            GlobalPosition = pathFollow.GlobalPosition;
        }
        else
        {   // Zugriff auf GameManager und Leben reduzieren
            var gameManager = (GameManager) GetNode("/root/GameManager");
            if (Health > 100)
            {
                gameManager.Player_Health -= 20;
            }
            if (Health > 75)
            {
                gameManager.Player_Health -= 15;
            }
            else if (Health > 50)
            {
                gameManager.Player_Health -= 10;
            }
            else if (Health > 25)
            {
                gameManager.Player_Health -= 5;
            }
            gameManager.Player_Health -= 25;
 
        }
    }

    public override void _Process(double delta)
    {
        if (pathFollow == null || Path == null)
            return;

        pathFollow.Progress += Speed * (float)delta;

        GlobalPosition = pathFollow.GlobalPosition;

        // Enemy entfernen, wenn das Ende erreicht ist
        if (pathFollow.ProgressRatio >= 1.0)
        {
            QueueFree();
        }
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            QueueFree(); // Enemy verschwindet bei 0 HP
        }
    }
}
