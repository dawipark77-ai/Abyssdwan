using UnityEngine;

/// <summary>
/// 3D 던전을 2D 스타일로 렌더링하기 위한 설정
/// </summary>
[RequireComponent(typeof(Camera))]
public class PixelArtRenderer : MonoBehaviour
{
    [Header("Pixel Art Settings")]
    public int pixelResolution = 320; // 픽셀 해상도 (낮을수록 더 픽셀 아트 느낌)
    public bool useOrthographic = true; // Orthographic 카메라 사용
    public float orthographicSize = 5f; // Orthographic 크기
    
    [Header("Rendering Settings")]
    public FilterMode textureFilterMode = FilterMode.Point; // 픽셀 아트용 필터
    public bool pixelPerfect = true;
    
    private Camera cam;
    private RenderTexture renderTexture;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        SetupPixelArtCamera();
    }
    
    void SetupPixelArtCamera()
    {
        if (cam == null) return;
        
        // Orthographic 모드로 전환
        if (useOrthographic)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
        }
        
        // 낮은 해상도 렌더 텍스처 생성 (픽셀 아트 효과)
        if (pixelPerfect)
        {
            int width = pixelResolution;
            int height = Mathf.RoundToInt(pixelResolution * (Screen.height / (float)Screen.width));
            
            renderTexture = new RenderTexture(width, height, 24);
            renderTexture.filterMode = textureFilterMode;
            cam.targetTexture = renderTexture;
            
            // 렌더 텍스처를 화면에 표시할 오브젝트 생성
            CreateDisplayQuad();
        }
    }
    
    void CreateDisplayQuad()
    {
        // 화면 전체를 덮는 Quad 생성
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "PixelArtDisplay";
        quad.transform.SetParent(transform);
        quad.transform.localPosition = new Vector3(0, 0, cam.nearClipPlane + 0.1f);
        quad.transform.localRotation = Quaternion.identity;
        quad.transform.localScale = new Vector3(2, 2, 1);
        
        // Quad를 카메라 앞에 배치
        float distance = cam.nearClipPlane + 0.1f;
        quad.transform.position = cam.transform.position + cam.transform.forward * distance;
        quad.transform.LookAt(cam.transform);
        quad.transform.Rotate(180, 0, 0);
        
        // Material 생성 및 적용
        Material mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = renderTexture;
        quad.GetComponent<Renderer>().material = mat;
        
        // Collider 제거 (필요없음)
        Destroy(quad.GetComponent<Collider>());
    }
    
    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}








