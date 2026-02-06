using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ?섏쟾 ?щ·??02 - 3D 怨듦컙??2D ?ㅽ??쇰줈 ?뚮뜑留곷릺???섏쟾 ?앹꽦湲?/// </summary>
public class DungeonCrawler02Generator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int corridorLength = 30;
    public float corridorWidth = 4f;
    public float tileSize = 1f; // ????ш린
    
    [Header("Sprite Settings")]
    public bool usePixelArt = true;
    public int spriteResolution = 64; // ?ㅽ봽?쇱씠???댁긽??    
    private List<GameObject> dungeonObjects = new List<GameObject>();
    private GameObject dungeonRoot;
    
    void Start()
    {
        GenerateDungeon();
    }
    
    public void GenerateDungeon()
    {
        ClearDungeon();
        dungeonRoot = new GameObject("Dungeon02_Root");
        dungeonObjects.Add(dungeonRoot);
        
        CreateFloor();
        CreateWalls();
        CreateArches();
        AddTorches();
        AddDecorations();
        SetupLighting();
        
        Debug.Log($"[DungeonCrawler02] Generated dungeon with {dungeonObjects.Count} objects");
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
        
        GameObject oldRoot = GameObject.Find("Dungeon02_Root");
        if (oldRoot != null)
        {
            DestroyImmediate(oldRoot);
        }
    }
    
    void CreateFloor()
    {
        // 諛붾떏 Y ?꾩튂: ?뚮젅?댁뼱 諛??꾩튂 (CharacterController center媛 1f?대?濡?諛쒖? 0f)
        float floorY = 0f;
        
        // ???⑥씪 諛붾떏 ?앹꽦 (?뺤떎??蹂댁씠?꾨줉)
        GameObject mainFloor = CreateSpriteTile("MainFloor", 
            CreateFloorSprite(), 
            new Vector3(0, floorY, corridorLength / 2), 
            Quaternion.Euler(90, 0, 0), 
            dungeonRoot.transform);
        mainFloor.transform.localScale = new Vector3(corridorWidth * 1.5f, corridorLength * 1.5f, 1);
        
        BillboardSprite mainBillboard = mainFloor.GetComponent<BillboardSprite>();
        if (mainBillboard != null)
        {
            DestroyImmediate(mainBillboard);
        }
        
        // 異붽?: 諛붾떏 ????앹꽦 (?μ떇??
        int tilesX = Mathf.CeilToInt(corridorWidth / tileSize);
        int tilesZ = Mathf.CeilToInt(corridorLength / tileSize);
        
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                Vector3 pos = new Vector3(
                    -corridorWidth / 2 + x * tileSize + tileSize / 2,
                    floorY,
                    z * tileSize + tileSize / 2
                );
                
                GameObject floorTile = CreateSpriteTile("Floor_" + x + "_" + z, 
                    CreateFloorSprite(), pos, Quaternion.Euler(90, 0, 0), dungeonRoot.transform);
                floorTile.transform.localScale = new Vector3(tileSize, tileSize, 1);
                
                BillboardSprite billboard = floorTile.GetComponent<BillboardSprite>();
                if (billboard != null)
                {
                    DestroyImmediate(billboard);
                }
            }
        }
        
        Debug.Log($"[DungeonCrawler02] Created floor at Y={floorY} with {tilesX * tilesZ} tiles");
    }
    
    void CreateWalls()
    {
        // ?쇱そ 踰?
        int wallTiles = Mathf.CeilToInt(corridorLength / tileSize);
        for (int i = 0; i < wallTiles; i++)
        {
            Vector3 leftPos = new Vector3(-corridorWidth / 2, 0, i * tileSize + tileSize / 2);
            GameObject leftWall = CreateSpriteTile("LeftWall_" + i, 
                CreateWallSprite(), leftPos, Quaternion.Euler(0, 90, 0), dungeonRoot.transform);
            leftWall.transform.localScale = new Vector3(tileSize, tileSize * 4, 1); // ?믪씠 4諛?            
            Vector3 rightPos = new Vector3(corridorWidth / 2, 0, i * tileSize + tileSize / 2);
            GameObject rightWall = CreateSpriteTile("RightWall_" + i, 
                CreateWallSprite(), rightPos, Quaternion.Euler(0, -90, 0), dungeonRoot.transform);
            rightWall.transform.localScale = new Vector3(tileSize, tileSize * 4, 1);
        }
    }
    
    void CreateArches()
    {
        int archCount = Mathf.FloorToInt(corridorLength / 4f);
        for (int i = 1; i < archCount; i++)
        {
            float zPos = i * 4f;
            GameObject arch = CreateSpriteTile("Arch_" + i, 
                CreateArchSprite(), new Vector3(0, 2f, zPos), Quaternion.identity, dungeonRoot.transform);
            arch.transform.localScale = new Vector3(corridorWidth, 2f, 1);
        }
    }
    
    void AddTorches()
    {
        for (float z = 2f; z < corridorLength - 2f; z += 8f)
        {
            // ?쇱そ 踰??껊? (諛붾떏 ??
            CreateTorch(new Vector3(-corridorWidth / 2 + 0.3f, 1.5f, z));
            // ?ㅻⅨ履?踰??껊? (諛붾떏 ??
            CreateTorch(new Vector3(corridorWidth / 2 - 0.3f, 1.5f, z));
        }
    }
    
    void CreateTorch(Vector3 position)
    {
        GameObject torch = CreateSpriteTile("Torch", 
            CreateTorchSprite(), position, Quaternion.identity, dungeonRoot.transform);
        torch.transform.localScale = Vector3.one * 0.5f;
        
        // Billboard ?④낵 (移대찓?쇰? ??긽 諛붾씪遊?
        BillboardSprite billboard = torch.AddComponent<BillboardSprite>();
        
        // 議곕챸 異붽?
        Light torchLight = torch.AddComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.color = new Color(0.3f, 0.6f, 1f);
        torchLight.intensity = 2.5f;
        torchLight.range = 8f;
        torchLight.shadows = LightShadows.Soft;
    }
    
    void AddDecorations()
    {
        // ?닿낏 諛곗튂 (諛붾떏 ??
        for (int i = 0; i < corridorLength * 0.2f; i++)
        {
            Vector3 pos = new Vector3(
                UnityEngine.Random.Range(-corridorWidth / 2 + 0.5f, corridorWidth / 2 - 0.5f),
                0f, // 諛붾떏 ??
                UnityEngine.Random.Range(1f, corridorLength - 1f)
            );
            CreateSkeleton(pos);
        }
    }
    
    void CreateSkeleton(Vector3 position)
    {
        // ?닿낏 ?꾩튂: 諛붾떏 ??
        Vector3 skeletonPos = new Vector3(position.x, 0f, position.z);
        GameObject skeleton = CreateSpriteTile("Skeleton", 
            CreateSkeletonSprite(), skeletonPos, Quaternion.identity, dungeonRoot.transform);
        skeleton.transform.localScale = Vector3.one * 0.3f;
        skeleton.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        
        // Billboard ?④낵
        BillboardSprite billboard = skeleton.AddComponent<BillboardSprite>();
    }
    
    GameObject CreateSpriteTile(string name, Sprite sprite, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = Mathf.RoundToInt(-position.z * 10);
        sr.sortingLayerName = "Default";
        
        // 諛붾떏? ??긽 蹂댁씠?꾨줉 ?ㅼ젙
        if (name.Contains("Floor"))
        {
            sr.sortingOrder = -1000; // 媛???ㅼ뿉 ?뚮뜑留?
        }
        
        // ?쎌? ?꾪듃 ?ㅽ???
        if (sprite != null && sprite.texture != null)
        {
            sprite.texture.filterMode = usePixelArt ? FilterMode.Point : FilterMode.Bilinear;
        }
        
        dungeonObjects.Add(obj);
        return obj;
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
    
    // ?ㅽ봽?쇱씠???앹꽦 硫붿꽌?쒕뱾
    Sprite CreateFloorSprite()
    {
        Texture2D tex = new Texture2D(spriteResolution, spriteResolution, TextureFormat.RGBA32, false);
        Color baseColor = new Color(0.3f, 0.3f, 0.35f); // ??諛앷쾶
        
        for (int x = 0; x < spriteResolution; x++)
        {
            for (int y = 0; y < spriteResolution; y++)
            {
                // ????⑦꽩 - ??紐낇솗?섍쾶
                int tileX = x / (spriteResolution / 4);
                int tileY = y / (spriteResolution / 4);
                
                if ((tileX + tileY) % 2 == 0)
                {
                    tex.SetPixel(x, y, baseColor * 1.15f);
                }
                else
                {
                    tex.SetPixel(x, y, baseColor * 0.85f);
                }
                
                // ???寃쎄퀎??
                if (x % (spriteResolution / 4) == 0 || y % (spriteResolution / 4) == 0)
                {
                    tex.SetPixel(x, y, baseColor * 0.7f);
                }
            }
        }
        tex.filterMode = usePixelArt ? FilterMode.Point : FilterMode.Bilinear;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, spriteResolution, spriteResolution), new Vector2(0.5f, 0.5f), spriteResolution);
    }
    
    Sprite CreateWallSprite()
    {
        Texture2D tex = new Texture2D(spriteResolution, spriteResolution * 2, TextureFormat.RGBA32, false);
        Color wallColor = new Color(0.35f, 0.35f, 0.4f);
        
        for (int x = 0; x < spriteResolution; x++)
        {
            for (int y = 0; y < spriteResolution * 2; y++)
            {
                // ??釉붾줉 ?⑦꽩
                if ((x / (spriteResolution / 8) + y / (spriteResolution / 8)) % 2 == 0)
                {
                    tex.SetPixel(x, y, wallColor * 1.1f);
                }
                else
                {
                    tex.SetPixel(x, y, wallColor * 0.9f);
                }
            }
        }
        tex.filterMode = usePixelArt ? FilterMode.Point : FilterMode.Bilinear;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, spriteResolution, spriteResolution * 2), new Vector2(0.5f, 0.5f), spriteResolution);
    }
    
    Sprite CreateArchSprite()
    {
        Texture2D tex = new Texture2D(spriteResolution, spriteResolution, TextureFormat.RGBA32, false);
        Color archColor = new Color(0.35f, 0.35f, 0.4f);
        
        for (int x = 0; x < spriteResolution; x++)
        {
            for (int y = 0; y < spriteResolution; y++)
            {
                tex.SetPixel(x, y, archColor);
            }
        }
        tex.filterMode = usePixelArt ? FilterMode.Point : FilterMode.Bilinear;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, spriteResolution, spriteResolution), new Vector2(0.5f, 0.5f), spriteResolution);
    }
    
    Sprite CreateTorchSprite()
    {
        Texture2D tex = new Texture2D(spriteResolution / 2, spriteResolution, TextureFormat.RGBA32, false);
        Color torchColor = new Color(1f, 0.5f, 0f);
        Color baseColor = new Color(0.3f, 0.2f, 0.1f);
        
        for (int x = 0; x < spriteResolution / 2; x++)
        {
            for (int y = 0; y < spriteResolution; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(spriteResolution / 4, spriteResolution * 0.7f));
                if (dist < spriteResolution / 8)
                {
                    tex.SetPixel(x, y, torchColor);
                }
                else if (dist < spriteResolution / 4)
                {
                    tex.SetPixel(x, y, baseColor);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.filterMode = usePixelArt ? FilterMode.Point : FilterMode.Bilinear;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, spriteResolution / 2, spriteResolution), new Vector2(0.5f, 0.5f), spriteResolution);
    }
    
    Sprite CreateSkeletonSprite()
    {
        Texture2D tex = new Texture2D(spriteResolution / 2, spriteResolution, TextureFormat.RGBA32, false);
        Color boneColor = Color.white;
        
        for (int x = 0; x < spriteResolution / 2; x++)
        {
            for (int y = 0; y < spriteResolution; y++)
            {
                // 媛꾨떒???닿낏 紐⑥뼇
                if ((x > spriteResolution / 8 && x < spriteResolution * 3 / 8 && y > spriteResolution * 0.6f && y < spriteResolution * 0.9f) || // 癒몃━
                    (x > spriteResolution / 6 && x < spriteResolution / 3 && y > spriteResolution * 0.3f && y < spriteResolution * 0.6f)) // 紐명넻
                {
                    tex.SetPixel(x, y, boneColor);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.filterMode = usePixelArt ? FilterMode.Point : FilterMode.Bilinear;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, spriteResolution / 2, spriteResolution), new Vector2(0.5f, 0.5f), spriteResolution);
    }
}


