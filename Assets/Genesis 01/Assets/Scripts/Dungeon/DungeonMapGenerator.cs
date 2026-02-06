using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ?섏쟾 留??앹꽦湲?
/// 20x20 ?ш린??誘몃줈 ?섏쟾???앹꽦
/// </summary>
public class DungeonMapGenerator : MonoBehaviour
{
    [Header("留??ㅼ젙")]
    public int mapWidth = 20;
    public int mapHeight = 20;
    
    private DungeonMapTile[,] map;
    private Vector2Int startPosition;
    private Vector2Int exitPosition;
    
    /// <summary>
    /// ?섏쟾 留??앹꽦
    /// </summary>
    public DungeonMapTile[,] GenerateMap()
    {
        map = new DungeonMapTile[mapWidth, mapHeight];
        
        // 紐⑤뱺 ??쇱쓣 踰쎌쑝濡?珥덇린??
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[x, y] = new DungeonMapTile(new Vector2Int(x, y), DungeonMapTile.TileType.Wall);
            }
        }
        
        // 誘몃줈 ?앹꽦 (Random Walk ?뚭퀬由ъ쬁)
        GenerateMaze();
        
        // ?쒖옉?먭낵 ?덉텧???ㅼ젙
        SetStartAndExit();
        
        return map;
    }
    
    /// <summary>
    /// 誘몃줈 ?앹꽦 (Random Walk)
    /// </summary>
    private void GenerateMaze()
    {
        // ?쒖옉 ?꾩튂 (???醫뚰몴濡??ㅼ젙?섏뿬 寃쎄퀎? 寃뱀튂吏 ?딄쾶)
        int startX = 1;
        int startY = 1;
        
        Vector2Int current = new Vector2Int(startX, startY);
        map[current.x, current.y].tileType = DungeonMapTile.TileType.Path;
        
        // 諛⑸Ц???꾩튂 異붿쟻
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        visited.Add(current);
        
        // 寃쎈줈 ?ㅽ깮 (諛깊듃?섑궧??
        Stack<Vector2Int> pathStack = new Stack<Vector2Int>();
        pathStack.Push(current);
        
        // ?대룞 諛⑺뼢 (?곹븯醫뚯슦)
        Vector2Int[] directions = {
            new Vector2Int(0, 2),  // ??
            new Vector2Int(0, -2), // ?꾨옒
            new Vector2Int(2, 0),  // ?ㅻⅨ履?
            new Vector2Int(-2, 0)  // ?쇱そ
        };
        
        int maxIterations = (mapWidth * mapHeight) / 2;
        int iterations = 0;
        
        while (pathStack.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            
            // ?꾩옱 ?꾩튂?먯꽌 ?대룞 媛?ν븳 諛⑺뼢 李얘린
            List<Vector2Int> availableDirections = new List<Vector2Int>();
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                Vector2Int wall = current + dir / 2;
                
                // 寃쎄퀎 泥댄겕 諛?誘몃갑臾?泥댄겕
                if (IsValidPosition(next) && !visited.Contains(next))
                {
                    availableDirections.Add(dir);
                }
            }
            
            if (availableDirections.Count > 0)
            {
                // ?쒕뜡??諛⑺뼢 ?좏깮
                Vector2Int chosenDir = availableDirections[UnityEngine.Random.Range(0, availableDirections.Count)];
                Vector2Int next = current + chosenDir;
                Vector2Int wall = current + chosenDir / 2;
                
                // 踰쎌쓣 湲몃줈 留뚮뱾怨??ㅼ쓬 ?꾩튂??湲몃줈
                map[wall.x, wall.y].tileType = DungeonMapTile.TileType.Path;
                map[next.x, next.y].tileType = DungeonMapTile.TileType.Path;
                
                visited.Add(next);
                pathStack.Push(next);
                current = next;
            }
            else
            {
                // ???댁긽 媛?怨녹씠 ?놁쑝硫?諛깊듃?섑궧
                if (pathStack.Count > 0)
                {
                    current = pathStack.Pop();
                }
            }
        }
        
        // 異붽? 寃쎈줈 ?앹꽦 (?곌껐???μ긽)
        AddExtraPaths(visited);
    }
    
    /// <summary>
    /// 異붽? 寃쎈줈 ?앹꽦 (?섏쟾????蹂듭옟?섍쾶)
    /// </summary>
    private void AddExtraPaths(HashSet<Vector2Int> visited)
    {
        int extraPaths = (mapWidth * mapHeight) / 20; // ??5% 異붽? 寃쎈줈
        
        for (int i = 0; i < extraPaths; i++)
        {
            int x = UnityEngine.Random.Range(1, mapWidth - 1);
            int y = UnityEngine.Random.Range(1, mapHeight - 1);
            
            if (map[x, y].tileType == DungeonMapTile.TileType.Wall)
            {
                // 二쇰???湲몄씠 ?덉쑝硫????꾩튂??湲몃줈 留뚮뱾湲?
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
    /// ?쒖옉?먭낵 ?덉텧???ㅼ젙
    /// </summary>
    private void SetStartAndExit()
    {
        // ?쒖옉??李얘린 (媛???쇱そ ?꾩そ 寃쎈줈)
        for (int x = 1; x < mapWidth - 1; x++)
        {
            for (int y = mapHeight - 2; y > 0; y--)
            {
                if (map[x, y].tileType == DungeonMapTile.TileType.Path)
                {
                    startPosition = new Vector2Int(x, y);
                    map[x, y].tileType = DungeonMapTile.TileType.Start;
                    map[x, y].isExplored = true; // ?쒖옉?먯? ?대? ?먰뿕??寃껋쑝濡?
                    goto foundStart;
                }
            }
        }
        
        foundStart:
        
        // ?덉텧??李얘린 (媛???ㅻⅨ履??꾨옒履?寃쎈줈, ?쒖옉?먭낵 理쒕???硫由?
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
    /// ?좏슚???꾩튂?몄? ?뺤씤
    /// </summary>
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x > 0 && pos.x < mapWidth - 1 && 
               pos.y > 0 && pos.y < mapHeight - 1;
    }
    
    /// <summary>
    /// ?쒖옉 ?꾩튂 諛섑솚
    /// </summary>
    public Vector2Int GetStartPosition()
    {
        return startPosition;
    }
    
    /// <summary>
    /// ?덉텧???꾩튂 諛섑솚
    /// </summary>
    public Vector2Int GetExitPosition()
    {
        return exitPosition;
    }
}



