using UnityEngine;

public class PlayerGridMove : MonoBehaviour
{
    /*
     * 0 = 이동 가능 (바닥)
     * 1 = 이동 불가 (벽)
     * 
     * y가 위, x가 오른쪽입니다.
     */

    int[,] dungeonMap =
    {
        {1,1,1,1,1},
        {1,0,0,0,1},
        {1,0,1,0,1},
        {1,0,0,0,1},
        {1,1,1,1,1},
    };

    // 플레이어의 그리드 좌표
    Vector2Int playerPos = new Vector2Int(1, 1);

    void Start()
    {
        UpdateWorldPosition();
    }

    void Update()
    {
        Vector2Int dir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W))
            dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S))
            dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A))
            dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D))
            dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
            TryMove(dir);
    }

    void TryMove(Vector2Int dir)
    {
        Vector2Int next = playerPos + dir;

        // 맵 범위 체크
        if (next.x < 0 || next.y < 0 ||
            next.x >= dungeonMap.GetLength(0) ||
            next.y >= dungeonMap.GetLength(1))
            return;

        // 벽이면 이동 불가
        if (dungeonMap[next.x, next.y] == 1)
            return;

        // 이동 확정
        playerPos = next;
        UpdateWorldPosition();
    }

    void UpdateWorldPosition()
    {
        // 그리드 좌표 → 월드 좌표
        transform.position = new Vector3(playerPos.x, playerPos.y, 0f);
    }
}