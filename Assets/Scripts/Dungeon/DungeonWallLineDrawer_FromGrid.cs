using UnityEngine;

public class DungeonWallLineDrawer_FromGrid : MonoBehaviour
{
    public int width;
    public int height;
    public float cellSize = 1f;
    public float lineWidth = 0.05f;
    public Color lineColor = Color.white;

    // 예시: 외부에서 채워질 던전 데이터
    public int[,] dungeon;

    void Start()
    {
        if (dungeon == null)
        {
            Debug.LogError("Dungeon data is null");
            return;
        }

        DrawWalls();
    }

    void DrawWalls()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (dungeon[x, y] != 1)
                    continue;

                Vector3 cellCenter = new Vector3(
                    x * cellSize,
                    y * cellSize,
                    -1f
                );

                // 위
                if (IsEmpty(x, y + 1))
                    DrawLine(
                        cellCenter + new Vector3(-0.5f, 0.5f),
                        cellCenter + new Vector3(0.5f, 0.5f)
                    );

                // 아래
                if (IsEmpty(x, y - 1))
                    DrawLine(
                        cellCenter + new Vector3(-0.5f, -0.5f),
                        cellCenter + new Vector3(0.5f, -0.5f)
                    );

                // 왼쪽
                if (IsEmpty(x - 1, y))
                    DrawLine(
                        cellCenter + new Vector3(-0.5f, -0.5f),
                        cellCenter + new Vector3(-0.5f, 0.5f)
                    );

                // 오른쪽
                if (IsEmpty(x + 1, y))
                    DrawLine(
                        cellCenter + new Vector3(0.5f, -0.5f),
                        cellCenter + new Vector3(0.5f, 0.5f)
                    );
            }
        }
    }

    bool IsEmpty(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return true;

        return dungeon[x, y] == 0;
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("WallLine");
        lineObj.transform.parent = transform;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lineColor;
        lr.endColor = lineColor;
    }
}
