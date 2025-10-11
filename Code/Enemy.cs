using Godot;
using System;


public partial class Enemy : Node2D
{
    [Export] public float Speed;     public Path2D Path; // Wird vom Spawner gesetzt
    private PathFollow2D pathFollow; // PathFollow2D für die Bewegung des Enemies

    [Export] public float Health = 100f; // Lebenspunkte
    [Export] public float MaxHealth = 100f; // Maximale Lebenspunkte 
    [Export] public float Damage = 10f; // Schaden, den dieser Enemy verursacht
    [Export] public float Money_Given = 1.0f; // Geld, das der Spieler erhält, wenn dieser Enemy besiegt wird
    [Export] public float Enemy_1_Armour = 50f; // Rüstung des Enemys, reduziert den erlittenen Schaden
    
    // Caches to avoid repeated GetNode calls
    private MoneyManagment moneyManagment;
    private GameManager gameManager;

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
        // Cache commonly used singletons/nodes
        moneyManagment = GetNodeOrNull<MoneyManagment>("/root/Node2D/MoneyManagment");
        gameManager = GetNodeOrNull<GameManager>("/root/Node2D/GameManager");
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

    // New signature: allow caller (Bullet) to indicate if the hit was armour-piercing
    public void TakeDamage(float amount, bool armourPiercing = false, float attackerArmourPiercing = 0f)
    {
        if (armourPiercing)
        {
            amount *= 2f;
            GD.Print("Armourpiercing!");
        }
        else
        {
            amount *= 0.5f;
            // reduce armour slightly based on attacker's armour value
            if (attackerArmourPiercing > 0f)
                Enemy_1_Armour -= attackerArmourPiercing / 2f;
        }

        Health -= amount;
        GD.Print($"Enemy took {amount} damage, remaining health: {Health}");
        if (Health <= 0)
        {
            if (moneyManagment != null)
            {
                moneyManagment.Money += Money_Given; // Geld hinzufügen
            }
            else
            {
                // fallback if cache failed
                var mm = GetNodeOrNull<MoneyManagment>("/root/Node2D/MoneyManagment");
                if (mm != null) mm.Money += Money_Given;
            }
            GD.Print($"Enemy defeated! Money given: {Money_Given}");
            QueueFree(); 
        }
    }
}
