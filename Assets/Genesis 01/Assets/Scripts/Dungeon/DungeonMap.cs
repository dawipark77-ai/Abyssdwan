using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 맵 관리자
/// 플레이어 위치 추적, 탐험 상태 관리, 클리어 체크
/// </summary>
public class DungeonMap : MonoBehaviour
{
    [Header("맵 생성기")]
    public DungeonMapGenerator mapGenerator;
    
    [Header("맵 설정")]
    public int mapWidth = 20;
    public int mapHeight = 20;
    
    private DungeonMapTile[,] map;
    private Vector2Int currentPlayerPosition;
    private Vector2Int startPosition;
    private Vector2Int exitPosition;
    private bool isCleared = false;
    
    // 이벤트
    public System.Action OnMapCleared;
    public System.Action<Vector2Int> OnPlayerPositionChanged;
    
    private void Start()
    {
        if (mapGenerator == null)
        {
            mapGenerator = GetComponent<DungeonMapGenerator>();
            if (mapGenerator == null)
            {
                mapGenerator = gameObject.AddComponent<DungeonMapGenerator>();
            }
        }
        
        InitializeMap();
    }
    
    /// <summary>
    /// 맵 초기화
    /// </summary>
    public void InitializeMap()
    {
        map = mapGenerator.GenerateMap();
        startPosition = mapGenerator.GetStartPosition();
        exitPosition = mapGenerator.GetExitPosition();
        currentPlayerPosition = startPosition;
        
        // 시작점 주변 탐험
        ExploreArea(currentPlayerPosition);
        
        Debug.Log($"[DungeonMap] 맵 생성 완료. 시작: {startPosition}, 탈출: {exitPosition}");
    }
    
    /// <summary>
    /// 플레이어 위치 업데이트
    /// </summary>
    public void SetPlayerPosition(Vector2Int position)
    {
        if (!IsValidPosition(position)) return;
        if (!IsWalkable(position)) return;
        
        currentPlayerPosition = position;
        
        // 주변 영역 탐험
        ExploreArea(position);
        
        // 탈출점 도착 체크
        if (position == exitPosition && !isCleared)
        {
            isCleared = true;
            OnMapCleared?.Invoke();
            Debug.Log("[DungeonMap] 던전 클리어!");
        }
        
        OnPlayerPositionChanged?.Invoke(position);
    }
    
    /// <summary>
    /// 영역 탐험 처리 (주변 8방향)
    /// </summary>
    private void ExploreArea(Vector2Int center)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int pos = center + new Vector2Int(dx, dy);
                if (IsValidPosition(pos))
                {
                    map[pos.x, pos.y].isExplored = true;
                }
            }
        }
    }
    
    /// <summary>
    /// 타일 정보 가져오기
    /// </summary>
    public DungeonMapTile GetTile(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            return map[position.x, position.y];
        }
        return null;
    }
    
    /// <summary>
    /// 타일 정보 가져오기 (x, y 좌표로)
    /// </summary>
    public DungeonMapTile GetTile(int x, int y)
    {
        return GetTile(new Vector2Int(x, y));
    }
    
    /// <summary>
    /// 타일 정보 가져오기 (x, y 좌표로, 3번째 인자 무시)
    /// </summary>
    public DungeonMapTile GetTile(int x, int y, int unused)
    {
        return GetTile(new Vector2Int(x, y));
    }
    
    /// <summary>
    /// 타일 설정
    /// </summary>
    public void SetTile(Vector2Int position, DungeonMapTile.TileType tileType, bool explored = false)
    {
        if (IsValidPosition(position))
        {
            map[position.x, position.y].tileType = tileType;
            map[position.x, position.y].isExplored = explored;
        }
    }
    
    /// <summary>
    /// 타일 설정 (x, y 좌표로)
    /// </summary>
    public void SetTile(int x, int y, DungeonMapTile.TileType tileType, bool explored = false)
    {
        SetTile(new Vector2Int(x, y), tileType, explored);
    }
    
    /// <summary>
    /// 타일 설정 (x, y 좌표로, int 타입으로)
    /// </summary>
    public void SetTile(int x, int y, int tileTypeInt, bool explored = false)
    {
        // int를 enum으로 변환 (명시적 캐스팅)
        DungeonMapTile.TileType tileType = (DungeonMapTile.TileType)tileTypeInt;
        SetTile(new Vector2Int(x, y), tileType, explored);
    }
    
    /// <summary>
    /// 타일 설정 (x, y 좌표로, int 타입으로, explored 없이)
    /// </summary>
    public void SetTile(int x, int y, int tileTypeInt)
    {
        SetTile(x, y, tileTypeInt, false);
    }
    
    /// <summary>
    /// 타일 설정 (x, y 좌표로, int 타입으로, DungeonMapTile 객체로)
    /// </summary>
    public void SetTile(int x, int y, int tileTypeInt, DungeonMapTile tile)
    {
        if (IsValidPosition(new Vector2Int(x, y)))
        {
            map[x, y] = tile;
        }
    }
    
    /// <summary>
    /// 타일 설정 (DungeonMapTile 객체로)
    /// </summary>
    public void SetTile(Vector2Int position, DungeonMapTile tile)
    {
        if (IsValidPosition(position))
        {
            map[position.x, position.y] = tile;
        }
    }
    
    /// <summary>
    /// 타일이 탐험되었는지 확인
    /// </summary>
    public bool IsExplored(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            return map[position.x, position.y].isExplored;
        }
        return false;
    }
    
    /// <summary>
    /// 유효한 위치인지 확인
    /// </summary>
    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < mapWidth &&
               position.y >= 0 && position.y < mapHeight;
    }
    
    /// <summary>
    /// 이동 가능한 위치인지 확인
    /// </summary>
    public bool IsWalkable(Vector2Int position)
    {
        if (!IsValidPosition(position)) return false;
        return map[position.x, position.y].IsWalkable();
    }
    
    /// <summary>
    /// 현재 플레이어 위치 반환
    /// </summary>
    public Vector2Int GetPlayerPosition()
    {
        return currentPlayerPosition;
    }
    
    /// <summary>
    /// 시작 위치 반환
    /// </summary>
    public Vector2Int GetStartPosition()
    {
        return startPosition;
    }
    
    /// <summary>
    /// 탈출점 위치 반환
    /// </summary>
    public Vector2Int GetExitPosition()
    {
        return exitPosition;
    }
    
    /// <summary>
    /// 맵 데이터 반환
    /// </summary>
    public DungeonMapTile[,] GetMap()
    {
        return map;
    }
    
    /// <summary>
    /// 클리어 여부 반환
    /// </summary>
    public bool IsCleared()
    {
        return isCleared;
    }
}

