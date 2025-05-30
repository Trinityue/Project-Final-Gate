using Godot;
using System;

public partial class EnemySpawner : Node2D
{

    [Export] PackedScene enemyScene;
    [Export] Node2D[] spawn_Points;
    [Export] float eps = 1f;

    [Export] public Node2D[] PathNodes; // Ziehe im Editor deine Wegpunkte rein

    [Export] public float MinSpeed = 80f;   // Minimal einstellbare Geschwindigkeit
    [Export] public float MaxSpeed = 150f;  // Maximal einstellbare Geschwindigkeit
    float spawn_rate;
    float tus = 0; // time until spawn

    public override void _Ready()
    {
        spawn_rate = 1 / eps;

    }

    public override void _Process(double delta)
    {
        if (tus > spawn_rate)
        {
            Spawn();
            tus = 0;
        }
        else
        {
            tus += (float)delta;
        }
    }

    private void Spawn()
    {
        if (spawn_Points == null || spawn_Points.Length == 0)
            return;

        RandomNumberGenerator rng = new RandomNumberGenerator();
        int index = (int)(rng.Randi() % spawn_Points.Length);
        Vector2 location = spawn_Points[index].GlobalPosition;

        Enemy enemy = (Enemy)enemyScene.Instantiate();
        enemy.GlobalPosition = location;
        enemy.PathNodes = this.PathNodes;

        // Geschwindigkeit zufällig im Bereich [MinSpeed, MaxSpeed] setzen
        enemy.Speed = rng.RandfRange(MinSpeed, MaxSpeed);

        GetTree().Root.AddChild(enemy);
    }

}