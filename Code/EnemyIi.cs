using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public partial class EnemyIi : Node2D
{
    [Export] public float Speed = 100f; // Geschwindigkeit des Enemies
    public Path2D Path; // Wird vom Spawner gesetzt
    private PathFollow2D pathFollow; // PathFollow2D für die Bewegung des Enemies

    [Export] public float Health = 100f; // Lebenspunkte
    [Export] public float MaxHealth = 100f; // Maximale Lebenspunkte 
    [Export] public float Damage = 10f; // Schaden, den dieser Enemy verursacht
    [Export] public float Money_Given = 1.0f; // Geld, das der Spieler erhält, wenn dieser Enemy besiegt wird
    [Export] public float Healing = 10f; // Heilung 
    private Stopwatch stopwatch = new Stopwatch();



    public override void _Ready()
    {
        var timer = new Timer();
        timer.WaitTime = 5.0f;      // Timer läuft 5 Sekunden
        timer.OneShot = true;       // Timer läuft nur einmal (kein Loop)
        AddChild(timer);            // Timer als Kindnode hinzufügen
        timer.Timeout += OnTimerTimeout; // Event/Signal verbinden
        timer.Start();              // Timer starten

        AddToGroup("Enemies");
        pathFollow = new PathFollow2D();
        pathFollow.Loop = false;
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

        if (pathFollow.ProgressRatio >= 1.0f)
        {
            var gameManager = GetNode<GameManager>("/root/Node2D/GameManager");
            GD.Print("Enemy end");
            if (gameManager != null)
            {
                if (Health / MaxHealth <= 0.25f)
                {
                    gameManager.Player_Health -= Damage / 8;
                }
                else if (Health / MaxHealth <= 0.5f)
                {
                    gameManager.Player_Health -= Damage / 4;
                }
                else if (Health / MaxHealth <= 0.75f)
                {
                    gameManager.Player_Health -= Damage / 2; ;
                }
                else if (Health / MaxHealth <= 1.0f)
                {
                    gameManager.Player_Health -= Damage;
                }
            }
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
        private void OnTimerTimeout()
    {
        GD.Print("Timer done! Healing precceedign");
        if (Health < MaxHealth)
        {
            Health += Healing;
        }
        else if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }
        else if (Health == MaxHealth)
        {
        }

        
    }
}


