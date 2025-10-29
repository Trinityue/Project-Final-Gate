using Godot;
using System;

public partial class Bullet : Node2D
{
    

    [Export] public float Speed = 300f;
    [Export] public float Damage = 10f;
    [Export] public float Armourpircing = 50f;
    public Node2D target; // Kann Enemy oder EnemyII sein

    public Node2D Target { get; set; }
    // optional: keep reference to the PackedScene that produced this bullet so we can return to the correct pool
    public PackedScene OriginScene { get; set; }


    public override void _Ready()
    {
        AddToGroup("Bullets");
        // Target sollte vom Tower gesetzt werden. Falls nicht, versuchen wir einmalig eine Suche.
        if (Target == null)
        {
            target = GetNearestEnemy();
            if (target == null)
            {
                // kein Ziel verfügbar -> zurück in Pool
                BulletPool.ReturnBullet(this, OriginScene);
                return;
            }
            else
            {
                Target = target;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameState.Paused) return;

        if (Target == null || !IsInstanceValid(Target))
        {
            BulletPool.ReturnBullet(this, OriginScene);
            return;
        }

        Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
        Rotation = (Target.GlobalPosition - GlobalPosition).Angle(); // Bullet zeigt zum Ziel
        GlobalPosition += direction * Speed * (float)delta;

        // Verschwinde, wenn Ziel erreicht
        if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 10f)
        {
            // Call TakeDamage directly on known enemy types to avoid reflection
            if (Target is Enemy enemy)
            {
                enemy.TakeDamage(Damage, Armourpircing > enemy.Enemy_1_Armour, Armourpircing);
            }
            else if (Target is EnemyIi enemyIi)
            {
                enemyIi.TakeDamage(Damage, Armourpircing > enemyIi.Enemy_2_Armour, Armourpircing);
            }
            else
            {
                // Fallback: try reflection once (very rare)
                var method = Target.GetType().GetMethod("TakeDamage");
                if (method != null)
                    method.Invoke(Target, new object[] { Damage });
            }
            // return to pool instead of freeing
            BulletPool.ReturnBullet(this, OriginScene);
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

    // Called by pool when this bullet is reused
    public void ResetForReuse(Vector2 position, Node2D newTarget, PackedScene originScene)
    {
        GlobalPosition = position;
        Target = newTarget;
        OriginScene = originScene;
        Show();
        SetProcess(true);
        SetPhysicsProcess(true);
    }
}