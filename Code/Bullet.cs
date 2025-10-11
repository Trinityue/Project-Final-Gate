using Godot;
using System;

public partial class Bullet : Node2D
{
    

    [Export] public float Speed = 300f;
    [Export] public float Damage = 10f;
    [Export] public float Armourpircing = 50f;
    public Node2D target; // Kann Enemy oder EnemyII sein

    public Node2D Target { get; set; }


    public override void _Ready()
    {
        AddToGroup("Bullets");
        // Suche das nächste Ziel beim Erzeugen
        target = GetNearestEnemy();
        if (target == null)
            QueueFree();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (target == null || !IsInstanceValid(target))
        {
            QueueFree();
            return;
        }

        Vector2 direction = (target.GlobalPosition - GlobalPosition).Normalized();
        Rotation = (target.GlobalPosition - GlobalPosition).Angle(); // Bullet zeigt zum Ziel
        GlobalPosition += direction * Speed * (float)delta;

        // Verschwinde, wenn Ziel erreicht
        if (GlobalPosition.DistanceTo(target.GlobalPosition) < 10f)
        {
            // Call TakeDamage directly on known enemy types to avoid reflection
            if (target is Enemy enemy)
            {
                enemy.TakeDamage(Damage, Armourpircing > enemy.Enemy_1_Armour, Armourpircing);
            }
            else if (target is EnemyIi enemyIi)
            {
                enemyIi.TakeDamage(Damage, Armourpircing > enemyIi.Enemy_2_Armour, Armourpircing);
            }
            else
            {
                // Fallback: try reflection once (very rare)
                var method = target.GetType().GetMethod("TakeDamage");
                if (method != null)
                    method.Invoke(target, new object[] { Damage });
            }

            QueueFree();
        }
    }

    private Node2D GetNearestEnemy()
    {
        Node2D nearest = null;
        float nearestDist = float.MaxValue;
        foreach (var node in GetTree().GetNodesInGroup("Enemies"))
        {
            if (node is Node2D e)
            {
                float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = e;
                }
            }
        }
        return nearest;
    }
}