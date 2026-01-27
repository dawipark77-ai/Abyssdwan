using UnityEngine;

/// <summary>
/// 던전 맵 타일 데이터
/// </summary>
[System.Serializable]
public class DungeonMapTile
{
    public enum TileType
    {
        Wall,    // 벽
        Path,    // 길
        Start,   // 시작점
        Exit     // 탈출점
    }
    
    public TileType tileType;
    public bool isExplored;  // 탐험했는지 여부
    public Vector2Int position;
    
    public DungeonMapTile(Vector2Int pos, TileType type = TileType.Wall)
    {
        position = pos;
        tileType = type;
        isExplored = false;
    }
    
    public bool IsWalkable()
    {
        return tileType == TileType.Path || 
               tileType == TileType.Start || 
               tileType == TileType.Exit;
    }
}


