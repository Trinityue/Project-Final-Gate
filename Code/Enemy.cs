using Godot;
using System;


public partial class Enemy : Node2D
{
    [Export] public float Speed = 100f; // Geschwindigkeit des Enemies
    public Path2D Path; // Wird vom Spawner gesetzt
    private PathFollow2D pathFollow; // PathFollow2D für die Bewegung des Enemies

    [Export] public float Health = 100f; // Lebenspunkte
    [Export] public float MaxHealth = 100f; // Maximale Lebenspunkte 
    [Export] public float Damage = 10f; // Schaden, den dieser Enemy verursacht
    [Export] public float Money_Given = 1.0f; // Geld, das der Spieler erhält, wenn dieser Enemy besiegt wird

    public override void _Ready()
    {
        AddToGroup("Enemies");
        pathFollow = new PathFollow2D();
        pathFollow.Loop = false; // <<< Das verhindert das Zurückspringen!
        if (Path != null)
        {
            Path.AddChild(pathFollow);
            pathFollow.Progress = 0;
            GlobalPosition = pathFollow.GlobalPosition;
        }
    }

    public override void _Process(double delta)
    {
        if (pathFollow == null || Path == null)
            return;

        pathFollow.Progress += Speed * (float)delta;
        GlobalPosition = pathFollow.GlobalPosition;

        // Das problem ist hier das er in der ersten zeile beim if nicht werkennt das der enemy am ende des Path ist. 
        //  Idk why weil er ja verschwindet
        // ich lass das als placeholder drinne 
        if (pathFollow.ProgressRatio >= 1.0f)
        {
            var gameManager = GetNode<GameManager>("/root/Node2D/GameManager");
            GD.Print("Enemy end");
            if (gameManager != null)
            {
                gameManager.Player_Health -= Damage;
            }
            // ende placeholder
            QueueFree();
        }
    }

    public void TakeDamage(float amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            var moneyManagment = GetNode<MoneyManagment>("/root/Node2D/MoneyManagment");
            if (moneyManagment != null)
            {
                moneyManagment.Money += Money_Given; // Geld hinzufügen
            }
            GD.Print($"Enemy defeated! Money given: {Money_Given}");
            QueueFree(); 
        }
    }
}
