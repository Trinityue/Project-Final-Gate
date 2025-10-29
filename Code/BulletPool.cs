using Godot;
using System;
using System.Collections.Generic;

public static class BulletPool
{
    private static readonly Dictionary<string, Queue<Bullet>> pools = new();
    private static Node2D poolRoot;

    private static Node2D GetPoolRoot()
    {
        if (poolRoot != null) return poolRoot;
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree == null) return null;
        var root = tree.Root as Node;
        poolRoot = root.GetNodeOrNull<Node2D>("_BulletPoolRoot");
        if (poolRoot == null)
        {
            poolRoot = new Node2D();
            poolRoot.Name = "_BulletPoolRoot";
            root.AddChild(poolRoot);
            poolRoot.Visible = false;
        }
        return poolRoot;
    }

    public static Bullet GetBullet(PackedScene scene)
    {
        if (scene == null) return null;
        string key = scene.ResourcePath ?? scene.GetType().FullName;
        if (!pools.TryGetValue(key, out var q))
        {
            q = new Queue<Bullet>();
            pools[key] = q;
        }

        if (q.Count > 0)
        {
            var b = q.Dequeue();
            if (b == null) return InstantiateBullet(scene);
            var root = GetPoolRoot();
            if (b.GetParent() == root)
                root.RemoveChild(b);
            b.Show();
            b.SetProcess(true);
            b.SetPhysicsProcess(true);
            return b;
        }
        return InstantiateBullet(scene);
    }

    private static Bullet InstantiateBullet(PackedScene scene)
    {
        var inst = scene.Instantiate();
        if (inst is Bullet b)
        {
            return b;
        }
        else if (inst is Node2D node)
        {
            // try to find a Bullet script on the node or children
            var bullet = node.GetNodeOrNull<Bullet>(".");
            if (bullet != null) return bullet;
            // fallback: look for first child with Bullet
            foreach (Node child in node.GetChildren())
            {
                if (child is Bullet cb) return cb;
            }
            GD.PrintErr($"InstantiateBullet: PackedScene did not contain a Bullet instance: {scene.ResourcePath}");
        }
        return null;
    }

    public static void ReturnBullet(Bullet b, PackedScene originalScene = null)
    {
        if (b == null) return;
        // stop processing and hide
        b.SetProcess(false);
        b.SetPhysicsProcess(false);
        b.Hide();
        b.Target = null;

        var root = GetPoolRoot();
        if (root != null && b.GetParent() != root)
        {
            // reparent into pool root to keep scene tree tidy
            var parent = b.GetParent();
            parent?.RemoveChild(b);
            root.AddChild(b);
        }

        // enqueue into pool by scene path if provided
        string key = originalScene?.ResourcePath ?? "__generic_bullet";
        if (!pools.TryGetValue(key, out var q))
        {
            q = new Queue<Bullet>();
            pools[key] = q;
        }
        q.Enqueue(b);
    }
}
