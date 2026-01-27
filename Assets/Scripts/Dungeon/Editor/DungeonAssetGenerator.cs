using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class DungeonAssetGenerator : EditorWindow
{
    [MenuItem("Dungeon/Generate Placeholder Assets")]
    public static void GenerateAssets()
    {
        string path = "Assets/Images";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        // 1. Settings
        int width = 512;
        int height = 512;
        Color wallColor = new Color(0.4f, 0.4f, 0.45f); // Stone Gray
        Color lineColor = Color.black;
        
        // 2. Generate Front Wall (Square)
        Texture2D texFront = new Texture2D(width, height);
        FillColor(texFront, Color.clear);
        
        // Draw a square in the middle? No, the Image itself will be sized. 
        // Just make a brick texture.
        DrawBrickPattern(texFront, wallColor, lineColor);
        SaveTexture(texFront, path + "/Wall_Front.png");

        // 3. Generate Side Wall Left (Trapezoid)
        // To make it fit perfectly, we need to transparency mask it.
        // Let's assume the Side Wall occupies the full 512x512 texture, 
        // but we draw a trapezoid shape: 
        // Left Edge: height 100%
        // Right Edge: height 60% (centered)
        Texture2D texSide = new Texture2D(width, height);
        FillColor(texSide, Color.clear);
        DrawTrapezoid(texSide, wallColor, lineColor, true);
        SaveTexture(texSide, path + "/Wall_Side_Left.png");

        // 4. Generate Side Wall Right (Flip of Left)
        Texture2D texSideR = new Texture2D(width, height);
        FillColor(texSideR, Color.clear);
        DrawTrapezoid(texSideR, wallColor, lineColor, false);
        SaveTexture(texSideR, path + "/Wall_Side_Right.png");

        AssetDatabase.Refresh();
        Debug.Log("Dungeon Assets Generated in Assets/Images/");
    }

    private static void FillColor(Texture2D tex, Color c)
    {
        Color[] cols = new Color[tex.width * tex.height];
        for (int i = 0; i < cols.Length; i++) cols[i] = c;
        tex.SetPixels(cols);
    }

    private static void DrawBrickPattern(Texture2D tex, Color baseColor, Color lineColor)
    {
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                // Simple Grid bricks
                bool isLine = (y % 64 < 2) || (x % 128 < 2);
                // Offset every other row
                if ((y / 64) % 2 == 1)
                {
                    isLine = (y % 64 < 2) || ((x + 64) % 128 < 2);
                }
                
                // Border
                if (x < 4 || x > tex.width - 4 || y < 4 || y > tex.height - 4) isLine = true;

                tex.SetPixel(x, y, isLine ? lineColor : baseColor);
            }
        }
    }

    private static void DrawTrapezoid(Texture2D tex, Color baseColor, Color lineColor, bool isLeft)
    {
        // Left Wall: Left edge is full height, Right edge is smaller.
        // Right Wall: Left edge is smaller, Right edge is full height.
        
        float hLeft = isLeft ? 1.0f : 0.6f;
        float hRight = isLeft ? 0.6f : 1.0f;

        for (int x = 0; x < tex.width; x++)
        {
            float t = (float)x / tex.width;
            float hCurrent = Mathf.Lerp(hLeft, hRight, t);
            
            int yCenter = tex.height / 2;
            int yHalf = (int)((tex.height * hCurrent) / 2);
            int yMin = yCenter - yHalf;
            int yMax = yCenter + yHalf;

            for (int y = 0; y < tex.height; y++)
            {
                if (y >= yMin && y <= yMax)
                {
                    // Brick Logic (projected?) - Keep it simple for now, just flat color with border
                    bool border = (y - yMin < 4) || (yMax - y < 4) || (x < 4) || (x > tex.width - 4);
                    // Add some vertical lines for perspective hint
                    if (x % 100 == 0) border = true;
                    
                    tex.SetPixel(x, y, border ? lineColor : baseColor);
                }
            }
        }
    }

    private static void SaveTexture(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }
}
#endif
