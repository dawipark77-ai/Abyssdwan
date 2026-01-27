using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 방 정보를 담는 클래스
/// </summary>
[System.Serializable]
public class DungeonRoom
{
    public Vector2Int position;
    public bool isExplored;
    public bool hasEnemy;
    public bool hasTreasure;
    public bool hasExit;
    public List<Vector2Int> connectedRooms;
    
    public DungeonRoom(Vector2Int pos)
    {
        position = pos;
        isExplored = false;
        hasEnemy = false;
        hasTreasure = false;
        hasExit = false;
        connectedRooms = new List<Vector2Int>();
    }
    
    /// <summary>
    /// 방 탐험 처리
    /// </summary>
    public void Explore()
    {
        isExplored = true;
    }
    
    /// <summary>
    /// 연결된 방 추가
    /// </summary>
    public void AddConnection(Vector2Int connectedRoom)
    {
        if (!connectedRooms.Contains(connectedRoom))
        {
            connectedRooms.Add(connectedRoom);
        }
    }
    
    /// <summary>
    /// 특정 방과 연결되어 있는지 확인
    /// </summary>
    public bool IsConnectedTo(Vector2Int roomPosition)
    {
        return connectedRooms.Contains(roomPosition);
    }
}






