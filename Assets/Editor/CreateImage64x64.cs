using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateImage64x64 : EditorWindow
{
    [MenuItem("Tools/Create 64x64 Image")]
    public static void CreateImage()
    {
        // 64x64 크기의 Texture2D 생성
        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGB24, false);
        
        // 검은색으로 초기화
        Color[] pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }
        
        // 빨간색 직사각형 그리기
        // 첫 번째 직사각형 (위쪽, y: 15-18, x: 20-44)
        for (int y = 15; y < 18; y++)
        {
            for (int x = 20; x < 44; x++)
            {
                pixels[y * 64 + x] = Color.red;
            }
        }
        
        // 두 번째 직사각형 (아래쪽, y: 46-49, x: 20-44)
        for (int y = 46; y < 49; y++)
        {
            for (int x = 20; x < 44; x++)
            {
                pixels[y * 64 + x] = Color.red;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // PNG로 저장
        byte[] pngData = texture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, "red_rectangles_64x64.png");
        File.WriteAllBytes(path, pngData);
        
        AssetDatabase.Refresh();
        Debug.Log($"이미지가 생성되었습니다: {path}");
        
        DestroyImmediate(texture);
    }
}













