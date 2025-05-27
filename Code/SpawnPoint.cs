using Godot;
using System;

public partial class SpawnPoint : Node2D
{

    [Export] PackedScene enemyScene;
    [Export] Node2D[] spawn_Points;
    [Export] float eps = 1;

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
    {   RandomNumberGenerator rng = new RandomNumberGenerator();
        Vector2 location = spawn_Points[rng.Randi() % spawn_Points.Length].GlobalPosition;
        // Ensure the spawn point is not occupied
        Enemy enemy = (Enemy)enemyScene.Instantiate();
        enemy.GlobalPosition = location;
        GetTree().Root.AddChild(enemy);
        }

}
