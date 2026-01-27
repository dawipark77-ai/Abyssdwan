using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 던전 맵 생성기
/// 20x20 크기의 미로 던전을 생성
/// </summary>
public class DungeonMapGenerator : MonoBehaviour
{
    [Header("맵 설정")]
    public int mapWidth = 20;
    public int mapHeight = 20;
    
    private DungeonMapTile[,] map;
    private Vector2Int startPosition;
    private Vector2Int exitPosition;
    
    /// <summary>
    /// 던전 맵 생성
    /// </summary>
    public DungeonMapTile[,] GenerateMap()
    {
        map = new DungeonMapTile[mapWidth, mapHeight];
        
        // 모든 타일을 벽으로 초기화
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[x, y] = new DungeonMapTile(new Vector2Int(x, y), DungeonMapTile.TileType.Wall);
            }
        }
        
        // 미로 생성 (Random Walk 알고리즘)
        GenerateMaze();
        
        // 시작점과 탈출점 설정
        SetStartAndExit();
        
        return map;
    }
    
    /// <summary>
    /// 미로 생성 (Random Walk)
    /// </summary>
    private void GenerateMaze()
    {
        // 시작 위치 (홀수 좌표로 설정하여 경계와 겹치지 않게)
        int startX = 1;
        int startY = 1;
        
        Vector2Int current = new Vector2Int(startX, startY);
        map[current.x, current.y].tileType = DungeonMapTile.TileType.Path;
        
        // 방문한 위치 추적
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        visited.Add(current);
        
        // 경로 스택 (백트래킹용)
        Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
        pathStack.Push(current);
        
        // 이동 방향 (상하좌우)
        Vector2Int[] directions = {
            new Vector2Int(0, 2),  // 위
            new Vector2Int(0, -2), // 아래
            new Vector2Int(2, 0),  // 오른쪽
            new Vector2Int(-2, 0)  // 왼쪽
        };
        
        int maxIterations = (mapWidth * mapHeight) / 2;
        int iterations = 0;
        
        while (pathStack.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            
            // 현재 위치에서 이동 가능한 방향 찾기
            List<Vector2Int> availableDirections = new List<Vector2Int>();
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                Vector2Int wall = current + dir / 2;
                
                // 경계 체크 및 미방문 체크
                if (IsValidPosition(next) && !visited.Contains(next))
                {
                    availableDirections.Add(dir);
                }
            }
            
            if (availableDirections.Count > 0)
            {
                // 랜덤한 방향 선택
                Vector2Int chosenDir = availableDirections[Random.Range(0, availableDirections.Count)];
                Vector2Int next = current + chosenDir;
                Vector2Int wall = current + chosenDir / 2;
                
                // 벽을 길로 만들고 다음 위치도 길로
                map[wall.x, wall.y].tileType = DungeonMapTile.TileType.Path;
                map[next.x, next.y].tileType = DungeonMapTile.TileType.Path;
                
                visited.Add(next);
                pathStack.Push(next);
                current = next;
            }
            else
            {
                // 더 이상 갈 곳이 없으면 백트래킹
                if (pathStack.Count > 0)
                {
                    current = pathStack.Pop();
                }
            }
        }
        
        // 추가 경로 생성 (연결성 향상)
        AddExtraPaths(visited);
    }
    
    /// <summary>
    /// 추가 경로 생성 (던전을 더 복잡하게)
    /// </summary>
    private void AddExtraPaths(HashSet<Vector2Int> visited)
    {
        int extraPaths = (mapWidth * mapHeight) / 20; // 약 5% 추가 경로
        
        for (int i = 0; i < extraPaths; i++)
        {
            int x = Random.Range(1, mapWidth - 1);
            int y = Random.Range(1, mapHeight - 1);
            
            if (map[x, y].tileType == DungeonMapTile.TileType.Wall)
            {
                // 주변에 길이 있으면 이 위치도 길로 만들기
                int pathCount = 0;
                if (x > 0 && map[x - 1, y].tileType == DungeonMapTile.TileType.Path) pathCount++;
                if (x < mapWidth - 1 && map[x + 1, y].tileType == DungeonMapTile.TileType.Path) pathCount++;
                if (y > 0 && map[x, y - 1].tileType == DungeonMapTile.TileType.Path) pathCount++;
                if (y < mapHeight - 1 && map[x, y + 1].tileType == DungeonMapTile.TileType.Path) pathCount++;
                
                if (pathCount >= 2)
                {
                    map[x, y].tileType = DungeonMapTile.TileType.Path;
                }
            }
        }
    }
    
    /// <summary>
    /// 시작점과 탈출점 설정
    /// </summary>
    private void SetStartAndExit()
    {
        // 시작점 찾기 (가장 왼쪽 위쪽 경로)
        for (int x = 1; x < mapWidth - 1; x++)
        {
            for (int y = mapHeight - 2; y > 0; y--)
            {
                if (map[x, y].tileType == DungeonMapTile.TileType.Path)
                {
                    startPosition = new Vector2Int(x, y);
                    map[x, y].tileType = DungeonMapTile.TileType.Start;
                    map[x, y].isExplored = true; // 시작점은 이미 탐험한 것으로
                    goto foundStart;
                }
            }
        }
        
        foundStart:
        
        // 탈출점 찾기 (가장 오른쪽 아래쪽 경로, 시작점과 최대한 멀리)
        int maxDistance = 0;
        Vector2Int bestExit = startPosition;
        
        for (int x = mapWidth - 2; x > 0; x--)
        {
            for (int y = 1; y < mapHeight - 1; y++)
            {
                if (map[x, y].tileType == DungeonMapTile.TileType.Path)
                {
                    int distance = Mathf.Abs(x - startPosition.x) + Mathf.Abs(y - startPosition.y);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        bestExit = new Vector2Int(x, y);
                    }
                }
            }
        }
        
        exitPosition = bestExit;
        map[exitPosition.x, exitPosition.y].tileType = DungeonMapTile.TileType.Exit;
    }
    
    /// <summary>
    /// 유효한 위치인지 확인
    /// </summary>
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x > 0 && pos.x < mapWidth - 1 && 
               pos.y > 0 && pos.y < mapHeight - 1;
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
}


