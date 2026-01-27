using UnityEngine;

public class DungeonRoomGenerator
{
    public static void CreateRoom(Genesis01.Dungeon.DungeonMap map, RoomInfo roomInfo)
    {
        RectInt room = roomInfo.rect;
        
        // Clear interior (make it empty)
        for (int y = room.yMin; y < room.yMax; y++)
        {
            for (int x = room.xMin; x < room.xMax; x++)
            {
                map.SetTile(0, x, y, new Genesis01.Dungeon.DungeonTile(Genesis01.Dungeon.TileType.Empty));
            }
        }

        // Note: Outer walls are handled by MapManager's GenerateMaze logic or SetTile if needed.
        // Reverting to simple interior clearing as per block-based rollback.
    }
}
