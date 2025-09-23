using System;

public static class GameState
{
    // Simple global flag to indicate the game is paused (soft pause)
    public static bool Paused { get; set; } = false;
}
