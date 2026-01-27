using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    [Header("던전 설정")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private int minRooms = 20;
    [SerializeField] private int maxRooms = 30;
    
    [Header("방 설정")]
    [SerializeField] private float enemySpawnChance = 0.3f;
    [SerializeField] private float treasureSpawnChance = 0.2f;
    [SerializeField] public float cellSize = 1f;
    
    private HashSet<Vector2Int> generatedRooms = new HashSet<Vector2Int>();
    private List<Vector2Int> roomList = new List<Vector2Int>();
    
    public Dictionary<Vector2Int, DungeonRoom> CreateDungeon()
    {
        generatedRooms.Clear();
        roomList.Clear();
        Dictionary<Vector2Int, DungeonRoom> rooms = new Dictionary<Vector2Int, DungeonRoom>();
        Vector2Int startRoom = new Vector2Int(0, 0);
        rooms[startRoom] = new DungeonRoom(startRoom);
        generatedRooms.Add(startRoom);
        roomList.Add(startRoom);
        int targetRoomCount = Random.Range(minRooms, maxRooms + 1);
        int attempts = 0;
        int maxAttempts = 1000;
        while (roomList.Count < targetRoomCount && attempts < maxAttempts)
        {
            attempts++;
            Vector2Int currentRoom = roomList[Random.Range(0, roomList.Count)];
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            Vector2Int direction = directions[Random.Range(0, directions.Length)];
            Vector2Int newRoomPos = currentRoom + direction;
            if (newRoomPos.x < 0 || newRoomPos.x >= width || newRoomPos.y < 0 || newRoomPos.y >= height)
            {
                continue;
            }
            if (generatedRooms.Contains(newRoomPos))
            {
                if (rooms.ContainsKey(currentRoom) && rooms.ContainsKey(newRoomPos))
                {
                    rooms[currentRoom].AddConnection(newRoomPos);
                    rooms[newRoomPos].AddConnection(currentRoom);
                }
                continue;
            }
            DungeonRoom newRoom = new DungeonRoom(newRoomPos);
            if (Random.value < enemySpawnChance)
            {
                newRoom.hasEnemy = true;
            }
            if (Random.value < treasureSpawnChance)
            {
                newRoom.hasTreasure = true;
            }
            rooms[newRoomPos] = newRoom;
            generatedRooms.Add(newRoomPos);
            roomList.Add(newRoomPos);
            if (rooms.ContainsKey(currentRoom))
            {
                rooms[currentRoom].AddConnection(newRoomPos);
                newRoom.AddConnection(currentRoom);
            }
        }
        if (roomList.Count > 1)
        {
            Vector2Int exitRoom = roomList[roomList.Count - 1];
            if (rooms.ContainsKey(exitRoom))
            {
                rooms[exitRoom].hasExit = true;
            }
        }
        return rooms;
    }
    
    // 호환성을 위한 별칭 메서드
    public Dictionary<Vector2Int, DungeonRoom> GenerateDungeon()
    {
        return CreateDungeon();
    }
    
    public bool HasRoomAt(Vector2Int position)
    {
        return generatedRooms.Contains(position);
    }
    
    public bool IsWalkable(Vector2Int position)
    {
        return generatedRooms.Contains(position);
    }
    
    public bool IsWalkable(int x, int y)
    {
        return IsWalkable(new Vector2Int(x, y));
    }
}
