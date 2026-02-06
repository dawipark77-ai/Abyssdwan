using UnityEngine;
using UnityEngine.Tilemaps;

public enum DungeonDirection { North, East, South, West }

public class DungeonGridPlayer : MonoBehaviour
{
    [Header("References")]
    public Tilemap wallTilemap;  // 벽 타일맵 (충돌 검사용)
    public Tilemap floorTilemap; // 바닥 타일맵 (월드 좌표 정렬용)
    public Tilemap fogTilemap;
    public MapManager mapManager; // 맵 정보(크기, 타일맵) 참조
    public int viewRadius = 1;
    public Vector2Int gridPos = new Vector2Int(2, 2);
    public DungeonDirection facing = DungeonDirection.North;

    [Header("Minimap Settings")]
    public RectTransform playerCursor;
    public float minimapCellSize = 40f; 
    public Vector2 minimapOrigin = new Vector2(-520, -940);

    void Start()
    {
        EnsureReferences();
        
        // [SO] PlayerStats가 스스로 statData를 로드하므로 GameManager 연동 제거
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats == null)
        {
            stats = gameObject.AddComponent<PlayerStats>();
            Debug.Log("[DungeonGridPlayer] PlayerStats component was missing. Auto-added.");
        }

        UpdateWorldPosition();
        UpdateView();
        
        // 데이터 복구 시 안개 상태 복구
        if (DungeonPersistentData.hasSavedState)
        {
            RestoreFog();
        }
        
        RevealFog();
    }

    void Update()
    {
        // Absolute Cardinal Movement
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) MoveNorth();
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) MoveSouth();
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) MoveWest();
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveEast();
    }

    // Public methods for UI Buttons (Cardinal)
    public void MoveNorth() => TryMoveTo(DungeonDirection.North);
    public void MoveSouth() => TryMoveTo(DungeonDirection.South);
    public void MoveWest()  => TryMoveTo(DungeonDirection.West);
    public void MoveEast()  => TryMoveTo(DungeonDirection.East);

    private void TryMoveTo(DungeonDirection dir)
    {
        EnsureReferences();

        facing = dir;
        
        // Check for block-based walls using tilemap
        Vector2Int moveDir = GetDirVector(dir);
        Vector2Int nextPos = gridPos + moveDir;

        // 맵 범위 체크 (mapManager 정보를 우선 사용)
        if (mapManager != null)
        {
            if (nextPos.x < 0 || nextPos.y < 0 || nextPos.x >= mapManager.width || nextPos.y >= mapManager.height)
            {
                Debug.Log($"[Player] 이동 불가: 맵 범위를 벗어남 ({nextPos.x}, {nextPos.y})");
                return;
            }
        }

        // 타일맵에서 벽 체크 (타일이 있으면 벽 = 이동 불가)
        Vector3Int tilePos = new Vector3Int(nextPos.x, -nextPos.y, 0);
        if (wallTilemap != null && wallTilemap.HasTile(tilePos))
        {
            Debug.Log($"[Player] 이동 불가: 벽에 막힘 ({nextPos.x}, {nextPos.y})");
            return;
        }

        // Move to next tile
        gridPos = nextPos;
        UpdateWorldPosition();
        UpdateView();
        RevealFog();

        // 출구에 도달하면 다음 층 생성
        if (mapManager != null && gridPos == mapManager.ExitPos)
        {
            Debug.Log($"[Player] Exit reached at ({gridPos.x}, {gridPos.y}) -> Next floor");
            mapManager.GenerateNextFloor();
            return;
        }
        
        // 랜덤 인카운터 체크
        DungeonEncounter.Instance?.CheckEncounter(gridPos);
        
        Debug.Log($"[Player] Moved to ({gridPos.x}, {gridPos.y}) | Facing: {facing}");
    }

    private Vector2Int GetDirVector(DungeonDirection dir)
    {
        switch (dir)
        {
            case DungeonDirection.North: return new Vector2Int(0, -1);
            case DungeonDirection.East: return new Vector2Int(1, 0);
            case DungeonDirection.South: return new Vector2Int(0, 1);
            case DungeonDirection.West: return new Vector2Int(-1, 0);
        }
        return Vector2Int.zero;
    }

    public Vector2Int GetForwardVector()
    {
        return GetDirVector(facing);
    }

    /// <summary>
    /// 이동/텔레포트 전에 필요한 참조를 자동으로 채워 그리드 정렬이 어긋나지 않도록 함.
    /// </summary>
    private void EnsureReferences()
    {
        if (mapManager == null)
        {
            mapManager = FindFirstObjectByType<MapManager>();
        }

        if (wallTilemap == null)
        {
            if (mapManager != null) wallTilemap = mapManager.wallTilemap;

            if (wallTilemap == null)
            {
                GameObject gridObj = GameObject.Find("Grid");
                if (gridObj != null)
                {
                    Transform wallTilemapTransform = gridObj.transform.Find("Wall");
                    if (wallTilemapTransform != null)
                    {
                        wallTilemap = wallTilemapTransform.GetComponent<Tilemap>();
                    }
                }
            }
        }

        if (floorTilemap == null)
        {
            if (mapManager != null) floorTilemap = mapManager.floorTilemap;
            else
            {
                var mgr = FindFirstObjectByType<MapManager>();
                if (mgr != null) floorTilemap = mgr.floorTilemap;
            }

            if (floorTilemap == null)
            {
                GameObject gridObj = GameObject.Find("Grid");
                if (gridObj != null)
                {
                    Transform floorTilemapTransform = gridObj.transform.Find("Floor");
                    if (floorTilemapTransform != null)
                    {
                        floorTilemap = floorTilemapTransform.GetComponent<Tilemap>();
                    }
                }
            }
        }

        if (fogTilemap == null && mapManager != null)
        {
            fogTilemap = mapManager.fogTilemap;
        }
    }

    void UpdateWorldPosition()
    {
        EnsureReferences();

        // 타일맵의 셀 센터에 정렬 (Grid Cell Size 및 Offset을 자동 반영)
        Tilemap positionTilemap = floorTilemap != null ? floorTilemap : wallTilemap;
        if (positionTilemap != null)
        {
            Vector3Int cell = new Vector3Int(gridPos.x, -gridPos.y, 0);
            transform.position = positionTilemap.GetCellCenterWorld(cell);
        }
        else
        {
            // 타일맵을 못 찾았을 때의 안전한 기본값(셀 크기 1, y 반전)
            transform.position = new Vector3(gridPos.x, -gridPos.y, 0);
        }
    }

    private void UpdateView()
    {
        // Update Minimap Cursor
        if (playerCursor != null)
        {
            // Position: Grid * Size
            playerCursor.anchoredPosition = minimapOrigin + new Vector2(gridPos.x * minimapCellSize, gridPos.y * minimapCellSize);
            
            // Rotation: North=0, East=-90, South=180, West=90 (assuming Sprite points UP)
            float zRot = 0;
            switch (facing)
            {
                case DungeonDirection.North: zRot = 0; break;
                case DungeonDirection.East:  zRot = -90; break;
                case DungeonDirection.South: zRot = 180; break;
                case DungeonDirection.West:  zRot = 90; break;
            }
            playerCursor.localRotation = Quaternion.Euler(0, 0, zRot);
        }
    }

    public void Teleport(Vector2Int pos)
    {
        EnsureReferences();
        gridPos = pos;
        UpdateWorldPosition();
        UpdateView();
        RevealFog();
        
        // 랜덤 인카운터 체크 (텔레포트 시에는 인카운터 발생 안 함)
        // DungeonEncounter.Instance?.CheckEncounter(gridPos);
        
        Debug.Log($"[Player] Teleported to ({pos.x}, {pos.y})");
    }

    // 벽선 그리기 컴포넌트 캐시 (성능 최적화)
    private Genesis01.Dungeon.DungeonWallLineDrawer cachedWallDrawer = null;
    private float lastWallUpdateTime = 0f;
    private const float WALL_UPDATE_INTERVAL = 0.1f; // 0.1초마다만 업데이트

    void RevealFog()
    {
        if (fogTilemap == null) return;

        bool fogChanged = false;
        for (int y = -viewRadius; y <= viewRadius; y++)
        {
            for (int x = -viewRadius; x <= viewRadius; x++)
            {
                Vector2Int worldGridPos = new Vector2Int(gridPos.x + x, gridPos.y + y);
                Vector3Int tilePos = new Vector3Int(worldGridPos.x, -worldGridPos.y, 0);
                
                if (fogTilemap.HasTile(tilePos))
                {
                    fogTilemap.SetTile(tilePos, null);
                    // 탐험 영역 저장 (중복 체크)
                    if (!DungeonPersistentData.revealedTiles.Contains(worldGridPos))
                    {
                        DungeonPersistentData.revealedTiles.Add(worldGridPos);
                        fogChanged = true;
                    }
                }
            }
        }
        
        // 벽선 가시성 업데이트 (안개 시스템 연동)
        // 성능 최적화: 캐시 사용 및 업데이트 빈도 제한
        if (fogChanged && Time.time - lastWallUpdateTime > WALL_UPDATE_INTERVAL)
        {
            if (cachedWallDrawer == null)
            {
                cachedWallDrawer = FindFirstObjectByType<Genesis01.Dungeon.DungeonWallLineDrawer>();
            }
            
            if (cachedWallDrawer != null)
            {
                cachedWallDrawer.UpdateWallVisibility();
                lastWallUpdateTime = Time.time;
            }
        }
    }

    void RestoreFog()
    {
        if (fogTilemap == null) return;
        
        Debug.Log($"[Player] Restoring fog for {DungeonPersistentData.revealedTiles.Count} tiles.");
        foreach (var pos in DungeonPersistentData.revealedTiles)
        {
            Vector3Int tilePos = new Vector3Int(pos.x, -pos.y, 0);
            fogTilemap.SetTile(tilePos, null);
        }
        
        // 벽선 가시성도 함께 업데이트
        Genesis01.Dungeon.DungeonWallLineDrawer wallDrawer = FindFirstObjectByType<Genesis01.Dungeon.DungeonWallLineDrawer>();
        if (wallDrawer != null)
        {
            wallDrawer.UpdateWallVisibility();
        }
    }

    private void OnDrawGizmos()
    {
        // Draw Player
        Gizmos.color = Color.green;
        Vector3 pPos = new Vector3(gridPos.x, 0, gridPos.y);
        Gizmos.DrawSphere(pPos, 0.5f);
        
        Vector2Int fwd = GetForwardVector();
        Gizmos.DrawLine(pPos, pPos + new Vector3(fwd.x, 0, fwd.y));
    }
}
