using UnityEngine;

/// <summary>
/// 던전 맵 플레이어 컨트롤러
/// WASD로 이동, 던전 맵과 연동
/// </summary>
public class DungeonMapPlayerController : MonoBehaviour
{
    [Header("참조")]
    public DungeonMap dungeonMap;
    
    [Header("입력 설정")]
    public KeyCode moveUpKey = KeyCode.W;
    public KeyCode moveDownKey = KeyCode.S;
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;
    
    private Vector2Int currentPosition;
    
    private void Start()
    {
        if (dungeonMap == null)
        {
            dungeonMap = FindObjectOfType<DungeonMap>();
        }
        
        if (dungeonMap != null)
        {
            currentPosition = dungeonMap.GetStartPosition();
            dungeonMap.SetPlayerPosition(currentPosition);
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    /// <summary>
    /// 입력 처리
    /// </summary>
    private void HandleInput()
    {
        Vector2Int moveDirection = Vector2Int.zero;
        
        if (Input.GetKeyDown(moveUpKey))
        {
            moveDirection = Vector2Int.up;
        }
        else if (Input.GetKeyDown(moveDownKey))
        {
            moveDirection = Vector2Int.down;
        }
        else if (Input.GetKeyDown(moveLeftKey))
        {
            moveDirection = Vector2Int.left;
        }
        else if (Input.GetKeyDown(moveRightKey))
        {
            moveDirection = Vector2Int.right;
        }
        
        if (moveDirection != Vector2Int.zero)
        {
            TryMove(moveDirection);
        }
    }
    
    /// <summary>
    /// 이동 시도
    /// </summary>
    private void TryMove(Vector2Int direction)
    {
        if (dungeonMap == null) return;
        
        Vector2Int newPosition = currentPosition + direction;
        
        if (dungeonMap.IsWalkable(newPosition))
        {
            currentPosition = newPosition;
            dungeonMap.SetPlayerPosition(newPosition);
            
            // 클리어 체크는 DungeonMap에서 자동으로 처리됨
        }
    }
    
    /// <summary>
    /// 현재 위치 반환
    /// </summary>
    public Vector2Int GetCurrentPosition()
    {
        return currentPosition;
    }
}


