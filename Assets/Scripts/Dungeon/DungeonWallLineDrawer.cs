using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace Genesis01.Dungeon
{
    public class DungeonWallLineDrawer : MonoBehaviour
    {
        [Header("References")]
        public DungeonMap dungeonMap;
        public Tilemap wallTilemap;   // 타일맵 기반 벽(Fallback)
        public Tilemap floorTilemap;  // 바닥 타일 기준 정렬
        public Tilemap fogTilemap;    // 안개 타일맵 - 탐험 여부 확인용
        
        [Header("Wall Line Settings")]
        public float lineWidth = 0.12f;    // Thicker for guaranteed visibility
        public Color lineColor = Color.white;
        public float lineHeight = -0.5f;  // Bring closer to camera (Floor is usually at 0)
        public float cellSize = 1f;       // 셀 크기
        
        private Material wallMaterial;     // Shared material
        
        // 벽선을 셀별로 관리 (셀 좌표 -> 벽선 오브젝트 리스트)
        private Dictionary<Vector2Int, List<GameObject>> wallLinesByCell = new Dictionary<Vector2Int, List<GameObject>>();
    
    void Start()
    {
        if (dungeonMap == null) dungeonMap = FindFirstObjectByType<Genesis01.Dungeon.DungeonMap>();
        if (floorTilemap == null)
        {
            floorTilemap = FindFirstObjectByType<MapManager>()?.floorTilemap;
        }
        if (wallTilemap == null)
        {
            wallTilemap = FindFirstObjectByType<MapManager>()?.wallTilemap;
            if (wallTilemap == null)
            {
                // Grid/Wall 이름으로 탐색
                var gridObj = GameObject.Find("Grid");
                if (gridObj != null)
                {
                    var wallTf = gridObj.transform.Find("Wall");
                    if (wallTf != null) wallTilemap = wallTf.GetComponent<Tilemap>();
                    var floorTf = gridObj.transform.Find("Floor");
                    if (floorTf != null) floorTilemap = floorTf.GetComponent<Tilemap>();
                }
            }
        }
        
        // MapManager가 먼저 실행되도록 약간 지연
        Invoke(nameof(DrawWalls), 0.2f);
    }

    /// <summary>
    /// 모든 벽선 GameObject와 캐시를 초기화
    /// </summary>
    public void ResetLines()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        wallLinesByCell.Clear();
    }
    
    public void DrawWalls()
    {
        bool usedTilemapFallback = false;

        // 기존 벽선 모두 삭제
        ResetLines();
        
        // 1) DungeonMap 기반(비트마스크) 우선
        if (dungeonMap != null)
        {
            int width = dungeonMap.Width;
            int height = dungeonMap.Height;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = dungeonMap.GetTile(0, x, y);
                    Vector2Int gridPos = new Vector2Int(x, y);
                    
                    // 월드 좌표 계산 (DungeonGridPlayer와 일치시키기 위해 y를 -y로 처리)
                    Vector3 cellWorldPos = new Vector3(x * cellSize, -y * cellSize, 0);

                    // 4방향 체크
                    if (tile.HasWall(Genesis01.Dungeon.WallFlags.North)) 
                        CreateWallLine(cellWorldPos, gridPos, new Vector3Int(0, -1, 0), 0f);
                    if (tile.HasWall(Genesis01.Dungeon.WallFlags.East)) 
                        CreateWallLine(cellWorldPos, gridPos, Vector3Int.right, 90f);
                    if (tile.HasWall(Genesis01.Dungeon.WallFlags.South)) 
                        CreateWallLine(cellWorldPos, gridPos, new Vector3Int(0, 1, 0), 180f);
                    if (tile.HasWall(Genesis01.Dungeon.WallFlags.West)) 
                        CreateWallLine(cellWorldPos, gridPos, Vector3Int.left, 270f);
                }
            }
        }
        // 2) DungeonMap이 없으면 Tilemap 기반으로 벽선 생성
        //    - 벽타일이 아니라 "바닥 타일"을 기준으로 경계선을 감지해 라인을 맞춤
        else if (floorTilemap != null)
        {
            usedTilemapFallback = true;

            Vector3 gridCellSize = floorTilemap.layoutGrid.cellSize;
            float csX = Mathf.Approximately(gridCellSize.x, 0f) ? 1f : gridCellSize.x;
            float csY = Mathf.Approximately(gridCellSize.y, 0f) ? 1f : gridCellSize.y;
            cellSize = Mathf.Max(csX, csY); // 대략적인 셀 크기

            BoundsInt bounds = floorTilemap.cellBounds;
            foreach (Vector3Int cellPos in bounds.allPositionsWithin)
            {
                if (!floorTilemap.HasTile(cellPos)) continue;

                // 타일맵 좌표 -> 그리드 좌표 (y 반전)
                Vector2Int gridPos = new Vector2Int(cellPos.x, -cellPos.y);
                Vector3 cellWorldPos = floorTilemap.GetCellCenterWorld(cellPos);

                // 네 방향 이웃 체크: 바닥이 없는 곳에 경계선 생성
                if (!floorTilemap.HasTile(cellPos + new Vector3Int(0, 1, 0))) // 위쪽(타일맵 y+1) == Grid South, 하지만 월드 선은 북쪽 경계
                    CreateWallLine(cellWorldPos, gridPos, new Vector3Int(0, -1, 0), 0f);
                if (!floorTilemap.HasTile(cellPos + new Vector3Int(1, 0, 0))) // 오른쪽
                    CreateWallLine(cellWorldPos, gridPos, Vector3Int.right, 90f);
                if (!floorTilemap.HasTile(cellPos + new Vector3Int(0, -1, 0))) // 아래쪽
                    CreateWallLine(cellWorldPos, gridPos, new Vector3Int(0, 1, 0), 180f);
                if (!floorTilemap.HasTile(cellPos + new Vector3Int(-1, 0, 0))) // 왼쪽
                    CreateWallLine(cellWorldPos, gridPos, Vector3Int.left, 270f);
            }
        }
        else
        {
            Debug.LogError("[Drawer] DungeonMap도 없고 Wall Tilemap도 없어 벽선을 그릴 수 없습니다.");
            return;
        }
        
        // 초기 벽선 가시성 업데이트 (안개 체크)
        UpdateWallVisibility();
        
        if (usedTilemapFallback)
            Debug.Log($"[Drawer] 벽선 생성 완료: {transform.childCount}개 (Tilemap fallback)");
        else
            Debug.Log($"[Drawer] 벽선 생성 완료: {transform.childCount}개 (비트마스크 기반)");
    }
    
    
    void CreateWallLine(Vector3 cellWorldPos, Vector2Int gridPos, Vector3Int direction, float rotationY)
    {
        // 셀 경계에 벽선 배치
        Vector3 wallPos = cellWorldPos;
        
        // 방향에 따라 셀 경계로 이동 (셀 크기 1x1 기준, 중심에서 ±0.5)
        // 방향에 따라 셀 경계로 이동 (셀 크기 1x1 기준, 중심에서 ±0.5)
        // (x, -y) 좌표계 기준: North(y--)는 +0.5, South(y++)는 -0.5
        if (direction.y == -1)       // 북 (Grid Y--) -> World Y++
            wallPos.y += 0.5f * cellSize;
        else if (direction.y == 1)   // 남 (Grid Y++) -> World Y--
            wallPos.y -= 0.5f * cellSize;
        else if (direction.x == 1)  // 동 (Grid X++) -> World X++
            wallPos.x += 0.5f * cellSize;
        else if (direction.x == -1) // 서 (Grid X--) -> World X--
            wallPos.x -= 0.5f * cellSize;
        
        // z축 높이 설정 (바닥 위에 표시)
        wallPos.z = lineHeight;
        
        // 벽선의 시작점과 끝점 계산
        Vector3 start, end;
        
        if (direction.y != 0)  // 북/남 (수평선)
        {
            start = wallPos + new Vector3(-0.5f * cellSize, 0, 0);
            end = wallPos + new Vector3(0.5f * cellSize, 0, 0);
        }
        else  // 동/서 (수직선)
        {
            start = wallPos + new Vector3(0, -0.5f * cellSize, 0);
            end = wallPos + new Vector3(0, 0.5f * cellSize, 0);
        }
        
        // LineRenderer로 벽선 생성
        GameObject lineObj = new GameObject("WallLine");
        lineObj.transform.parent = transform;
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        
        // Material 설정 (공유 머티리얼 사용 - UI/Default는 텍스처 없이도 잘 보임)
        if (wallMaterial == null)
        {
            wallMaterial = new Material(Shader.Find("UI/Default")); // More robust for simple lines
            wallMaterial.color = lineColor;
        }
        lr.material = wallMaterial;
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        // 레이어 설정 (가장 위에 그려지도록)
        lr.sortingLayerName = "Default"; // Ensure it's on a known layer
        lr.sortingOrder = 100;
        
        // 셀별로 벽선 저장 (안개 시스템 연동용)
        if (!wallLinesByCell.ContainsKey(gridPos))
        {
            wallLinesByCell[gridPos] = new List<GameObject>();
        }
        wallLinesByCell[gridPos].Add(lineObj);
        
        // 초기에는 표시 (안개가 있으면 UpdateWallVisibility에서 가려짐)
        lineObj.SetActive(true);
    }
    
    // 안개 상태에 따라 벽선 가시성 업데이트
    [ContextMenu("Update Visibility Now")]
    public void UpdateWallVisibility()
    {
        // null 체크
        if (wallLinesByCell == null) return;
        
        if (fogTilemap == null)
        {
            // 안개 타일맵이 없으면 모든 벽선 표시
            foreach (var kvp in wallLinesByCell)
            {
                if (kvp.Value == null) continue;
                foreach (var line in kvp.Value)
                {
                    if (line != null)
                    {
                        line.SetActive(true);
                    }
                }
            }
            return;
        }
        
        // 각 셀의 안개 상태 확인
        foreach (var kvp in wallLinesByCell)
        {
            if (kvp.Value == null) continue;
            
            Vector2Int gridPos = kvp.Key;
            // 타일맵 좌표로 변환 (x, -y, 0)
            Vector3Int tilePos = new Vector3Int(gridPos.x, -gridPos.y, 0);
            
            // 안개가 없으면 (탐험한 영역) 벽선 표시
            bool isRevealed = !fogTilemap.HasTile(tilePos);

            foreach (var line in kvp.Value)
            {
                if (line != null)
                    line.SetActive(isRevealed);
            }
        }
    }
}
}
