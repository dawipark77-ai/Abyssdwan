using UnityEngine;

/// <summary>
/// 2D 던전 배경 이미지를 3D 공간에 배치하는 스크립트
/// PNG 이미지를 드래그 앤 드롭으로 설정할 수 있습니다.
/// </summary>
public class DungeonBackground2D : MonoBehaviour
{
    [Header("Background Image")]
    [Tooltip("배경으로 사용할 PNG 이미지를 여기에 드래그하세요")]
    public Sprite backgroundSprite;
    
    [Header("Background Settings")]
    [Tooltip("배경을 타일링할지 여부")]
    public bool tileBackground = true;
    
    [Tooltip("배경 타일 크기 (타일링 사용 시)")]
    public float tileSize = 10f;
    
    [Tooltip("배경을 카메라 앞에 고정할지 여부")]
    public bool lockToCamera = false;
    
    [Tooltip("배경 거리 (카메라로부터의 거리)")]
    public float backgroundDistance = 20f;
    
    [Header("Dungeon Settings")]
    public int corridorLength = 30;
    public float corridorWidth = 4f;
    
    private Camera mainCamera;
    private GameObject backgroundRoot;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // 바닥 생성 (추락 방지)
        CreateFloor();
        
        if (backgroundSprite != null)
        {
            CreateBackground();
        }
        else
        {
            Debug.LogWarning("[DungeonBackground2D] Background sprite is not assigned!");
        }
    }
    
    void Update()
    {
        if (lockToCamera && mainCamera != null && backgroundRoot != null)
        {
            // 배경을 카메라 앞에 고정
            backgroundRoot.transform.position = mainCamera.transform.position + mainCamera.transform.forward * backgroundDistance;
            backgroundRoot.transform.rotation = mainCamera.transform.rotation;
        }
    }
    
    void CreateFloor()
    {
        // 바닥 생성 (추락 방지)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, 0, corridorLength / 2);
        floor.transform.localScale = new Vector3(corridorWidth / 10f, 1, corridorLength / 10f);
        
        // 바닥 머티리얼 설정
        Material floorMat = new Material(Shader.Find("Standard"));
        floorMat.color = new Color(0.2f, 0.2f, 0.25f);
        floor.GetComponent<Renderer>().material = floorMat;
        
        Debug.Log("[DungeonBackground2D] Floor created at Y=0");
    }
    
    [ContextMenu("Create Background")]
    public void CreateBackground()
    {
        if (backgroundSprite == null)
        {
            Debug.LogError("[DungeonBackground2D] Background sprite is null!");
            return;
        }
        
        // 기존 배경 제거
        if (backgroundRoot != null)
        {
            DestroyImmediate(backgroundRoot);
        }
        
        backgroundRoot = new GameObject("Background_Root");
        backgroundRoot.transform.SetParent(transform);
        
        if (tileBackground)
        {
            CreateTiledBackground();
        }
        else
        {
            CreateSingleBackground();
        }
        
        Debug.Log("[DungeonBackground2D] Background created!");
    }
    
    void CreateSingleBackground()
    {
        // 단일 큰 배경 스프라이트 생성
        GameObject bg = CreateBackgroundSprite("Background", backgroundSprite, 
            Vector3.zero, Quaternion.identity);
        
        // 배경 크기를 복도 크기에 맞춤
        float spriteWidth = backgroundSprite.bounds.size.x;
        float spriteHeight = backgroundSprite.bounds.size.y;
        float scaleX = corridorWidth / spriteWidth;
        float scaleY = corridorLength / spriteHeight;
        bg.transform.localScale = new Vector3(scaleX, scaleY, 1);
        
        // 카메라 앞에 배치
        if (mainCamera != null)
        {
            bg.transform.position = mainCamera.transform.position + mainCamera.transform.forward * backgroundDistance;
            bg.transform.LookAt(mainCamera.transform);
            bg.transform.Rotate(0, 180, 0);
        }
    }
    
    void CreateTiledBackground()
    {
        // 배경을 타일링하여 복도처럼 배치
        int tilesX = Mathf.CeilToInt(corridorWidth / tileSize);
        int tilesZ = Mathf.CeilToInt(corridorLength / tileSize);
        
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                Vector3 pos = new Vector3(
                    -corridorWidth / 2 + x * tileSize + tileSize / 2,
                    0,
                    z * tileSize + tileSize / 2
                );
                
                GameObject tile = CreateBackgroundSprite($"Background_{x}_{z}", backgroundSprite, 
                    pos, Quaternion.identity);
                tile.transform.localScale = Vector3.one * tileSize;
                
                // 카메라를 바라보도록
                if (mainCamera != null)
                {
                    tile.transform.LookAt(mainCamera.transform);
                    tile.transform.Rotate(0, 180, 0);
                }
            }
        }
    }
    
    GameObject CreateBackgroundSprite(string name, Sprite sprite, Vector3 position, Quaternion rotation)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(backgroundRoot.transform);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -100; // 배경이므로 뒤에 렌더링
        
        // 픽셀 아트 스타일
        if (sprite != null && sprite.texture != null)
        {
            sprite.texture.filterMode = FilterMode.Point;
        }
        
        return obj;
    }
}

