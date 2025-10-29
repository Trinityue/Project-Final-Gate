using Godot;
using System.Collections.Generic;

public static class EnemyPool
{
    private static readonly Dictionary<string, Queue<Node2D>> pools = new();

    public static Node2D GetEnemy(PackedScene scene)
    {
        if (scene == null) return null;
        string key = scene.ResourcePath ?? "__enemy";
        if (!pools.TryGetValue(key, out var q))
        {
            q = new Queue<Node2D>();
            pools[key] = q;
        }
        if (q.Count > 0)
        {
            var e = q.Dequeue();
            e.Show();
            e.SetProcess(true);
            e.SetPhysicsProcess(true);
            return e;
        }
        var inst = scene.Instantiate();
        if (inst is Node2D nd) return nd;
        return null;
    }

    public static void ReturnEnemy(Node2D e, PackedScene scene = null)
    {
        if (e == null) return;
        e.SetProcess(false);
        e.SetPhysicsProcess(false);
        e.Hide();
        string key = scene?.ResourcePath ?? "__enemy";
        if (!pools.TryGetValue(key, out var q))
        {
            q = new Queue<Node2D>();
            pools[key] = q;
        }
        q.Enqueue(e);
    }
}
