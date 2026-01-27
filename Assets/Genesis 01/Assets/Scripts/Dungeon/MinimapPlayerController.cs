using UnityEngine;

/// <summary>
/// 미니맵 기반 플레이어 컨트롤러
/// 미니맵에서 클릭하거나 방향키로 이동
/// </summary>
public class MinimapPlayerController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private DungeonMinimap dungeonMinimap;
    
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useSmoothMovement = true;
    
    [Header("입력 설정")]
    [SerializeField] private KeyCode moveUpKey = KeyCode.W;
    [SerializeField] private KeyCode moveDownKey = KeyCode.S;
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;
    
    private Vector2Int targetPosition;
    private Vector2Int currentPosition;
    private Vector2Int lastMoveDirection = Vector2Int.up; // 마지막 이동 방향 (기본값: 북쪽)
    private bool isMoving = false;
    private float moveTimer = 0f;
    
    private void Start()
    {
        if (dungeonMinimap == null)
        {
            dungeonMinimap = FindObjectOfType<DungeonMinimap>();
        }
        
        if (dungeonMinimap != null)
        {
            currentPosition = dungeonMinimap.GetPlayerPosition();
            targetPosition = currentPosition;
        }
    }
    
    private void Update()
    {
        HandleInput();
        
        if (isMoving && useSmoothMovement)
        {
            UpdateMovement();
        }
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
        
        // 마우스 클릭으로 이동 (미니맵 클릭)
        if (Input.GetMouseButtonDown(0))
        {
            HandleMinimapClick();
        }
    }
    
    /// <summary>
    /// 이동 시도
    /// </summary>
    private void TryMove(Vector2Int direction)
    {
        if (isMoving) return;
        
        Vector2Int newPosition = currentPosition + direction;
        
        if (dungeonMinimap != null && dungeonMinimap.CanMoveTo(newPosition))
        {
            // 이동 방향 저장
            lastMoveDirection = direction;
            
            // 미니맵 컨트롤러에 방향 업데이트
            if (MiniMapController.Instance != null)
            {
                // Vector2Int를 Vector2로 변환하여 전달
                Vector2 directionVector = new Vector2(direction.x, direction.y);
                MiniMapController.Instance.UpdatePlayerRotation(directionVector);
            }
            
            targetPosition = newPosition;
            
            if (useSmoothMovement)
            {
                isMoving = true;
                moveTimer = 0f;
            }
            else
            {
                // 즉시 이동
                currentPosition = newPosition;
                dungeonMinimap.MovePlayer(newPosition);
            }
        }
    }
    
    /// <summary>
    /// 부드러운 이동 업데이트
    /// </summary>
    private void UpdateMovement()
    {
        moveTimer += Time.deltaTime * moveSpeed;
        
        if (moveTimer >= 1f)
        {
            // 이동 완료
            moveTimer = 1f;
            isMoving = false;
            currentPosition = targetPosition;
            dungeonMinimap.MovePlayer(targetPosition);
            
            // 미니맵 컨트롤러에 방향 업데이트 (이동 완료 시)
            if (MiniMapController.Instance != null)
            {
                Vector2 directionVector = new Vector2(lastMoveDirection.x, lastMoveDirection.y);
                MiniMapController.Instance.UpdatePlayerRotation(directionVector);
            }
        }
    }
    
    /// <summary>
    /// 미니맵 클릭 처리
    /// </summary>
    private void HandleMinimapClick()
    {
        // Raycast를 사용하여 미니맵에서 클릭한 위치 확인
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        if (hit.collider != null)
        {
            // 클릭한 위치를 던전 좌표로 변환
            Vector3 worldPos = hit.point;
            Vector2Int targetPos = new Vector2Int(
                Mathf.RoundToInt(worldPos.x),
                Mathf.RoundToInt(worldPos.y)
            );
            
            if (dungeonMinimap != null && dungeonMinimap.CanMoveTo(targetPos))
            {
                // 경로 찾기 (간단한 버전 - 직접 이동)
                MoveToPosition(targetPos);
            }
        }
    }
    
    /// <summary>
    /// 특정 위치로 이동
    /// </summary>
    private void MoveToPosition(Vector2Int targetPos)
    {
        if (isMoving) return;
        
        targetPosition = targetPos;
        
        if (useSmoothMovement)
        {
            isMoving = true;
            moveTimer = 0f;
        }
        else
        {
            currentPosition = targetPosition;
            dungeonMinimap.MovePlayer(targetPosition);
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





