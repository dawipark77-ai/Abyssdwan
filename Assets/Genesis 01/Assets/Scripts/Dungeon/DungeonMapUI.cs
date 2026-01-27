using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 던전 맵 UI 관리
/// F1 키로 맵 표시/숨김, 타일 렌더링
/// </summary>
public class DungeonMapUI : MonoBehaviour
{
    [Header("참조")]
    public DungeonMap dungeonMap;
    public Canvas mapCanvas;
    public RectTransform mapContainer;
    public GameObject tilePrefab; // 타일 프리팹 (Image 컴포넌트가 있는 GameObject)
    
    [Header("타일 설정")]
    public Color wallColor = Color.black;
    public Color pathColor = Color.gray;
    public Color exploredPathColor = Color.white;
    public Color startColor = Color.green;
    public Color exitColor = Color.red;
    public Color playerColor = Color.yellow;
    public float tileSize = 20f; // 타일 크기 (픽셀)
    
    [Header("맵 설정")]
    public KeyCode toggleKey = KeyCode.F1;
    public bool mapVisible = false;
    
    private GameObject[,] tileObjects;
    private int mapWidth;
    private int mapHeight;
    
    private void Start()
    {
        if (dungeonMap == null)
        {
            dungeonMap = Object.FindFirstObjectByType<DungeonMap>();
        }
        
        if (mapCanvas == null)
        {
            // Canvas 자동 생성
            GameObject canvasObj = new GameObject("DungeonMapCanvas");
            mapCanvas = canvasObj.AddComponent<Canvas>();
            mapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mapCanvas.sortingOrder = 100; // 다른 UI 위에 표시
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Container 생성
            GameObject containerObj = new GameObject("MapContainer");
            containerObj.transform.SetParent(canvasObj.transform, false);
            mapContainer = containerObj.AddComponent<RectTransform>();
            mapContainer.anchorMin = new Vector2(0.5f, 0.5f);
            mapContainer.anchorMax = new Vector2(0.5f, 0.5f);
            mapContainer.pivot = new Vector2(0.5f, 0.5f);
            mapContainer.sizeDelta = new Vector2(400, 400); // 20x20 * 20px
        }
        
        if (tilePrefab == null)
        {
            // 기본 타일 프리팹 생성
            tilePrefab = new GameObject("TilePrefab");
            Image image = tilePrefab.AddComponent<Image>();
            image.color = Color.white;
            RectTransform rect = tilePrefab.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(tileSize, tileSize);
        }
        
        InitializeMap();
        
        // 이벤트 구독
        if (dungeonMap != null)
        {
            dungeonMap.OnPlayerPositionChanged += OnPlayerMoved;
            dungeonMap.OnMapCleared += OnMapCleared;
        }
        
        // 초기에는 맵 숨김
        mapCanvas.gameObject.SetActive(false);
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (dungeonMap != null)
        {
            dungeonMap.OnPlayerPositionChanged -= OnPlayerMoved;
            dungeonMap.OnMapCleared -= OnMapCleared;
        }
    }
    
    /// <summary>
    /// 플레이어 이동 이벤트 핸들러
    /// </summary>
    private void OnPlayerMoved(Vector2Int position)
    {
        if (mapVisible)
        {
            UpdateMapDisplay();
        }
    }
    
    /// <summary>
    /// 맵 클리어 이벤트 핸들러
    /// </summary>
    private void OnMapCleared()
    {
        Debug.Log("[DungeonMapUI] 던전 클리어! 축하합니다!");
        // 필요시 클리어 UI 표시 등 추가 가능
    }
    
    private void Update()
    {
        // F1 키로 맵 토글
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMap();
        }
        
        // 맵이 보일 때만 업데이트
        if (mapVisible)
        {
            UpdateMapDisplay();
        }
    }
    
    /// <summary>
    /// 맵 초기화
    /// </summary>
    private void InitializeMap()
    {
        if (dungeonMap == null) return;
        
        mapWidth = dungeonMap.mapWidth;
        mapHeight = dungeonMap.mapHeight;
        
        tileObjects = new GameObject[mapWidth, mapHeight];
        
        // 컨테이너 크기 설정
        mapContainer.sizeDelta = new Vector2(mapWidth * tileSize, mapHeight * tileSize);
        
        // 타일 오브젝트 생성
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                GameObject tileObj = Instantiate(tilePrefab, mapContainer);
                tileObj.name = $"Tile_{x}_{y}";
                
                RectTransform rect = tileObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(x * tileSize, -y * tileSize);
                rect.sizeDelta = new Vector2(tileSize, tileSize);
                
                tileObjects[x, y] = tileObj;
            }
        }
        
        UpdateMapDisplay();
    }
    
    /// <summary>
    /// 맵 표시 토글
    /// </summary>
    public void ToggleMap()
    {
        mapVisible = !mapVisible;
        mapCanvas.gameObject.SetActive(mapVisible);
        
        if (mapVisible)
        {
            UpdateMapDisplay();
            Debug.Log("[DungeonMapUI] 맵 표시");
        }
        else
        {
            Debug.Log("[DungeonMapUI] 맵 숨김");
        }
    }
    
    /// <summary>
    /// 맵 표시 업데이트
    /// </summary>
    private void UpdateMapDisplay()
    {
        if (dungeonMap == null || tileObjects == null) return;
        
        DungeonMapTile[,] map = dungeonMap.GetMap();
        Vector2Int playerPos = dungeonMap.GetPlayerPosition();
        
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                GameObject tileObj = tileObjects[x, y];
                Image image = tileObj.GetComponent<Image>();
                
                if (image == null) continue;
                
                DungeonMapTile tile = map[x, y];
                Color tileColor = wallColor;
                
                // 플레이어 위치
                if (x == playerPos.x && y == playerPos.y)
                {
                    tileColor = playerColor;
                }
                // 탈출점
                else if (tile.tileType == DungeonMapTile.TileType.Exit)
                {
                    tileColor = exitColor;
                }
                // 시작점
                else if (tile.tileType == DungeonMapTile.TileType.Start)
                {
                    tileColor = startColor;
                }
                // 탐험한 길
                else if (tile.isExplored && tile.tileType == DungeonMapTile.TileType.Path)
                {
                    tileColor = exploredPathColor;
                }
                // 탐험하지 않은 길 (반투명)
                else if (tile.tileType == DungeonMapTile.TileType.Path)
                {
                    tileColor = pathColor;
                    tileColor.a = 0.3f; // 반투명
                }
                // 벽
                else
                {
                    tileColor = wallColor;
                    // 탐험하지 않은 벽은 완전히 투명
                    if (!tile.isExplored)
                    {
                        tileColor.a = 0f;
                    }
                }
                
                image.color = tileColor;
            }
        }
    }
    
    /// <summary>
    /// 맵 표시
    /// </summary>
    public void ShowMap()
    {
        if (!mapVisible)
        {
            ToggleMap();
        }
    }
    
    /// <summary>
    /// 맵 숨김
    /// </summary>
    public void HideMap()
    {
        if (mapVisible)
        {
            ToggleMap();
        }
    }
}

