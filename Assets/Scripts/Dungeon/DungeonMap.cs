using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genesis01.Dungeon
{
    [System.Flags]
    public enum WallFlags
    {
        None  = 0,
        North = 1 << 0,
        East  = 1 << 1,
        South = 1 << 2,
        West  = 1 << 3
    }


    /// <summary>
    /// Represents different types of dungeon tiles
    /// </summary>
    public enum TileType
    {
        Empty = 0,      // Walkable floor
        Wall = 1,       // Solid block (deprecated in favor of WallFlags, but kept for compatibility)
        Door = 2,       // Door (can be opened)
        Stairs = 3,     // Stairs to next level
        Trap = 4,       // Trap tile
        Item = 5,       // Item on floor
        Enemy = 6       // Enemy spawn point
    }

    /// <summary>
    /// Represents a single tile in the dungeon
    /// </summary>
    [Serializable]
    public class DungeonTile
    {
        public TileType type;
        public WallFlags wallFlags;   // 비트마스크 형태의 벽 정보 (North/East/South/West)
        public int wallTextureId;  // For visual variety
        public bool isExplored;    // For fog of war
        public string metadata;    // JSON data for items, enemies, etc.

        public DungeonTile(TileType type = TileType.Empty)
        {
            this.type = type;
            this.wallFlags = WallFlags.None;
            this.wallTextureId = 0;
            this.isExplored = false;
            this.metadata = "";
        }

        public bool IsWalkable()
        {
            // A tile is walkable if it's not a solid block
            return type != TileType.Wall;
        }
        
        public bool HasWall(WallFlags dir)
        {
            return (wallFlags & dir) != 0;
        }
    }

    /// <summary>
    /// 3D grid-based dungeon map
    /// </summary>
    public class DungeonMap : MonoBehaviour
    {
        [Header("Map Dimensions")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private int floors = 1;

        [Header("Map Data")]
        [SerializeField] private bool useProceduralGeneration = false;

        // 3D array: [floor][y][x]
        private DungeonTile[,,] tiles;

        public int Width => width;
        public int Height => height;
        public int Floors => floors;

        private void Awake()
        {
            InitializeMap();
        }

        /// <summary>
        /// Initialize or Resize the dungeon map
        /// </summary>
        public void InitializeMap(int w = -1, int h = -1)
        {
            if (w > 0) width = w;
            if (h > 0) height = h;

            tiles = new DungeonTile[floors, height, width];

            for (int f = 0; f < floors; f++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        tiles[f, y, x] = new DungeonTile(TileType.Empty);
                    }
                }
            }

            if (useProceduralGeneration)
            {
                GenerateProceduralDungeon();
            }
            else
            {
                CreateSampleDungeon();
            }
        }

        /// <summary>
        /// Get tile at specific position
        /// </summary>
        public DungeonTile GetTile(int floor, int x, int y)
        {
            if (!IsValidPosition(floor, x, y))
                return new DungeonTile(TileType.Wall); // Out of bounds = wall

            return tiles[floor, y, x];
        }

        /// <summary>
        /// Set tile at specific position
        /// </summary>
        public void SetTile(int floor, int x, int y, DungeonTile tile)
        {
            if (IsValidPosition(floor, x, y))
            {
                tiles[floor, y, x] = tile;
            }
        }
        
        /// <summary>
        /// Set or clear wall flags at a position (edge-based 벽 표현용)
        /// </summary>
        public void SetWallAt(int floor, int x, int y, WallFlags dir, bool add = true)
        {
            if (!IsValidPosition(floor, x, y)) return;
            var tile = tiles[floor, y, x];
            if (add)
                tile.wallFlags |= dir;
            else
                tile.wallFlags &= ~dir;
            tiles[floor, y, x] = tile;
        }

        /// <summary>
        /// Check if position is within map bounds
        /// </summary>
        public bool IsValidPosition(int floor, int x, int y)
        {
            return floor >= 0 && floor < floors &&
                   x >= 0 && x < width &&
                   y >= 0 && y < height;
        }

        /// <summary>
        /// Check if a tile is walkable (not a solid wall block)
        /// </summary>
        public bool IsWalkable(int floor, int x, int y)
        {
            if (!IsValidPosition(floor, x, y)) return false;
            return tiles[floor, y, x].IsWalkable();
        }

        /// <summary>
        /// Create a sample dungeon for testing using edge-based walls
        /// </summary>
        private void CreateSampleDungeon()
        {
            // Create walls around the perimeter
            for (int x = 0; x < width; x++)
            {
                SetTile(0, x, 0, new DungeonTile(TileType.Wall));
                SetTile(0, x, height - 1, new DungeonTile(TileType.Wall));
            }

            for (int y = 0; y < height; y++)
            {
                SetTile(0, 0, y, new DungeonTile(TileType.Wall));
                SetTile(0, width - 1, y, new DungeonTile(TileType.Wall));
            }

            // Create some interior walls
            for (int y = 2; y < 8; y++)
            {
                if (y != 5) // Leave a gap at y=5
                {
                    SetTile(0, 5, y, new DungeonTile(TileType.Wall));
                }
            }

            // Horizontal wall segments
            for (int x = 2; x < 5; x++)
            {
                SetTile(0, x, 3, new DungeonTile(TileType.Wall));
            }

            for (int x = 6; x < 9; x++)
            {
                SetTile(0, x, 6, new DungeonTile(TileType.Wall));
            }

            // Add some variety to wall textures
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tiles[0, y, x].type == TileType.Wall)
                    {
                        tiles[0, y, x].wallTextureId = UnityEngine.Random.Range(0, 3);
                    }
                }
            }

            Debug.Log("Sample dungeon created: " + width + "x" + height);
        }

        /// <summary>
        /// Generate a procedural dungeon using edge-based walls
        /// </summary>
        private void GenerateProceduralDungeon()
        {
            // 1. Fill with ALL walls (Closed grid)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tiles[0, y, x] = new DungeonTile(TileType.Wall);
                }
            }

            // 2. Create rooms
            int numRooms = UnityEngine.Random.Range(4, 7);
            List<RectInt> rooms = new List<RectInt>();

            for (int i = 0; i < numRooms; i++)
            {
                int w = UnityEngine.Random.Range(2, 5);
                int h = UnityEngine.Random.Range(2, 5);
                int x = UnityEngine.Random.Range(1, width - w - 1);
                int y = UnityEngine.Random.Range(1, height - h - 1);
                
                RectInt room = new RectInt(x, y, w, h);
                
                // Carve room interior (remove walls between inside cells)
                for (int ry = y; ry < y + h; ry++)
                {
                    for (int rx = x; rx < x + w; rx++)
                    {
                        SetTile(0, rx, ry, new DungeonTile(TileType.Empty));
                    }
                }
                rooms.Add(room);
            }

            // 3. Connect rooms with simple corridors (carve paths)
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                Vector2Int start = Vector2Int.RoundToInt(rooms[i].center);
                Vector2Int end = Vector2Int.RoundToInt(rooms[i + 1].center);
                CreateCorridor(start, end);
            }

            Debug.Log($"Procedural dungeon (Edge-based) generated with {rooms.Count} rooms and corridors.");
        }

        /// <summary>
        /// Create a corridor between two points, removing bitmask walls along the path
        /// </summary>
        private void CreateCorridor(Vector2Int start, Vector2Int end)
        {
            int x = start.x;
            int y = start.y;

            // Horizontal then vertical
            while (x != end.x)
            {
                int step = (end.x > x) ? 1 : -1;
                SetTile(0, x, y, new DungeonTile(TileType.Empty)); // Remove wall
                x += step;
            }

            while (y != end.y)
            {
                int step = (end.y > y) ? 1 : -1;
                WallFlags dir = (step == 1) ? WallFlags.South : WallFlags.North;
                
                SetTile(0, x, y, new DungeonTile(TileType.Empty)); // Remove wall block
                SetWallAt(0, x, y, dir, false); // Remove wall in move direction
                y += step;
            }
        }

        /// <summary>
        /// Get a debug string representation of the map
        /// </summary>
        public string GetMapDebugString(int floor = 0)
        {
            if (floor < 0 || floor >= floors)
                return "Invalid floor";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Dungeon Map (Floor " + floor + "):");

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    switch (tiles[floor, y, x].type)
                    {
                        case TileType.Empty: sb.Append("."); break;
                        case TileType.Wall: sb.Append("#"); break;
                        case TileType.Door: sb.Append("D"); break;
                        case TileType.Stairs: sb.Append("S"); break;
                        default: sb.Append("?"); break;
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
