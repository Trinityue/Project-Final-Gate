using Godot;
using System;
using System.Collections.Generic;
using System.Runtime;

public partial class InfernoTower : Node2D
{
    [Export] public PackedScene BulletScene;
    [Export] public Node2D Spawner;
    // Settings for the inferno damage behavior
    [Export] public int MaxTargets = 2; // up to two enemies
    [Export] public float AttackRange = 500f;
    [Export] public float DamageStart = 5f; // damage at first second
    [Export] public float MaxDamage = 100f; // damage cap after ramp
    [Export] public float RampSeconds = 10f; // seconds until reaching MaxDamage

    // internal
    private Timer damageTimer;
    private Dictionary<Node2D, float> targetSeconds = new(); // seconds each target has been continuously targeted
    // visuals
    [Export] public PackedScene BeamScene;
    [Export] public Texture2D BeamTexture;
    [Export] public Texture2D BeamHeadTexture;
    private Node2D beamsContainer;
    private Dictionary<Node2D, Node2D> targetBeams = new(); // target -> beam node

    public override  void _Ready()
    {
        // create a repeating timer that ticks each second to apply damage
        damageTimer = new Timer();
        damageTimer.WaitTime = 0.5f; // tick every 0.5 seconds
        damageTimer.OneShot = false;
        AddChild(damageTimer);
        damageTimer.Timeout += OnDamageTick;
        damageTimer.Start();
        // create a container for beam visuals
        beamsContainer = new Node2D();
        beamsContainer.Name = "Beams";
        AddChild(beamsContainer);
    }

    public override void _Process(double delta)
    {
        // Optional: visual feedback for enemies in range can be done here
        foreach (var enemy in GetTree().GetNodesInGroup("Enemies"))
        {
            if (enemy is Node2D e && GlobalPosition.DistanceTo(e.GlobalPosition) < AttackRange)
            {
                // e.g. highlight or particle effect
            }
        }

        // update beam visuals each frame so they follow their targets smoothly
        UpdateBeams();
    }

    private void TryShootAtNearestEnemy()
    {
        // This tower now deals direct damage in OnDamageTick, so this method is unused.
        // Left intentionally empty for compatibility.
    }

    private void OnDamageTick()
    {
        // get up to MaxTargets nearest enemies in range
        var enemiesInRange = new List<Node2D>();
        foreach (var node in GetTree().GetNodesInGroup("Enemies"))
        {
            if (node is Node2D e)
            {
                float dist = GlobalPosition.DistanceTo(e.GlobalPosition);
                if (dist <= AttackRange)
                    enemiesInRange.Add(e);
            }
        }

        // sort by distance and take up to MaxTargets
        enemiesInRange.Sort((a, b) => GlobalPosition.DistanceTo(a.GlobalPosition).CompareTo(GlobalPosition.DistanceTo(b.GlobalPosition)));
        var targets = enemiesInRange.GetRange(0, Math.Min(MaxTargets, enemiesInRange.Count));

    // update targetSeconds: increment for current targets, remove others
        var currentTargets = new HashSet<Node2D>(targets);
        // increment seconds for targets
        foreach (var t in targets)
        {
            // ensure beam exists for this target
            if (!targetBeams.ContainsKey(t))
                CreateBeamForTarget(t);

            if (!targetSeconds.ContainsKey(t))
                targetSeconds[t] = 0f;
            // increment by the timer wait time (supports 0.5s ticks)
            targetSeconds[t] += (float)damageTimer.WaitTime;

            // compute damage using exponential ramp: damage = DamageStart * r^(seconds-1)
            float damage = ComputeRampedDamage(targetSeconds[t]);

            // apply damage to known enemy types
            if (t is Enemy e)
            {
                // ignore enemy armour: pass armourPiercing = true
                e.TakeDamage(damage, true);
            }
            else if (t is EnemyIi ei)
            {
                // ignore enemy armour: pass armourPiercing = true
                ei.TakeDamage(damage, true);
            }
            else
            {
                // fallback: try reflection (rare)
                var method = t.GetType().GetMethod("TakeDamage");
                if (method != null)
                    method.Invoke(t, new object[] { damage });
            }
        }

        // Remove entries for targets that are no longer selected or were freed
        var toRemove = new List<Node2D>();
        foreach (var kv in targetSeconds)
        {
            var node = kv.Key;
            if (!currentTargets.Contains(node) || !IsInstanceValid(node))
                toRemove.Add(node);
        }
        foreach (var r in toRemove)
        {
            targetSeconds.Remove(r);
            // remove beam visual if present
            if (targetBeams.TryGetValue(r, out var beamNode))
            {
                beamNode.QueueFree();
                targetBeams.Remove(r);
            }
        }
    }

    private void CreateBeamForTarget(Node2D target)
    {
        if (beamsContainer == null) return;
        Node2D beamNode = null;
        // If user provided a BeamScene (Beam.tscn), instantiate it and expect
        // child names: Line (Line2D) and Beam (Sprite2D)
        if (BeamScene != null)
        {
            var inst = BeamScene.Instantiate();
            if (inst is Node2D n)
            {
                beamNode = n;
            }
            else if (inst is Node iNode)
            {
                // try to cast Node -> Node2D if possible
                beamNode = iNode as Node2D;
            }
        }

        // fallback: dynamic creation
        if (beamNode == null)
        {
            beamNode = new Node2D();
            beamNode.Name = "Beam";

            var line = new Line2D();
            line.Name = "Line";
            line.Width = 10f;
            if (BeamTexture != null)
                line.Texture = BeamTexture;
            // initial points relative to tower
            line.Points = new Vector2[] { Vector2.Zero, target.GlobalPosition - GlobalPosition };
            beamNode.AddChild(line);

            var head = new Sprite2D();
            head.Name = "Beam";
            if (BeamHeadTexture != null)
                head.Texture = BeamHeadTexture;
            head.Position = target.GlobalPosition - GlobalPosition;
            head.Centered = true;
            beamNode.AddChild(head);
        }
        else
        {
            // If instantiated from scene, ensure names exist and apply textures if provided
            var line = beamNode.GetNodeOrNull<Line2D>("Line");
            if (line != null && BeamTexture != null)
                line.Texture = BeamTexture;
            var head = beamNode.GetNodeOrNull<Sprite2D>("Beam");
            if (head != null && BeamHeadTexture != null)
                head.Texture = BeamHeadTexture;
            // Set initial positions
            var headNode = beamNode.GetNodeOrNull<Node2D>("Beam");
            if (headNode != null)
                headNode.Position = target.GlobalPosition - GlobalPosition;
            var lineNode = beamNode.GetNodeOrNull<Line2D>("Line");
            if (lineNode != null)
                lineNode.Points = new Vector2[] { Vector2.Zero, target.GlobalPosition - GlobalPosition };
        }

        beamsContainer.AddChild(beamNode);
        targetBeams[target] = beamNode;
    }

    private void UpdateBeams()
    {
        if (targetBeams == null) return;
        var remove = new List<Node2D>();
        foreach (var kv in targetBeams)
        {
            var target = kv.Key;
            var beamNode = kv.Value;
            if (!IsInstanceValid(target) || !IsInstanceValid(beamNode))
            {
                remove.Add(target);
                continue;
            }

            var line = beamNode.GetNodeOrNull<Line2D>("Line");
            var head = beamNode.GetNodeOrNull<Sprite2D>("Beam");
            var local = target.GlobalPosition - GlobalPosition;
            if (line != null)
            {
                line.Points = new Vector2[] { Vector2.Zero, local };
            }
            if (head != null)
            {
                head.Position = local;
                head.Rotation = local.Angle();
            }
        }

        foreach (var t in remove)
        {
            if (targetBeams.TryGetValue(t, out var b))
            {
                b.QueueFree();
                targetBeams.Remove(t);
            }
        }
    }

    private float ComputeRampedDamage(float seconds)
    {
        var start = Math.Max(DamageStart, 0.0001f);
        var max = Math.Max(MaxDamage, start);
        if (seconds <= 1f)
            return Math.Min(start, max);

        // compute multiplicative factor r so that start * r^(RampSeconds-1) = max
        var steps = Math.Max(RampSeconds, 1f);
        // if steps == 1, immediately max
        if (steps <= 1f)
            return max;

        double r = Math.Pow(max / start, 1.0 / (steps - 1.0));
        double value = start * Math.Pow(r, Math.Floor(seconds) - 1.0);
        if (value > max) value = max;
        return (float)value;
    }
}

