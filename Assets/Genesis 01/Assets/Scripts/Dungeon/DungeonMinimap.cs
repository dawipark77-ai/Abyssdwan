using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 미니맵 시스템 관리자
/// 배경은 고정되고 미니맵에서만 탐험하는 시스템
/// </summary>
public class DungeonMinimap : MonoBehaviour
{
    [Header("미니맵 설정")]
    public Camera minimapCamera;
    public RectTransform minimapViewport;
    public RectTransform minimapContent;
    public GameObject playerIconPrefab;
    public GameObject roomIconPrefab;
    public GameObject exploredRoomIconPrefab;
    
    [Header("던전 설정")]
    public int dungeonWidth = 10;
    public int dungeonHeight = 10;
    public float roomSize = 1f;
    public float minimapScale = 0.1f;
    public bool useDungeonGenerator = true;
    public DungeonGenerator dungeonGenerator;
    
    [Header("배경 설정")]
    public SpriteRenderer backgroundRenderer;
    public Sprite dungeonBackground;
    
    [Header("이벤트 관리")]
    public DungeonEventManager eventManager;
    
    private Dictionary<Vector2Int, DungeonRoom> rooms = new Dictionary<Vector2Int, DungeonRoom>();
    private Dictionary<Vector2Int, GameObject> roomIcons = new Dictionary<Vector2Int, GameObject>();
    private GameObject playerIcon;
    private Vector2Int currentPlayerPosition;
    private HashSet<Vector2Int> exploredRooms = new HashSet<Vector2Int>();
    
    private void Start()
    {
        // 이벤트 매니저 찾기
        if (eventManager == null)
        {
            eventManager = FindFirstObjectByType<DungeonEventManager>();
        }
        
        InitializeDungeon();
        CreatePlayerIcon();
        UpdateMinimap();
    }
    
    /// <summary>
    /// 던전 초기화 - 방들을 생성
    /// </summary>
    private void InitializeDungeon()
    {
        // 던전 생성기 사용
        if (useDungeonGenerator && dungeonGenerator != null)
        {
            rooms = dungeonGenerator.CreateDungeon();
            
            // 생성된 방들에 대해 아이콘 생성
            foreach (var kvp in rooms)
            {
                CreateRoomIcon(kvp.Key);
            }
            
            // 시작 위치 찾기 (첫 번째 방)
            if (rooms.Count > 0)
            {
                currentPlayerPosition = new List<Vector2Int>(rooms.Keys)[0];
            }
            else
            {
                currentPlayerPosition = new Vector2Int(0, 0);
            }
        }
        else
        {
            // 기본 던전 구조 생성 (예시: 미로 형태)
            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int y = 0; y < dungeonHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    DungeonRoom room = new DungeonRoom(pos);
                    rooms[pos] = room;
                    
                    // 방 아이콘 생성
                    CreateRoomIcon(pos);
                }
            }
            
            // 시작 위치 설정
            currentPlayerPosition = new Vector2Int(0, 0);
        }
        
        ExploreRoom(currentPlayerPosition);
        
        // 배경 설정
        if (backgroundRenderer != null && dungeonBackground != null)
        {
            backgroundRenderer.sprite = dungeonBackground;
        }
    }
    
    /// <summary>
    /// 방 아이콘 생성
    /// </summary>
    private void CreateRoomIcon(Vector2Int position)
    {
        if (roomIconPrefab == null || minimapContent == null) return;
        
        Vector3 worldPos = new Vector3(position.x * roomSize, position.y * roomSize, 0);
        GameObject icon = Instantiate(roomIconPrefab, worldPos, Quaternion.identity, minimapContent);
        icon.name = $"Room_{position.x}_{position.y}";
        
        // 초기에는 숨김 처리 (탐험하지 않은 방)
        icon.SetActive(false);
        
        roomIcons[position] = icon;
    }
    
    /// <summary>
    /// 플레이어 아이콘 생성
    /// </summary>
    private void CreatePlayerIcon()
    {
        if (playerIconPrefab == null || minimapContent == null) return;
        
        Vector3 startPos = new Vector3(currentPlayerPosition.x * roomSize, currentPlayerPosition.y * roomSize, -0.1f);
        playerIcon = Instantiate(playerIconPrefab, startPos, Quaternion.identity, minimapContent);
        playerIcon.name = "PlayerIcon";
    }
    
    /// <summary>
    /// 플레이어 위치 업데이트
    /// </summary>
    public void MovePlayer(Vector2Int newPosition)
    {
        if (!IsValidPosition(newPosition)) return;
        
        // 이동 방향 계산
        Vector2Int moveDirection = newPosition - currentPlayerPosition;
        
        // 방향이 있으면 미니맵 컨트롤러에 방향 업데이트
        if (moveDirection != Vector2Int.zero && MiniMapController.Instance != null)
        {
            Vector2 directionVector = new Vector2(moveDirection.x, moveDirection.y);
            MiniMapController.Instance.UpdatePlayerRotation(directionVector);
        }
        
        currentPlayerPosition = newPosition;
        
        // 플레이어 아이콘 위치 업데이트
        if (playerIcon != null)
        {
            Vector3 newPos = new Vector3(newPosition.x * roomSize, newPosition.y * roomSize, -0.1f);
            playerIcon.transform.localPosition = newPos;
        }
        
        // MiniMapController에도 위치 업데이트
        if (MiniMapController.Instance != null)
        {
            MiniMapController.Instance.UpdatePlayerPosition(newPosition);
        }
        
        // 방 탐험
        ExploreRoom(newPosition);
        
        // 이벤트 처리
        if (rooms.ContainsKey(newPosition))
        {
            DungeonRoom room = rooms[newPosition];
            if (eventManager != null)
            {
                eventManager.HandleRoomEntered(room);
            }
        }
        
        // 미니맵 업데이트
        UpdateMinimap();
    }
    
    /// <summary>
    /// 방 탐험 처리
    /// </summary>
    private void ExploreRoom(Vector2Int position)
    {
        if (exploredRooms.Contains(position)) return;
        
        exploredRooms.Add(position);
        
        // 주변 방들도 약간 밝게 표시 (시야 효과)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int nearbyPos = position + new Vector2Int(dx, dy);
                if (IsValidPosition(nearbyPos))
                {
                    exploredRooms.Add(nearbyPos);
                }
            }
        }
    }
    
    /// <summary>
    /// 미니맵 UI 업데이트
    /// </summary>
    private void UpdateMinimap()
    {
        foreach (var kvp in roomIcons)
        {
            Vector2Int pos = kvp.Key;
            GameObject icon = kvp.Value;
            
            if (exploredRooms.Contains(pos))
            {
                icon.SetActive(true);
                
                // 현재 위치의 방은 더 밝게
                if (pos == currentPlayerPosition)
                {
                    SetIconColor(icon, Color.yellow);
                }
                else if (rooms.ContainsKey(pos))
                {
                    DungeonRoom room = rooms[pos];
                    // 적이 있는 방은 빨간색
                    if (room.hasEnemy)
                    {
                        SetIconColor(icon, Color.red);
                    }
                    // 보물이 있는 방은 초록색
                    else if (room.hasTreasure)
                    {
                        SetIconColor(icon, Color.green);
                    }
                    // 출구는 파란색
                    else if (room.hasExit)
                    {
                        SetIconColor(icon, Color.blue);
                    }
                    else
                    {
                        SetIconColor(icon, Color.gray);
                    }
                }
                else
                {
                    SetIconColor(icon, Color.gray);
                }
            }
            else
            {
                icon.SetActive(false);
            }
        }
        
        // 미니맵 카메라를 플레이어 위치로 이동
        if (minimapCamera != null)
        {
            Vector3 cameraPos = new Vector3(
                currentPlayerPosition.x * roomSize,
                currentPlayerPosition.y * roomSize,
                minimapCamera.transform.position.z
            );
            minimapCamera.transform.position = cameraPos;
        }
    }
    
    /// <summary>
    /// 아이콘 색상 설정
    /// </summary>
    private void SetIconColor(GameObject icon, Color color)
    {
        SpriteRenderer sr = icon.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
        }
    }
    
    /// <summary>
    /// 유효한 위치인지 확인
    /// </summary>
    private bool IsValidPosition(Vector2Int position)
    {
        // 던전 생성기를 사용하는 경우, 생성된 방만 유효
        if (useDungeonGenerator)
        {
            return rooms.ContainsKey(position);
        }
        
        // 기본 범위 체크
        return position.x >= 0 && position.x < dungeonWidth &&
               position.y >= 0 && position.y < dungeonHeight;
    }
    
    /// <summary>
    /// 현재 플레이어 위치 반환
    /// </summary>
    public Vector2Int GetPlayerPosition()
    {
        return currentPlayerPosition;
    }
    
    /// <summary>
    /// 특정 위치로 이동 가능한지 확인
    /// </summary>
    public bool CanMoveTo(Vector2Int position)
    {
        // 방이 존재하는지 확인
        if (!rooms.ContainsKey(position)) return false;
        
        // 현재 위치에서 연결된 방인지 확인
        if (rooms.ContainsKey(currentPlayerPosition))
        {
            DungeonRoom currentRoom = rooms[currentPlayerPosition];
            if (currentRoom.IsConnectedTo(position))
            {
                return true;
            }
        }
        
        // 인접한 방이면 이동 가능 (간단한 버전)
        Vector2Int diff = position - currentPlayerPosition;
        if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) == 1)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 현재 방 정보 반환
    /// </summary>
    public DungeonRoom GetCurrentRoom()
    {
        if (rooms.ContainsKey(currentPlayerPosition))
        {
            return rooms[currentPlayerPosition];
        }
        return null;
    }
}

