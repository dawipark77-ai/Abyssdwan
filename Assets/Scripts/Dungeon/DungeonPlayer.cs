using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonPlayer : MonoBehaviour
{
    public Tilemap floorTilemap;

    // 시작 위치 (맵 배열 기준)
    public Vector2Int gridPos = new Vector2Int(2, 2);

    void Start()
    {
        UpdateWorldPosition();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) Move(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) Move(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) Move(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) Move(Vector2Int.right);
    }

    void Move(Vector2Int dir)
    {
        Vector2Int next = gridPos + dir;

        Vector3Int cellPos = new Vector3Int(next.x, -next.y, 0);

        // 바닥 타일이 있는 곳만 이동 가능
        if (floorTilemap.HasTile(cellPos))
        {
            gridPos = next;
            UpdateWorldPosition();
        }
    }

    void UpdateWorldPosition()
    {
        Vector3Int cellPos = new Vector3Int(gridPos.x, -gridPos.y, 0);
        Vector3 worldPos = floorTilemap.GetCellCenterWorld(cellPos);
        transform.position = worldPos;
    }
}
