using UnityEngine;

/// <summary>
/// 미니맵 컨트롤러
/// 플레이어 위치를 미니맵에 표시하는 시스템
/// </summary>
public class MiniMapController : MonoBehaviour
{
    [Header("미니맵 UI 참조")]
    [Tooltip("빨간색 플레이어 아이콘 (Playercursor 오브젝트를 여기에 드래그하세요)")]
    public RectTransform playerIcon;
    
    [Header("설정")]
    [Tooltip("던전 타일 크기 (미니맵에서 한 칸의 크기). 기본값 50, 필요시 조정")]
    public float cellSize = 50f;
    
    [Tooltip("좌표 오프셋 (미니맵 시작 위치 조정)")]
    public Vector2 offset = Vector2.zero;
    
    private static MiniMapController instance;
    
    public static MiniMapController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MiniMapController>();

                // 씬에 없으면 자동으로 생성
                if (instance == null)
                {
                    GameObject obj = new GameObject("MiniMapController");
                    instance = obj.AddComponent<MiniMapController>();
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Start에서도 인스턴스 확인 (다른 스크립트가 Awake에서 찾을 수 있도록)
        if (instance == null)
        {
            instance = this;
        }
        
        // 디버그: 연결 상태 확인
        if (playerIcon == null)
        {
            Debug.LogWarning("[MiniMapController] Player Icon이 할당되지 않았습니다! Inspector에서 Playercursor를 할당해주세요.");
        }
        else
        {
            Debug.Log($"[MiniMapController] Player Icon 연결됨: {playerIcon.name}");
        }
    }
    
    /// <summary>
    /// MiniMapController를 찾거나 생성 (안전한 방법)
    /// </summary>
    public static MiniMapController GetOrCreate()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<MiniMapController>();

            if (instance == null)
            {
                GameObject obj = new GameObject("MiniMapController");
                instance = obj.AddComponent<MiniMapController>();
                Debug.Log("MiniMapController가 자동으로 생성되었습니다.");
            }
        }
        return instance;
    }
    
    /// <summary>
    /// 플레이어 위치 업데이트
    /// </summary>
    public void UpdatePlayerPosition(Vector2Int position)
    {
        if (playerIcon == null)
        {
            Debug.LogWarning("[MiniMapController] Player Icon이 할당되지 않았습니다! Inspector에서 Playercursor를 할당해주세요.");
            return;
        }
        
        // 던전 좌표를 UI 좌표로 변환
        Vector2 uiPosition = new Vector2(
            position.x * cellSize + offset.x,
            position.y * cellSize + offset.y
        );
        
        // 플레이어 아이콘 위치 업데이트
        playerIcon.anchoredPosition = uiPosition;
        
        // 강제로 UI 업데이트
        Canvas.ForceUpdateCanvases();
        
        // 디버그 정보 출력
        Debug.Log($"[MiniMap] 위치 업데이트: 던전({position.x}, {position.y}) → UI({uiPosition.x:F2}, {uiPosition.y:F2}) | CellSize: {cellSize}");
        Debug.Log($"[MiniMap] PlayerIcon 업데이트 후 위치: {playerIcon.anchoredPosition}");
        
        // Playercursor의 부모 정보도 출력
        if (playerIcon.parent != null)
        {
            Debug.Log($"[MiniMap] Playercursor 부모: {playerIcon.parent.name}, 부모 위치: {playerIcon.parent.GetComponent<RectTransform>()?.anchoredPosition}");
        }
    }
    
    /// <summary>
    /// 플레이어 위치 업데이트 (오버로드)
    /// </summary>
    public void UpdatePlayerPosition(int x, int y)
    {
        UpdatePlayerPosition(new Vector2Int(x, y));
    }
    
    /// <summary>
    /// 플레이어 위치 업데이트 (Vector3 버전)
    /// </summary>
    public void UpdatePlayerPosition(Vector3 position)
    {
        UpdatePlayerPosition(new Vector2Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y)
        ));
    }
    
    /// <summary>
    /// 플레이어 방향 업데이트 (각도)
    /// </summary>
    public void UpdatePlayerRotation(float angle)
    {
        if (playerIcon == null)
        {
            Debug.LogWarning("[MiniMap] Player Icon이 할당되지 않았습니다!");
            return;
        }
        
        // Unity UI는 Z축 기준 회전 (2D)
        // 0도 = 위쪽(북쪽), 시계 반대 방향이 양수
        playerIcon.localEulerAngles = new Vector3(0, 0, angle);
        
        // 강제로 UI 업데이트
        Canvas.ForceUpdateCanvases();
        
        Debug.Log($"[MiniMap] 플레이어 방향 업데이트: {angle:F2}도 | 실제 회전값: {playerIcon.localEulerAngles.z:F2}도");
    }
    
    /// <summary>
    /// 플레이어 방향 업데이트 (방향 벡터)
    /// </summary>
    public void UpdatePlayerRotation(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        
        // Vector2Int로 변환하여 정확한 방향 판별
        Vector2Int dirInt = new Vector2Int(
            direction.x > 0.5f ? 1 : (direction.x < -0.5f ? -1 : 0),
            direction.y > 0.5f ? 1 : (direction.y < -0.5f ? -1 : 0)
        );
        
        // 직접 각도 매핑 (더 정확함)
        // Unity UI: 0도 = 위쪽(북쪽), 90도 = 오른쪽(동쪽), 180도 = 아래쪽(남쪽), 270도 = 왼쪽(서쪽)
        float angle = 0f;
        
        if (dirInt == Vector2Int.up)        // (0, 1) = 북쪽
            angle = 0f;
        else if (dirInt == Vector2Int.right) // (1, 0) = 동쪽
            angle = 90f;
        else if (dirInt == Vector2Int.down)  // (0, -1) = 남쪽
            angle = 180f;
        else if (dirInt == Vector2Int.left)  // (-1, 0) = 서쪽
            angle = 270f;
        else
        {
            // 대각선 방향의 경우 Atan2 사용
            direction.Normalize();
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            if (angle < 0) angle += 360f;
        }
        
        Debug.Log($"[MiniMap] 방향 벡터: ({dirInt.x}, {dirInt.y}) → 각도: {angle:F2}도");
        
        UpdatePlayerRotation(angle);
    }
    
    /// <summary>
    /// 플레이어 방향 업데이트 (방향 enum: 0=북, 1=동, 2=남, 3=서)
    /// </summary>
    public void UpdatePlayerRotation(int direction)
    {
        // 0=북(0도), 1=동(270도), 2=남(180도), 3=서(90도)
        float[] angles = { 0f, 270f, 180f, 90f };
        if (direction >= 0 && direction < angles.Length)
        {
            UpdatePlayerRotation(angles[direction]);
        }
    }
    
    /// <summary>
    /// 플레이어 이동 이벤트 (위치만)
    /// </summary>
    public void OnPlayerMove(Vector2Int position)
    {
        Debug.Log($"[MiniMapController] OnPlayerMove 호출됨: {position}");
        UpdatePlayerPosition(position);
    }
    
    /// <summary>
    /// 플레이어 이동 이벤트 (위치 + 방향)
    /// </summary>
    public void OnPlayerMove(Vector2Int position, float rotation)
    {
        UpdatePlayerPosition(position);
        UpdatePlayerRotation(rotation);
    }
    
    /// <summary>
    /// 플레이어 이동 이벤트 (위치 + 방향 벡터)
    /// </summary>
    public void OnPlayerMove(Vector2Int position, Vector2 direction)
    {
        UpdatePlayerPosition(position);
        UpdatePlayerRotation(direction);
    }
    
    /// <summary>
    /// 플레이어 이동 이벤트 (오버로드)
    /// </summary>
    public void OnPlayerMove(int x, int y)
    {
        OnPlayerMove(new Vector2Int(x, y));
    }
    
    /// <summary>
    /// 플레이어 이동 이벤트 (오버로드 - 위치 + 방향)
    /// </summary>
    public void OnPlayerMove(int x, int y, float rotation)
    {
        OnPlayerMove(new Vector2Int(x, y), rotation);
    }
    
    /// <summary>
    /// 플레이어 이동 이벤트 (Vector3 버전)
    /// </summary>
    public void OnPlayerMove(Vector3 position)
    {
        OnPlayerMove(new Vector2Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y)
        ));
    }
}

