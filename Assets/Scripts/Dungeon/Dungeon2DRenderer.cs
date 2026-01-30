using UnityEngine;

/// <summary>
/// 던전을 2D 스타일로 렌더링하기 위한 설정
/// 카메라와 렌더링 설정을 조정하여 픽셀 아트 느낌을 만듭니다.
/// </summary>
public class Dungeon2DRenderer : MonoBehaviour
{
    [Header("2D Rendering Settings")]
    [Tooltip("픽셀 아트 해상도 (낮을수록 더 픽셀 아트 느낌)")]
    public int pixelResolution = 320;
    
    [Tooltip("텍스처 필터 모드 (Point = 픽셀 아트, Bilinear = 부드러움)")]
    public FilterMode textureFilterMode = FilterMode.Point;
    
    [Tooltip("안티앨리어싱 비활성화 (픽셀 아트 스타일)")]
    public bool disableAntiAliasing = true;
    
    private Camera mainCamera;
    private RenderTexture renderTexture;
    private GameObject displayQuad;
    
    void Start()
    {
        Setup2DRendering();
    }
    
    void Setup2DRendering()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogWarning("[Dungeon2DRenderer] No camera found!");
            return;
        }
        
        // 카메라가 직접 화면에 렌더링하도록 설정
        mainCamera.targetTexture = null;
        mainCamera.enabled = true;
        mainCamera.gameObject.SetActive(true);
        
        // 모든 텍스처에 Point 필터 적용 (픽셀 아트 스타일)
        ApplyPixelArtFilterToAllTextures();
        
        Debug.Log($"[Dungeon2DRenderer] 2D rendering setup complete. Camera: {mainCamera.name}, Enabled: {mainCamera.enabled}, Active: {mainCamera.gameObject.activeSelf}");
    }
    
    void CreateDisplayQuad()
    {
        // 렌더 텍스처를 사용하지 않으므로 Quad 생성 불필요
        // 대신 UI Canvas를 사용하거나 카메라를 직접 렌더링
    }
    
    void ApplyPixelArtFilterToAllTextures()
    {
        // 씬의 모든 Renderer에 Point 필터 적용
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material != null && renderer.material.mainTexture != null)
            {
                Texture2D tex = renderer.material.mainTexture as Texture2D;
                if (tex != null)
                {
                    tex.filterMode = textureFilterMode;
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        
        if (displayQuad != null)
        {
            Destroy(displayQuad);
        }
    }
}

