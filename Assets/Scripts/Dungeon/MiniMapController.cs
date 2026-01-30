using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Genesis01.Dungeon
{
    /// <summary>
    /// Controls the mini-map rendering using MapTile prefabs.
    /// Replaces the old RawImage pixel-based approach.
    /// </summary>
    public class MiniMapController : MonoBehaviour
    {
        [Header("Dependencies")]
        public DungeonMap dungeonMap;
        public DungeonPlayerMovement playerMovement;
        
        [Header("UI References")]
        public Transform gridContainer; // The parent object for tiles (e.g., a Panel with GridLayoutGroup)
        public MapTile mapTilePrefab;   // The prefab we just created
        public RectTransform playerIcon; // The arrow icon for the player

        [Header("Map Settings")]
        [Range(0f, 1f)] public float mapOpacity = 0.7f;
        public Color floorColor = new Color(0.2f, 0.2f, 0.2f); // Dark Gray
        public Color wallColor = Color.white; // White Walls
        public Color visitedColor = Color.white; // Color for visited tiles

        private MapTile[,] tileGrid;
        private CanvasGroup canvasGroup;
        private Vector2Int lastPlayerPos = new Vector2Int(-1, -1);

        void Start()
        {
            // Setup CanvasGroup for opacity control
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
            // Find dependencies if missing
            if (dungeonMap == null) dungeonMap = FindFirstObjectByType<DungeonMap>();
            if (playerMovement == null) playerMovement = FindFirstObjectByType<DungeonPlayerMovement>();

            if (dungeonMap != null)
            {
                GenerateGrid();
                UpdateMapVisuals();
            }
        }

        void Update()
        {
            // Sync opacity
            if (canvasGroup != null)
            {
                canvasGroup.alpha = mapOpacity;
            }
        }

        public void GenerateGrid()
        {
            if (dungeonMap == null || mapTilePrefab == null || gridContainer == null)
            {
                Debug.LogWarning("MiniMapController: Missing references for Grid Generation! Minimap will not be displayed.");
                return;
            }

            // Clear existing children
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }

            int width = dungeonMap.Width;
            int height = dungeonMap.Height;
            tileGrid = new MapTile[width, height];

            // Ensure GridLayoutGroup is set up correctly on the container
            GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null) gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
            
            // Set constraint to Fixed Column Count to ensure correct wrapping
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = width;
            
            // Instantiate tiles
            // Note: Unity UI Grid Layout fills Top-Left to Bottom-Right usually.
            // We iterate y from height-1 down to 0 to match this visual layout.
            
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    MapTile newTile = Instantiate(mapTilePrefab, gridContainer);
                    newTile.name = $"Tile_{x}_{y}";
                    
                    DungeonTile currentTile = dungeonMap.GetTile(0, x, y);
                    
                    if (currentTile.type == TileType.Wall)
                    {
                        // Render Wall: Solid block
                        newTile.floorImage.enabled = true;
                        newTile.floorImage.color = wallColor; 
                        newTile.Setup(false, false, false, false); 
                    }
                    else
                    {
                        // Render Floor: No thin walls in block mode
                        newTile.floorImage.enabled = true;
                        newTile.SetColor(floorColor, wallColor);
                        newTile.Setup(false, false, false, false);
                    }

                    tileGrid[x, y] = newTile;
                }
            }
        }

        private bool IsWall(int x, int y)
        {
            if (dungeonMap == null) return true;
            if (x < 0 || x >= dungeonMap.Width || y < 0 || y >= dungeonMap.Height) return true;
            
            DungeonTile tile = dungeonMap.GetTile(0, x, y);
            return tile.type == TileType.Wall;
        }

        public void OnPlayerMove(Vector2Int playerPos)
        {
            UpdateMapVisuals(playerPos);
        }

        private void UpdateMapVisuals(Vector2Int currentPlayerPos = default)
        {
            if (dungeonMap == null || tileGrid == null) return;

            // Get player pos if default
            if (currentPlayerPos == default && playerMovement != null)
            {
                // Logic to get player pos if needed
            }
            
            // Update Visited Status (Fog of War)
            for (int y = 0; y < dungeonMap.Height; y++)
            {
                for (int x = 0; x < dungeonMap.Width; x++)
                {
                    if (tileGrid[x, y] == null) continue;

                    DungeonTile tileData = dungeonMap.GetTile(0, x, y);
                    
                    // Only show if explored
                    tileGrid[x, y].SetVisited(tileData.isExplored);
                }
            }

            // Update Player Icon Position
            if (playerIcon != null && tileGrid[currentPlayerPos.x, currentPlayerPos.y] != null)
            {
                // Move icon to the position of the tile
                playerIcon.SetParent(tileGrid[currentPlayerPos.x, currentPlayerPos.y].transform);
                
                // Center it!
                playerIcon.anchorMin = new Vector2(0.5f, 0.5f);
                playerIcon.anchorMax = new Vector2(0.5f, 0.5f);
                playerIcon.anchoredPosition = Vector2.zero;
                
                // Reset rotation/scale if needed
                playerIcon.localScale = Vector3.one;
                
                // Update Rotation based on player facing
                if (playerMovement != null)
                {
                    // Assuming playerMovement has a facing direction
                    // float rotation = ...
                    // playerIcon.localRotation = Quaternion.Euler(0, 0, -rotation);
                }
            }
        }
    }
}
