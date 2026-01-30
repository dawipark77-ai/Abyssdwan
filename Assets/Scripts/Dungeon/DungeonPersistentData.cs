using UnityEngine;

public static class DungeonPersistentData
{
    public static bool hasSavedState = false;
    public static int currentSeed = -1;
    public static int currentFloor = 1; // 1층부터 시작 (B1)
    public static Vector2Int lastPlayerGridPos;
    public static DungeonDirection lastPlayerFacing;
    public static System.Collections.Generic.HashSet<Vector2Int> revealedTiles = new System.Collections.Generic.HashSet<Vector2Int>();

    // Player Stats Persistence
    public static bool hasPlayerStats = false;
    public static int heroHP;
    public static int heroMaxHP;
    public static int heroMP;
    public static int heroMaxMP;
    public static bool heroIgnited;
    public static int heroIgniteTurns;

    public static void ClearState()
    {
        hasSavedState = false;
        currentSeed = -1;
        currentFloor = 1;
        revealedTiles.Clear();
        
        hasPlayerStats = false;
        heroHP = 0;
        heroMaxHP = 0;
        heroMP = 0;
        heroMaxMP = 0;
        heroIgnited = false;
        heroIgniteTurns = 0;
    }

    public static void SavePlayerState(PlayerStats player)
    {
        if (player == null) return;
        
        hasPlayerStats = true;
        heroHP = player.currentHP;
        heroMaxHP = player.maxHP;
        heroMP = player.currentMP;
        heroMaxMP = player.maxMP;
        heroIgnited = player.isIgnited;
        heroIgniteTurns = player.igniteTurnsRemaining;
        
        Debug.Log($"[DungeonPersistentData] Saved Player State: HP {heroHP}/{heroMaxHP}, MP {heroMP}/{heroMaxMP}, Ignited: {heroIgnited}");
    }

    public static void LoadPlayerState(PlayerStats player)
    {
        if (player == null || !hasPlayerStats) return;

        player.currentHP = heroHP;
        // maxHP and maxMP are now read-only properties calculated from CharacterClass SO
        player.currentMP = heroMP;
        player.SetIgnited(heroIgnited, heroIgniteTurns);

        Debug.Log($"[DungeonPersistentData] Loaded Player State: HP {heroHP}/{heroMaxHP}, MP {heroMP}/{heroMaxMP}, Ignited: {heroIgnited}");
    }
}
