using Godot;
using System;

public partial class Tower : Node2D
{
    [Export] public PackedScene BulletScene;
    [Export] public Node2D Spawner;
    [Export] public float bps = 1f; // bullets per sec 
    [Export] public Node2D bullets;
    [Export] public Node2D[] PathNodes; // Ziehe im Editor deine Wegpunkte rein
    [Export] public float MinSpeed = 80f;   // Minimal einstellbare Geschwindigkeit
    [Export] public float MaxSpeed = 150f;  // Maximal einstellbare Geschwindigkeit

    public float spawn_rate;
    public float tus = 0; // time until spawn
    public override void _Ready()
    {
        spawn_rate = 1 / bps;
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
        if (Spawner == null)
            return;

        // Bullet wird am Spawner-Node erzeugt
        Vector2 location = Spawner.GlobalPosition;

        // Bullet instanziieren
        var bulletInstance = BulletScene.Instantiate();
        if (bulletInstance is Bullet bullet)
        {
            bullet.GlobalPosition = location;
            bullet.PathNodes = this.PathNodes;
            bullet.Speed = (float)GD.RandRange(MinSpeed, MaxSpeed);

            // Bullet als Kind zum Szenenbaum hinzufügen
            GetTree().Root.AddChild(bullet);
        }
        else
        {
            GD.PrintErr("BulletScene ist nicht vom Typ 'Bullet'.");
        }
    }   
    
}