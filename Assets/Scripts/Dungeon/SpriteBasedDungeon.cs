using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스프라이트 기반 던전 생성 (완전한 2D 스타일)
/// 3D 공간에 2D 스프라이트를 배치하여 2D처럼 보이게 함
/// </summary>
public class SpriteBasedDungeon : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int corridorLength = 30;
    public float corridorWidth = 4f;
    public float spriteScale = 1f;
    
    [Header("Sprites (2D Textures)")]
    public Sprite wallSprite;
    public Sprite floorSprite;
    public Sprite archSprite;
    public Sprite torchSprite;
    public Sprite skeletonSprite;
    public Sprite potterySprite;
    
    [Header("Materials")]
    public Material spriteMaterial;
    
    [Header("Lighting")]
    public Color torchLightColor = new Color(0.3f, 0.6f, 1f);
    public float torchLightIntensity = 2.5f;
    public float torchLightRange = 8f;
    public float torchSpacing = 8f;
    
    private List<GameObject> dungeonObjects = new List<GameObject>();
    
    void Start()
    {
        // Start에서 자동 생성하지 않음 (씬 설정에서 호출)
        // GenerateSpriteDungeon();
    }
    
    public void GenerateSpriteDungeon()
    {
        Debug.Log("[SpriteBasedDungeon] Generating sprite-based dungeon...");
        ClearDungeon();
        CreateSpriteCorridor();
        AddSpriteDecorations();
        SetupLighting();
        Debug.Log($"[SpriteBasedDungeon] Dungeon generated with {dungeonObjects.Count} sprite objects");
    }
    
    void CreateSpriteCorridor()
    {
        GameObject dungeonRoot = new GameObject("SpriteDungeon");
        dungeonRoot.transform.SetParent(transform);
        
        // 바닥 스프라이트들 배치
        CreateSpriteFloor(dungeonRoot.transform);
        
        // 벽 스프라이트들 배치
        CreateSpriteWalls(dungeonRoot.transform);
        
        // 아치 스프라이트들 배치
        CreateSpriteArches(dungeonRoot.transform);
    }
    
    void CreateSpriteFloor(Transform parent)
    {
        // 바닥을 여러 스프라이트로 타일링 (위에서 내려다보는 각도)
        int floorTiles = Mathf.CeilToInt(corridorLength / spriteScale);
        for (int i = 0; i < floorTiles; i++)
        {
            Vector3 floorPos = new Vector3(0, -1.8f, i * spriteScale);
            GameObject floorTile = CreateSpriteObject("FloorTile_" + i, floorSprite ?? CreateDefaultFloorSprite(), 
                floorPos, parent);
            // 바닥 스프라이트 크기 조정
            floorTile.transform.localScale = new Vector3(corridorWidth, 1, 1);
            // 바닥은 위에서 내려다보는 각도로 회전
            floorTile.transform.rotation = Quaternion.Euler(90, 0, 0);
            // Billboard 제거 (바닥은 고정)
            BillboardSprite billboard = floorTile.GetComponent<BillboardSprite>();
            if (billboard != null) Destroy(billboard);
        }
    }
    
    void CreateSpriteWalls(Transform parent)
    {
        // 왼쪽 벽 - 옆면을 보여주는 스프라이트
        int wallTiles = Mathf.CeilToInt(corridorLength / spriteScale);
        for (int i = 0; i < wallTiles; i++)
        {
            Vector3 leftPos = new Vector3(-corridorWidth / 2, 0, i * spriteScale);
            GameObject leftWall = CreateSpriteObject("LeftWall_" + i, wallSprite ?? CreateDefaultWallSprite(),
                leftPos, parent);
            // 벽 스프라이트 크기 조정 (더 크게)
            leftWall.transform.localScale = new Vector3(1, 4, 1); // 높이 4배
            // 벽은 옆면을 보여주도록 (오른쪽을 바라봄)
            leftWall.transform.rotation = Quaternion.Euler(0, 90, 0);
            // Billboard 제거 (벽은 고정)
            BillboardSprite billboard = leftWall.GetComponent<BillboardSprite>();
            if (billboard != null) Destroy(billboard);
            
            Vector3 rightPos = new Vector3(corridorWidth / 2, 0, i * spriteScale);
            GameObject rightWall = CreateSpriteObject("RightWall_" + i, wallSprite ?? CreateDefaultWallSprite(),
                rightPos, parent);
            rightWall.transform.localScale = new Vector3(1, 4, 1);
            // 벽은 옆면을 보여주도록 (왼쪽을 바라봄)
            rightWall.transform.rotation = Quaternion.Euler(0, -90, 0);
            // Billboard 제거
            billboard = rightWall.GetComponent<BillboardSprite>();
            if (billboard != null) Destroy(billboard);
        }
    }
    
    void CreateSpriteArches(Transform parent)
    {
        int archCount = Mathf.FloorToInt(corridorLength / 4f);
        for (int i = 1; i < archCount; i++)
        {
            float zPos = i * 4f;
            GameObject arch = CreateSpriteObject("Arch_" + i, archSprite ?? CreateDefaultArchSprite(),
                new Vector3(0, 1.5f, zPos), parent);
            arch.transform.localScale = Vector3.one * spriteScale;
        }
    }
    
    GameObject CreateSpriteObject(string name, Sprite sprite, Vector3 position, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = position;
        
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = Mathf.RoundToInt(-position.z * 10); // Z 순서에 따라 정렬
        sr.sortingLayerName = "Default"; // 정렬 레이어 설정
        
        // Billboard 효과 추가 (카메라를 항상 바라보도록)
        BillboardSprite billboard = obj.AddComponent<BillboardSprite>();
        
        if (spriteMaterial != null)
        {
            sr.material = spriteMaterial;
        }
        
        // 텍스처 필터를 Point로 설정 (픽셀 아트 스타일)
        if (sprite != null && sprite.texture != null)
        {
            sprite.texture.filterMode = FilterMode.Point;
        }
        
        dungeonObjects.Add(obj);
        return obj;
    }
    
    void AddSpriteDecorations()
    {
        // 화로 배치
        for (float z = 2f; z < corridorLength - 2f; z += torchSpacing)
        {
            CreateSpriteTorch(new Vector3(-corridorWidth / 2 + 0.6f, -1.4f, z));
        }
        
        // 해골 배치
        for (int i = 0; i < corridorLength * 0.3f; i++)
        {
            CreateSpriteSkeleton(new Vector3(
                Random.Range(-corridorWidth / 2 + 0.5f, corridorWidth / 2 - 0.5f),
                -1.8f,
                Random.Range(1f, corridorLength - 1f)
            ));
        }
    }
    
    void CreateSpriteTorch(Vector3 position)
    {
        GameObject torch = CreateSpriteObject("Torch", torchSprite ?? CreateDefaultTorchSprite(), position, transform);
        torch.transform.localScale = Vector3.one * 0.5f;
        
        // 조명 추가
        Light torchLight = torch.AddComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.color = torchLightColor;
        torchLight.intensity = torchLightIntensity;
        torchLight.range = torchLightRange;
        torchLight.shadows = LightShadows.Soft;
    }
    
    void CreateSpriteSkeleton(Vector3 position)
    {
        GameObject skeleton = CreateSpriteObject("Skeleton", skeletonSprite ?? CreateDefaultSkeletonSprite(), 
            position, transform);
        skeleton.transform.localScale = Vector3.one * 0.3f;
        skeleton.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
    }
    
    void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.1f, 0.1f, 0.15f);
        RenderSettings.ambientEquatorColor = new Color(0.05f, 0.05f, 0.1f);
        RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.05f);
        RenderSettings.ambientIntensity = 0.5f;
        
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.1f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.015f;
    }
    
    // 기본 스프라이트 생성 (스프라이트가 없을 때)
    Sprite CreateDefaultWallSprite()
    {
        Texture2D tex = new Texture2D(64, 128, TextureFormat.RGBA32, false); // 높이 2배
        Color wallColor = new Color(0.35f, 0.35f, 0.4f);
        
        // 벽 텍스처에 간단한 패턴 추가 (2D 느낌)
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                // 돌 블록 패턴
                if ((x / 8 + y / 8) % 2 == 0)
                {
                    tex.SetPixel(x, y, wallColor * 1.1f);
                }
                else
                {
                    tex.SetPixel(x, y, wallColor * 0.9f);
                }
            }
        }
        tex.filterMode = FilterMode.Point; // 픽셀 아트 스타일
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 128), new Vector2(0.5f, 0.5f), 64);
    }
    
    Sprite CreateDefaultFloorSprite()
    {
        Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color floorColor = new Color(0.25f, 0.25f, 0.3f);
        
        // 바닥 타일 패턴
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                // 타일 경계선
                if (x % 16 == 0 || y % 16 == 0)
                {
                    tex.SetPixel(x, y, floorColor * 0.8f);
                }
                else
                {
                    tex.SetPixel(x, y, floorColor);
                }
            }
        }
        tex.filterMode = FilterMode.Point; // 픽셀 아트 스타일
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }
    
    Sprite CreateDefaultArchSprite()
    {
        Texture2D tex = new Texture2D(64, 64);
        Color archColor = new Color(0.35f, 0.35f, 0.4f);
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                tex.SetPixel(x, y, archColor);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }
    
    Sprite CreateDefaultTorchSprite()
    {
        Texture2D tex = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                if (dist < 8)
                {
                    tex.SetPixel(x, y, torchLightColor);
                }
                else if (dist < 12)
                {
                    tex.SetPixel(x, y, new Color(0.2f, 0.15f, 0.1f));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }
    
    Sprite CreateDefaultSkeletonSprite()
    {
        Texture2D tex = new Texture2D(32, 32);
        Color boneColor = Color.white;
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                if ((x > 10 && x < 22 && y > 20 && y < 32) || // 머리
                    (x > 14 && x < 18 && y > 10 && y < 20)) // 몸통
                {
                    tex.SetPixel(x, y, boneColor);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }
    
    void ClearDungeon()
    {
        foreach (GameObject obj in dungeonObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        dungeonObjects.Clear();
    }
    
    void OnDestroy()
    {
        ClearDungeon();
    }
}

