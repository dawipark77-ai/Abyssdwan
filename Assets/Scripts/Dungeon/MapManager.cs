using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    // ... (rest of fields maintained)
    public Tilemap fogTilemap;

    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase fogTile;
    public TileBase stairsTile; // 다음 층으로 내려가는 계단
    public TileBase exitHighlightTile; // 출구 색상 전용(밝은 타일 권장)
    [Header("Exit Highlight")]
    public bool highlightExit = true;
    public Color exitColor = Color.red;
    public bool alwaysRevealExit = false; // 기본은 안개에 가리되, 켜면 항상 보이게
    public Vector2Int ExitPos => exitPos; // 외부에서 출구 좌표 확인용

    [Header("Map Size Settings")]
    public int minWidth = 30;
    public int maxWidth = 30;
    public int minHeight = 30;
    public int maxHeight = 30;
    
    [HideInInspector]
    public int width;
    [HideInInspector]
    public int height;

    [Header("Special Room Settings")]
    public int minSpecialRoomCount = 1;
    public int maxSpecialRoomCount = 3;

    [Header("Room Settings")]
    public int minRoomCount = 3;
    public int maxRoomCount = 5;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;
    public int roomSpacing = 2; // 방 사이 최소 간격
    
    [Range(0f, 1f)]
    public float sealedRoomChance = 0.3f; // 일반 방이 닫힌 방이 될 확률

    int[,] mapData;
    bool[,] isRoom; // 방의 바닥과 벽을 보호하기 위한 배열
    List<RoomInfo> rooms = new List<RoomInfo>(); // 생성된 방들 저장
    Vector2Int startPos; // Store start position
    Vector2Int exitPos;  // 다음 층으로 내려가는 위치

    void Start()
    {
        try
        {
            Debug.Log("[MapManager] Start() - Initializing dungeon generation.");
            if (DungeonPersistentData.currentFloor < 1) DungeonPersistentData.currentFloor = 1;
            Debug.Log($"[MapManager] Current Floor: B{DungeonPersistentData.currentFloor}");
            if (!DungeonPersistentData.hasSavedState)
            {
                DungeonPersistentData.currentSeed = Random.Range(int.MinValue, int.MaxValue);
                DungeonPersistentData.revealedTiles.Clear();
                Debug.Log($"[MapManager] New Seed generated: {DungeonPersistentData.currentSeed}. Fog cleared.");
            }
            else
            {
                Debug.Log($"[MapManager] Using existing Seed: {DungeonPersistentData.currentSeed}");
            }

            Debug.Log("[MapManager] Step 1: Generating maze...");
            GenerateMaze();
            
            Debug.Log("[MapManager] Step 2: Drawing map...");
            DrawMap();
            
            Debug.Log("[MapManager] Step 3: Placing player...");
            PlacePlayer(); // Move player placement AFTER drawing map/fog
            
            Debug.Log("[MapManager] Dungeon generation sequence completed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MapManager] CRITICAL ERROR in Start(): {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            // 예외가 발생해도 게임이 계속 실행되도록 함
            // 최소한의 맵이라도 생성 시도
            try
            {
                if (floorTilemap != null && wallTilemap != null)
                {
                    Debug.LogWarning("[MapManager] Attempting emergency map generation...");
                    // 최소한의 안전한 맵 생성
                    if (width <= 0 || height <= 0)
                    {
                        width = 10;
                        height = 10;
                        mapData = new int[height, width];
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                mapData[y, x] = (x == 0 || x == width - 1 || y == 0 || y == height - 1) ? 1 : 0;
                            }
                        }
                        startPos = new Vector2Int(1, 1);
                        exitPos = new Vector2Int(width - 2, height - 2);
                        DrawMap();
                        PlacePlayer();
                    }
                }
            }
            catch (System.Exception ex2)
            {
                Debug.LogError($"[MapManager] Emergency recovery also failed: {ex2.Message}");
            }
        }
    }

    /// <summary>
    /// 계단 도달 시 다음 층(B1, B2 ...)을 생성
    /// </summary>
    public void GenerateNextFloor()
    {
        DungeonPersistentData.currentFloor++;
        DungeonPersistentData.hasSavedState = false;
        DungeonPersistentData.revealedTiles.Clear();
        DungeonPersistentData.lastPlayerGridPos = Vector2Int.zero;
        DungeonPersistentData.lastPlayerFacing = DungeonDirection.North;
        DungeonPersistentData.currentSeed = Random.Range(int.MinValue, int.MaxValue);

        // 벽선 라인도 초기화
        Genesis01.Dungeon.DungeonWallLineDrawer drawer = FindObjectOfType<Genesis01.Dungeon.DungeonWallLineDrawer>();
        if (drawer != null)
        {
            drawer.ResetLines();
        }

        // 타일맵/플레이어 상태를 완전히 초기화하여 이전 층 정보가 남지 않게 함
        if (fogTilemap != null) fogTilemap.ClearAllTiles();
        if (floorTilemap != null) floorTilemap.ClearAllTiles();
        if (wallTilemap != null) wallTilemap.ClearAllTiles();

        DungeonGridPlayer player = FindObjectOfType<DungeonGridPlayer>();
        if (player != null)
        {
            player.gridPos = Vector2Int.zero;
            player.facing = DungeonDirection.North;
        }

        Debug.Log($"[MapManager] Descend to floor B{DungeonPersistentData.currentFloor} with Seed {DungeonPersistentData.currentSeed}");

        GenerateMaze();
        DrawMap();
        PlacePlayer();

        // 새 맵 기반으로 벽선 재생성
        if (drawer != null)
        {
            drawer.DrawWalls();
        }
    }


    void GenerateMaze()
    {
        Random.InitState(DungeonPersistentData.currentSeed);
        
        width = Random.Range(minWidth, maxWidth + 1);
        height = Random.Range(minHeight, maxHeight + 1);
        mapData = new int[height, width];
        isRoom = new bool[height, width];

        // 1. Initialize walls
        Debug.Log($"[MapManager] Initializing {width}x{height} grid with walls.");
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                mapData[y, x] = 1;
                isRoom[y, x] = false;
            }
        }

        // 2. 방 생성
        Debug.Log("[MapManager] Step 2: Generating rooms...");
        GenerateRooms();
        
        // 3. 방들 연결
        Debug.Log("[MapManager] Step 3: Connecting rooms...");
        ConnectRooms();

        // 4. 나머지 공간에 미로 생성 (방과 통로와 연결되도록)
        Debug.Log("[MapManager] Step 4: Generating maze connections...");
        GenerateMazeConnections();

        // 5. 시작 위치 설정 (첫 번째 방의 중심)
        if (rooms.Count > 0)
        {
            var firstRoom = rooms[0];
            startPos = new Vector2Int(
                firstRoom.rect.x + firstRoom.rect.width / 2,
                firstRoom.rect.y + firstRoom.rect.height / 2
            );
        }
        else
        {
            // 방이 없으면 기존 로직 사용
            startPos = new Vector2Int(width / 2, height / 2);
            if (startPos.x % 2 == 0) startPos.x++;
            if (startPos.y % 2 == 0) startPos.y++;

            if (startPos.x >= width) startPos.x -= 2;
            if (startPos.y >= height) startPos.y -= 2;

            Carve(startPos.x, startPos.y);
        }

        // 6. 출구(다음 층) 위치 선정: 시작점에서 가장 먼 길(0) 셀
        exitPos = FindFarthestFloorCell(startPos);
        Debug.Log($"[MapManager] Start at {startPos}, Exit at {exitPos}");
    }

    void GenerateRooms()
    {
        rooms.Clear();
        int roomCount = Random.Range(minRoomCount, maxRoomCount + 1);
        int attempts = 0;
        int targetSpecialRoomCount = Mathf.Max(1, Random.Range(minSpecialRoomCount, maxSpecialRoomCount + 1));
        int specialRoomCreatedCount = 0;
        int maxAttempts = 500; // 시도 횟수를 대폭 늘림

        for (int i = 0; i < roomCount && attempts < maxAttempts; attempts++)
        {
            // 랜덤 방 크기
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);

            // 랜덤 위치 (경계 고려)
            // 홀수 좌표를 사용하여 미로 그리드와 일치시킴 (중요!)
            // 범위 체크 추가: 최소값이 최대값보다 작아야 함
            // x + roomWidth <= width, y + roomHeight <= height 보장 필요
            int maxX = width - roomWidth - 1; // x + roomWidth가 width를 넘지 않도록
            int maxY = height - roomHeight - 1; // y + roomHeight가 height를 넘지 않도록
            
            if (maxX < 1 || maxY < 1)
            {
                Debug.LogWarning($"[MapManager] 맵 크기가 너무 작거나 방이 너무 큼. width={width}, height={height}, roomWidth={roomWidth}, roomHeight={roomHeight}");
                break; // 더 이상 방을 생성할 수 없음
            }
            
            // 홀수 좌표로 제한하면서 범위 내에 있도록
            int maxXRange = maxX / 2;
            int maxYRange = maxY / 2;
            
            if (maxXRange < 1 || maxYRange < 1)
            {
                Debug.LogWarning($"[MapManager] 홀수 좌표 제약으로 인해 방을 생성할 수 없음. maxX={maxX}, maxY={maxY}");
                break;
            }
            
            int x = Random.Range(1, maxXRange + 1) * 2 + 1;
            int y = Random.Range(1, maxYRange + 1) * 2 + 1;
            
            // 최종 범위 체크 (안전장치)
            if (x + roomWidth > width || y + roomHeight > height)
            {
                Debug.LogWarning($"[MapManager] 계산된 방 위치가 범위를 벗어남. x={x}, y={y}, roomWidth={roomWidth}, roomHeight={roomHeight}, width={width}, height={height}");
                continue; // 이번 시도는 건너뛰고 다시 시도
            }

            RectInt newRoomRect = new RectInt(x, y, roomWidth, roomHeight);

            // 기존 방과 겹치는지 체크
            bool overlaps = false;
            foreach (var room in rooms)
            {
                // 간격을 고려한 확장된 영역 체크
                RectInt expanded = new RectInt(
                    room.rect.x - roomSpacing,
                    room.rect.y - roomSpacing,
                    room.rect.width + roomSpacing * 2,
                    room.rect.height + roomSpacing * 2
                );
                
                if (expanded.Overlaps(newRoomRect))
                {
                    overlaps = true;
                    break;
                }
            }

            // 겹치지 않으면 추가
            if (!overlaps)
            {
                // 특수 방(봉인된 방)을 우선적으로 생성, 나머지는 확률에 따라 결정
                bool isSealed = false;
                if (specialRoomCreatedCount < targetSpecialRoomCount)
                {
                    isSealed = true;
                    specialRoomCreatedCount++;
                }
                else
                {
                    isSealed = Random.value < sealedRoomChance;
                }

                RoomInfo newRoom = new RoomInfo(newRoomRect, isSealed);

                // 방 내부를 길(0)로 만들고 보호 구역으로 설정
                // 경계 체크 추가: 배열 범위를 벗어나지 않도록
                for (int ry = newRoom.rect.y; ry < newRoom.rect.y + newRoom.rect.height; ry++)
                {
                    if (ry < 0 || ry >= height) continue; // y 범위 체크
                    for (int rx = newRoom.rect.x; rx < newRoom.rect.x + newRoom.rect.width; rx++)
                    {
                        if (rx < 0 || rx >= width) continue; // x 범위 체크
                        mapData[ry, rx] = 0;
                        isRoom[ry, rx] = true;
                    }
                }

                // 완전히 닫힌 방인 경우: 방 주변을 벽으로 둘러싸고 문 하나만 만들기
                if (isSealed)
                {
                    CreateSealedRoom(newRoom);
                }

                rooms.Add(newRoom);
                i++; // 성공한 방 개수 증가
            }
        }

        int sealedCount = 0;
        foreach (var room in rooms)
        {
            if (room.isSealed) sealedCount++;
        }
        Debug.Log($"생성된 방 개수: {rooms.Count} (일반: {rooms.Count - sealedCount}개, 닫힌 방: {sealedCount}개)");
    }

    void CreateSealedRoom(RoomInfo room)
    {
        // 방 주변을 벽으로 둘러싸고 보호 구역으로 설정
        // 북쪽 벽
        for (int x = room.rect.x - 1; x <= room.rect.x + room.rect.width; x++)
        {
            if (x >= 0 && x < width && room.rect.y - 1 >= 0 && room.rect.y - 1 < height)
            {
                mapData[room.rect.y - 1, x] = 1;
                isRoom[room.rect.y - 1, x] = true;
            }
        }
        // 남쪽 벽
        for (int x = room.rect.x - 1; x <= room.rect.x + room.rect.width; x++)
        {
            if (x >= 0 && x < width && room.rect.y + room.rect.height >= 0 && room.rect.y + room.rect.height < height)
            {
                mapData[room.rect.y + room.rect.height, x] = 1;
                isRoom[room.rect.y + room.rect.height, x] = true;
            }
        }
        // 서쪽 벽
        for (int y = room.rect.y - 1; y <= room.rect.y + room.rect.height; y++)
        {
            if (room.rect.x - 1 >= 0 && room.rect.x - 1 < width && y >= 0 && y < height)
            {
                mapData[y, room.rect.x - 1] = 1;
                isRoom[y, room.rect.x - 1] = true;
            }
        }
        // 동쪽 벽
        for (int y = room.rect.y - 1; y <= room.rect.y + room.rect.height; y++)
        {
            if (room.rect.x + room.rect.width >= 0 && room.rect.x + room.rect.width < width && y >= 0 && y < height)
            {
                mapData[y, room.rect.x + room.rect.width] = 1;
                isRoom[y, room.rect.x + room.rect.width] = true;
            }
        }

        // 문 하나 만들기 (랜덤한 한 면에)
        // 0: 북, 1: 동, 2: 남, 3: 서
        // 범위 내에 있는 면만 선택하도록 시도
        List<int> validSides = new List<int>();
        if (room.rect.y > 0) validSides.Add(0); // 북쪽 가능
        if (room.rect.x + room.rect.width < width) validSides.Add(1); // 동쪽 가능
        if (room.rect.y + room.rect.height < height) validSides.Add(2); // 남쪽 가능
        if (room.rect.x > 0) validSides.Add(3); // 서쪽 가능
        
        if (validSides.Count == 0)
        {
            Debug.LogWarning($"[MapManager] CreateSealedRoom: 방이 맵 경계에 너무 가까워 문을 만들 수 없음. room={room.rect}");
            return; // 문을 만들 수 없으면 그냥 반환
        }
        
        int doorSide = validSides[Random.Range(0, validSides.Count)];
        Vector2Int doorPos = Vector2Int.zero;

        switch (doorSide)
        {
            case 0: // 북쪽
                doorPos = new Vector2Int(
                    room.rect.x + Random.Range(0, room.rect.width),
                    room.rect.y - 1
                );
                break;
            case 1: // 동쪽
                doorPos = new Vector2Int(
                    room.rect.x + room.rect.width,
                    room.rect.y + Random.Range(0, room.rect.height)
                );
                break;
            case 2: // 남쪽
                doorPos = new Vector2Int(
                    room.rect.x + Random.Range(0, room.rect.width),
                    room.rect.y + room.rect.height
                );
                break;
            case 3: // 서쪽
                doorPos = new Vector2Int(
                    room.rect.x - 1,
                    room.rect.y + Random.Range(0, room.rect.height)
                );
                break;
        }

        // 문 위치를 길(0)로 만들고 보호 해제 (통로와 연결될 수 있도록)
        // 추가 안전장치: 범위 체크
        if (doorPos.x >= 0 && doorPos.x < width && doorPos.y >= 0 && doorPos.y < height)
        {
            mapData[doorPos.y, doorPos.x] = 0;
            isRoom[doorPos.y, doorPos.x] = false; // 문은 보호하지 않음
            room.doorPos = doorPos;
        }
        else
        {
            Debug.LogWarning($"[MapManager] CreateSealedRoom: 계산된 문 위치가 범위를 벗어남. doorPos={doorPos}, width={width}, height={height}");
        }
    }

    void ConnectRooms()
    {
        Debug.Log($"ConnectRooms 시작 - 방 개수: {rooms.Count}");
        
        if (rooms.Count < 2)
        {
            Debug.Log("방이 2개 미만이므로 연결 스킵");
            // 방이 1개만 있어도 연결 체크는 해야 함
            EnsureAllConnected();
            return;
        }

        // 각 방의 중심점 계산 (닫힌 방은 문 위치 사용)
        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in rooms)
        {
            if (room.isSealed && room.doorPos != Vector2Int.zero)
            {
                // 닫힌 방은 문 위치를 연결점으로 사용
                roomCenters.Add(room.doorPos);
            }
            else
            {
                // 일반 방은 중심점 사용
                roomCenters.Add(new Vector2Int(
                    room.rect.x + room.rect.width / 2,
                    room.rect.y + room.rect.height / 2
                ));
            }
        }
        
        Debug.Log($"방 중심점 계산 완료: {roomCenters.Count}개");

        // 모든 방을 연결하는 최소 신장 트리 방식
        // 이미 연결된 방들을 추적
        List<int> connected = new List<int> { 0 }; // 첫 번째 방은 이미 연결됨
        List<int> unconnected = new List<int>();
        for (int i = 1; i < roomCenters.Count; i++)
        {
            unconnected.Add(i);
        }

        // 모든 방이 연결될 때까지 반복
        int safety = 0;
        int maxSafety = roomCenters.Count * 2;
        
        Debug.Log($"[MapManager] Starting room connection loop. Unconnected rooms: {unconnected.Count}");
        
        while (unconnected.Count > 0 && safety < maxSafety)
        {
            safety++;
            float minDistance = float.MaxValue;
            int closestConnected = -1;
            int closestUnconnected = -1;

            // 연결된 방과 연결되지 않은 방 중 가장 가까운 쌍 찾기
            foreach (int connectedIdx in connected)
            {
                foreach (int unconnectedIdx in unconnected)
                {
                    float dist = Vector2Int.Distance(roomCenters[connectedIdx], roomCenters[unconnectedIdx]);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestConnected = connectedIdx;
                        closestUnconnected = unconnectedIdx;
                    }
                }
            }

            // 가장 가까운 방들을 연결
            if (closestConnected >= 0 && closestUnconnected >= 0)
            {
                Debug.Log($"[MapManager] Connecting room {closestConnected} to room {closestUnconnected} (Distance: {minDistance})");
                CreateCorridor(roomCenters[closestConnected], roomCenters[closestUnconnected]);
                connected.Add(closestUnconnected);
                unconnected.Remove(closestUnconnected);
            }
            else
            {
                Debug.LogWarning("[MapManager] 방 연결 실패 - 가장 가까운 방을 찾을 수 없음");
                break; // 연결 실패 시 중단
            }
        }
        
        if (safety >= maxSafety)
        {
            Debug.LogError($"[MapManager] ConnectRooms safety break triggered! (safety: {safety}, maxSafety: {maxSafety})");
        }
        Debug.Log($"방 연결 완료 - 연결된 방: {connected.Count}개");
        
        // 모든 길(0) 셀이 연결되어 있는지 확인하고, 고립된 영역이 있으면 연결
        EnsureAllConnected();
    }

    void EnsureAllConnected()
    {
        Debug.Log("=== EnsureAllConnected 시작 ===");
        
        // 모든 길(0) 셀 찾기
        List<Vector2Int> allFloorCells = new List<Vector2Int>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mapData[y, x] == 0)
                {
                    allFloorCells.Add(new Vector2Int(x, y));
                }
            }
        }

        Debug.Log($"총 길(0) 셀 개수: {allFloorCells.Count}");
        
        if (allFloorCells.Count == 0)
        {
            Debug.LogWarning("길(0) 셀이 하나도 없습니다!");
            return;
        }

        // BFS로 연결된 영역 찾기
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        foreach (var cell in allFloorCells)
        {
            if (visited.Contains(cell)) continue;

            // 새로운 영역 발견
            List<Vector2Int> region = new List<Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(cell);
            visited.Add(cell);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                region.Add(current);

                // 4방향 체크 (경계 체크 완화)
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                foreach (var dir in dirs)
                {
                    Vector2Int next = current + dir;
                    if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height &&
                        mapData[next.y, next.x] == 0 && 
                        !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            regions.Add(region);
        }

        // 여러 영역이 있으면 연결 (최대 10번 반복으로 무한 루프 방지)
        int maxIterations = 10;
        int iteration = 0;
        
        while (regions.Count > 1 && iteration < maxIterations)
        {
            iteration++;
            Debug.Log($"고립된 영역 발견: {regions.Count}개, 연결 중... (반복 {iteration})");
            
            // 첫 번째 영역을 기준으로 나머지 영역들을 연결
            for (int i = 1; i < regions.Count; i++)
            {
                // 각 영역에서 가장 가까운 셀 찾기
                Vector2Int closest1 = regions[0][0];
                Vector2Int closest2 = regions[i][0];
                float minDist = float.MaxValue;

                foreach (var cell1 in regions[0])
                {
                    foreach (var cell2 in regions[i])
                    {
                        // 실제 거리가 아니라 택시 거리를 사용하여 복도와 더 잘 어울리게 함
                        float dist = Mathf.Abs(cell1.x - cell2.x) + Mathf.Abs(cell1.y - cell2.y);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closest1 = cell1;
                            closest2 = cell2;
                        }
                    }
                }

                // 두 영역을 통로로 연결
                CreateCorridor(closest1, closest2);
            }

            // CreateCorridor 호출 후 맵이 변경되었으므로 allFloorCells를 다시 계산해야 함
            allFloorCells.Clear();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mapData[y, x] == 0)
                    {
                        allFloorCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            // 다시 영역 체크
            visited.Clear();
            regions.Clear();
            
            foreach (var cell in allFloorCells)
            {
                if (visited.Contains(cell)) continue;

                List<Vector2Int> region = new List<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                queue.Enqueue(cell);
                visited.Add(cell);

                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    region.Add(current);

                    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (var dir in dirs)
                    {
                        Vector2Int next = current + dir;
                        if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height &&
                            mapData[next.y, next.x] == 0 && 
                            !visited.Contains(next))
                        {
                            visited.Add(next);
                            queue.Enqueue(next);
                        }
                    }
                }

                regions.Add(region);
            }
        }

        Debug.Log($"최종 영역 개수: {regions.Count}");
        
        if (regions.Count > 1)
        {
            Debug.LogWarning($"경고: {regions.Count}개의 고립된 영역이 남아있습니다.");
        }
        else if (regions.Count == 1)
        {
            Debug.Log("✅ 모든 영역이 연결되었습니다!");
        }
        else
        {
            Debug.LogWarning("경고: 영역이 없습니다!");
        }
        
        Debug.Log("=== EnsureAllConnected 완료 ===");
    }

    void GenerateMazeConnections()
    {
        Debug.Log("=== GenerateMazeConnections 시작 ===");
        
        // 맵 전체를 순회하며 아직 방문하지 않은(벽인) 홀수 좌표에서 미로 시작
        for (int y = 1; y < height - 1; y += 2)
        {
            for (int x = 1; x < width - 1; x += 2)
            {
                if (mapData[y, x] == 1 && !isRoom[y, x])
                {
                    Carve(x, y);
                }
            }
        }
        
        Debug.Log("=== GenerateMazeConnections 완료 ===");
    }

    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        // L자 경로로 연결 (수평 먼저, 그 다음 수직)
        // 수평 경로
        int xStart = Mathf.Min(start.x, end.x);
        int xEnd = Mathf.Max(start.x, end.x);
        for (int x = xStart; x <= xEnd; x++)
        {
            if (x >= 0 && x < width && start.y >= 0 && start.y < height)
            {
                // 복도는 보호 구역을 무시함 (모든 구역의 연결이 우선)
                mapData[start.y, x] = 0;
            }
        }

        // 수직 경로
        int yStart = Mathf.Min(start.y, end.y);
        int yEnd = Mathf.Max(start.y, end.y);
        for (int y = yStart; y <= yEnd; y++)
        {
            if (end.x >= 0 && end.x < width && y >= 0 && y < height)
            {
                // 복도는 보호 구역을 무시함 (모든 구역의 연결이 우선)
                mapData[y, end.x] = 0;
            }
        }
    }

    void PlacePlayer()
    {
        try
        {
            // 3. Teleport Player (Safe to do here because DrawMap has already filled the fog)
            DungeonGridPlayer player = Object.FindFirstObjectByType<DungeonGridPlayer>();
            if (player != null)
            {
                if (fogTilemap != null) player.fogTilemap = fogTilemap;
                
                if (DungeonPersistentData.hasSavedState)
                {
                    // 저장된 위치가 유효한지 확인
                    if (DungeonPersistentData.lastPlayerGridPos.x >= 0 && 
                        DungeonPersistentData.lastPlayerGridPos.y >= 0 &&
                        DungeonPersistentData.lastPlayerGridPos.x < width &&
                        DungeonPersistentData.lastPlayerGridPos.y < height)
                    {
                        player.Teleport(DungeonPersistentData.lastPlayerGridPos);
                        player.facing = DungeonPersistentData.lastPlayerFacing;
                        Debug.Log($"[MapManager] Restored player to ({DungeonPersistentData.lastPlayerGridPos.x}, {DungeonPersistentData.lastPlayerGridPos.y}) facing {player.facing}");
                    }
                    else
                    {
                        Debug.LogWarning($"[MapManager] Saved position invalid, using startPos instead. Saved: {DungeonPersistentData.lastPlayerGridPos}, Start: {startPos}");
                        player.Teleport(startPos);
                    }
                }
                else
                {
                    // startPos가 유효한지 확인
                    if (startPos.x >= 0 && startPos.y >= 0 && startPos.x < width && startPos.y < height)
                    {
                        player.Teleport(startPos);
                        Debug.Log($"[MapManager] Placing player at start: {startPos}");
                    }
                    else
                    {
                        Debug.LogWarning($"[MapManager] startPos invalid ({startPos}), using default position (1, 1)");
                        player.Teleport(new Vector2Int(1, 1));
                    }
                }
            }
            else
            {
                Debug.LogWarning("No DungeonGridPlayer found to place at start.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MapManager] Error in PlacePlayer(): {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    void Carve(int x, int y)
    {
        // 범위 체크
        if (!InBounds(x, y))
        {
            Debug.LogWarning($"[MapManager] Carve: 범위를 벗어난 좌표. x={x}, y={y}");
            return;
        }
        
        mapData[y, x] = 0; 

        List<Vector2Int> dirs = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int i = 0; i < dirs.Count; i++)
        {
            Vector2Int temp = dirs[i];
            int rand = Random.Range(i, dirs.Count);
            dirs[i] = dirs[rand];
            dirs[rand] = temp;
        }

        foreach (var dir in dirs)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;
            int midX = x + dir.x;
            int midY = y + dir.y;

            // 보호된 타일(isRoom)은 파고들지 않음
            if (InBounds(nx, ny) && mapData[ny, nx] == 1 && !isRoom[ny, nx])
            {
                // 중간 벽도 범위 체크 후 제거
                if (InBounds(midX, midY))
                {
                    mapData[midY, midX] = 0;
                }
                Carve(nx, ny);
            }
        }
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    void DrawMap()
    {
        // null 체크
        if (floorTilemap == null)
        {
            Debug.LogError("[MapManager] DrawMap: floorTilemap is null!");
            return;
        }
        if (wallTilemap == null)
        {
            Debug.LogError("[MapManager] DrawMap: wallTilemap is null!");
            return;
        }
        if (mapData == null)
        {
            Debug.LogError("[MapManager] DrawMap: mapData is null! GenerateMaze() must be called first.");
            return;
        }
        
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        if (fogTilemap != null) fogTilemap.ClearAllTiles();

        int floorCount = 0;
        int wallCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int pos = new Vector3Int(x, -y, 0);

                bool isExit = (x == exitPos.x && y == exitPos.y);

                if (fogTilemap != null && fogTile != null)
                {
                    // 탐험된 영역인지 확인하여 안개 설정
                    if (DungeonPersistentData.revealedTiles.Contains(new Vector2Int(x, y)))
                    {
                        fogTilemap.SetTile(pos, null);
                    }
                    else
                    {
                        // 안개 유지 (출구도 가리는 것이 기본)
                        fogTilemap.SetTile(pos, fogTile);
                    }
                }

                if (mapData[y, x] == 1)
                {
                    wallTilemap.SetTile(pos, wallTile);
                    wallCount++;
                }
                else
                {
                    // 기본 바닥을 깐다 (이후 출구면 다른 타일로 덮을 수 있음)
                    floorTilemap.SetTile(pos, floorTile);
                    // 색상 적용을 위해 LockColor 해제 (기본 바닥)
                    floorTilemap.SetTileFlags(pos, TileFlags.None);

                    // 출구 표시 (계단 타일 또는 색상 하이라이트)
                    if (isExit)
                    {
                        // 밝은 전용 타일이 있으면 우선 사용, 없으면 계단/바닥 순으로 선택
                        if (exitHighlightTile != null)
                        {
                            floorTilemap.SetTile(pos, exitHighlightTile);
                        }
                        else if (stairsTile != null)
                        {
                            floorTilemap.SetTile(pos, stairsTile);
                        }

                        // 계단 타일을 덮었으므로 다시 LockColor 해제
                        floorTilemap.SetTileFlags(pos, TileFlags.None);

                        if (highlightExit)
                        {
                            // 계단 타일이 있든 없든 색상 적용 (계단 스프라이트가 단색이면 덮어씀)
                            floorTilemap.SetColor(pos, exitColor);
                            // RefreshTile은 루프 끝에서 한 번만 호출하도록 최적화
                            Debug.Log($"[MapManager] Exit highlighted at {exitPos} with color {exitColor}");
                        }
                        else
                        {
                            floorTilemap.SetColor(pos, Color.white);
                            // RefreshTile은 루프 끝에서 한 번만 호출하도록 최적화
                        }
                    }
                    else
                    {
                        // 일반 바닥은 기본색으로
                        floorTilemap.SetColor(pos, Color.white);
                        // RefreshTile은 루프 끝에서 한 번만 호출하도록 최적화
                    }
                    floorCount++;
                }
            }
        }

        // 최종적으로 출구 색상을 다시 한 번 강제 적용 (혹시 중간에 덮였을 경우 대비)
        ForceExitHighlight();
        
        // 모든 타일 업데이트를 한 번에 처리 (성능 최적화)
        floorTilemap.RefreshAllTiles();
        wallTilemap.RefreshAllTiles();
        if (fogTilemap != null) fogTilemap.RefreshAllTiles();

        Debug.Log($"맵 생성 완료: 크기 {width}x{height} = {width * height}개 셀 (길: {floorCount}개, 벽: {wallCount}개)");
    }

    void ForceExitHighlight()
    {
        if (!highlightExit || floorTilemap == null) return;
        if (!InBounds(exitPos.x, exitPos.y)) return;

        Vector3Int pos = new Vector3Int(exitPos.x, -exitPos.y, 0);

        // 바닥 없으면 바닥부터
        if (!floorTilemap.HasTile(pos))
        {
            TileBase baseTile = exitHighlightTile != null ? exitHighlightTile : (stairsTile != null ? stairsTile : floorTile);
            floorTilemap.SetTile(pos, baseTile);
        }
        else if (exitHighlightTile != null)
        {
            floorTilemap.SetTile(pos, exitHighlightTile);
        }
        else if (stairsTile != null)
        {
            // 계단을 사용하도록 덮기
            floorTilemap.SetTile(pos, stairsTile);
        }

        // 색 적용을 위해 LockColor 해제 후 색상 강제
        floorTilemap.SetTileFlags(pos, TileFlags.None);

        // 알파가 0인 경우 대비: 최소 0.9로 보정
        Color c = exitColor;
        if (c.a < 0.05f) c.a = 1f;
        floorTilemap.SetColor(pos, c);

        // RefreshAllTiles는 DrawMap()에서 이미 호출되므로 여기서는 생략
        Debug.Log($"[MapManager] ForceExitHighlight at {exitPos} color {c}");
    }

    // 시작 지점에서 가장 먼 길(0) 셀 찾기 (맨해튼 거리 기준 BFS)
    Vector2Int FindFarthestFloorCell(Vector2Int start)
    {
        if (!InBounds(start.x, start.y) || mapData[start.y, start.x] != 0)
        {
            Debug.LogWarning($"[MapManager] FindFarthestFloorCell: 시작 위치가 유효하지 않음. startPos 반환: {startPos}");
            return startPos;
        }

        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> dist = new Dictionary<Vector2Int, int>();
        q.Enqueue(start);
        dist[start] = 0;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Vector2Int far = start;
        int maxD = 0;
        
        // 안전장치: 최대 탐색 횟수 제한 (맵 크기의 2배)
        int maxSearchCount = width * height * 2;
        int searchCount = 0;

        while (q.Count > 0 && searchCount < maxSearchCount)
        {
            searchCount++;
            var cur = q.Dequeue();
            int d = dist[cur];
            if (d > maxD)
            {
                maxD = d;
                far = cur;
            }

            foreach (var dir in dirs)
            {
                Vector2Int next = cur + dir;
                if (!InBounds(next.x, next.y)) continue;
                if (mapData[next.y, next.x] != 0) continue;
                if (dist.ContainsKey(next)) continue;
                dist[next] = d + 1;
                q.Enqueue(next);
            }
        }
        
        if (searchCount >= maxSearchCount)
        {
            Debug.LogWarning($"[MapManager] FindFarthestFloorCell: 최대 탐색 횟수 도달. 현재까지 찾은 가장 먼 위치 반환: {far}");
        }

        return far;
    }
}
