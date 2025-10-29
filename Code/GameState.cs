using System;

public static class GameState
{
    // Simple global flag to indicate the game is paused (soft pause)
    public static bool Paused { get; set; } = false;
    // Indicates that a tower is in manual control mode; GameManager should avoid placing towers while true
    public static bool ManualControlActive { get; set; } = false;
}
