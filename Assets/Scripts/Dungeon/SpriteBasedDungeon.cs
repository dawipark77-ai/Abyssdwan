using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ?ㅽ봽?쇱씠??湲곕컲 ?섏쟾 ?앹꽦 (?꾩쟾??2D ?ㅽ???
/// 3D 怨듦컙??2D ?ㅽ봽?쇱씠?몃? 諛곗튂?섏뿬 2D泥섎읆 蹂댁씠寃???/// </summary>
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
        // Start?먯꽌 ?먮룞 ?앹꽦?섏? ?딆쓬 (???ㅼ젙?먯꽌 ?몄텧)
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
        
        // 諛붾떏 ?ㅽ봽?쇱씠?몃뱾 諛곗튂
        CreateSpriteFloor(dungeonRoot.transform);
        
        // 踰??ㅽ봽?쇱씠?몃뱾 諛곗튂
        CreateSpriteWalls(dungeonRoot.transform);
        
        // ?꾩튂 ?ㅽ봽?쇱씠?몃뱾 諛곗튂
        CreateSpriteArches(dungeonRoot.transform);
    }
    
    void CreateSpriteFloor(Transform parent)
    {
        // 諛붾떏???щ윭 ?ㅽ봽?쇱씠?몃줈 ??쇰쭅 (?꾩뿉???대젮?ㅻ낫??媛곷룄)
        int floorTiles = Mathf.CeilToInt(corridorLength / spriteScale);
        for (int i = 0; i < floorTiles; i++)
        {
            Vector3 floorPos = new Vector3(0, -1.8f, i * spriteScale);
            GameObject floorTile = CreateSpriteObject("FloorTile_" + i, floorSprite ?? CreateDefaultFloorSprite(), 
                floorPos, parent);
            // 諛붾떏 ?ㅽ봽?쇱씠???ш린 議곗젙
            floorTile.transform.localScale = new Vector3(corridorWidth, 1, 1);
            // 諛붾떏? ?꾩뿉???대젮?ㅻ낫??媛곷룄濡??뚯쟾
            floorTile.transform.rotation = Quaternion.Euler(90, 0, 0);
            // Billboard ?쒓굅 (諛붾떏? 怨좎젙)
            BillboardSprite billboard = floorTile.GetComponent<BillboardSprite>();
            if (billboard != null) Destroy(billboard);
        }
    }
    
    void CreateSpriteWalls(Transform parent)
    {
        // ?쇱そ 踰?- ?녿㈃??蹂댁뿬二쇰뒗 ?ㅽ봽?쇱씠??
        int wallTiles = Mathf.CeilToInt(corridorLength / spriteScale);
        for (int i = 0; i < wallTiles; i++)
        {
            Vector3 leftPos = new Vector3(-corridorWidth / 2, 0, i * spriteScale);
            GameObject leftWall = CreateSpriteObject("LeftWall_" + i, wallSprite ?? CreateDefaultWallSprite(),
                leftPos, parent);
            // 踰??ㅽ봽?쇱씠???ш린 議곗젙 (???ш쾶)
            leftWall.transform.localScale = new Vector3(1, 4, 1); // ?믪씠 4諛?
            // 踰쎌? ?녿㈃??蹂댁뿬二쇰룄濡?(?ㅻⅨ履쎌쓣 諛붾씪遊?
            leftWall.transform.rotation = Quaternion.Euler(0, 90, 0);
            // Billboard ?쒓굅 (踰쎌? 怨좎젙)
            BillboardSprite billboard = leftWall.GetComponent<BillboardSprite>();
            if (billboard != null) Destroy(billboard);
            
            Vector3 rightPos = new Vector3(corridorWidth / 2, 0, i * spriteScale);
            GameObject rightWall = CreateSpriteObject("RightWall_" + i, wallSprite ?? CreateDefaultWallSprite(),
                rightPos, parent);
            rightWall.transform.localScale = new Vector3(1, 4, 1);
            // 踰쎌? ?녿㈃??蹂댁뿬二쇰룄濡?(?쇱そ??諛붾씪遊?
            rightWall.transform.rotation = Quaternion.Euler(0, -90, 0);
            // Billboard ?쒓굅
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
        sr.sortingOrder = Mathf.RoundToInt(-position.z * 10); // Z ?쒖꽌???곕씪 ?뺣젹
        sr.sortingLayerName = "Default"; // ?뺣젹 ?덉씠???ㅼ젙
        
        // Billboard ?④낵 異붽? (移대찓?쇰? ??긽 諛붾씪蹂대룄濡?
        BillboardSprite billboard = obj.AddComponent<BillboardSprite>();
        
        if (spriteMaterial != null)
        {
            sr.material = spriteMaterial;
        }
        
        // ?띿뒪泥??꾪꽣瑜?Point濡??ㅼ젙 (?쎌? ?꾪듃 ?ㅽ???
        if (sprite != null && sprite.texture != null)
        {
            sprite.texture.filterMode = FilterMode.Point;
        }
        
        dungeonObjects.Add(obj);
        return obj;
    }
    
    void AddSpriteDecorations()
    {
        // ?붾줈 諛곗튂
        for (float z = 2f; z < corridorLength - 2f; z += torchSpacing)
        {
            CreateSpriteTorch(new Vector3(-corridorWidth / 2 + 0.6f, -1.4f, z));
        }
        
        // ?닿낏 諛곗튂
        for (int i = 0; i < corridorLength * 0.3f; i++)
        {
            CreateSpriteSkeleton(new Vector3(
                UnityEngine.Random.Range(-corridorWidth / 2 + 0.5f, corridorWidth / 2 - 0.5f),
                -1.8f,
                UnityEngine.Random.Range(1f, corridorLength - 1f)
            ));
        }
    }
    
    void CreateSpriteTorch(Vector3 position)
    {
        GameObject torch = CreateSpriteObject("Torch", torchSprite ?? CreateDefaultTorchSprite(), position, transform);
        torch.transform.localScale = Vector3.one * 0.5f;
        
        // 議곕챸 異붽?
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
        skeleton.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
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
    
    // 湲곕낯 ?ㅽ봽?쇱씠???앹꽦 (?ㅽ봽?쇱씠?멸? ?놁쓣 ??
    Sprite CreateDefaultWallSprite()
    {
        Texture2D tex = new Texture2D(64, 128, TextureFormat.RGBA32, false); // ?믪씠 2諛?
        Color wallColor = new Color(0.35f, 0.35f, 0.4f);
        
        // 踰??띿뒪泥섏뿉 媛꾨떒???⑦꽩 異붽? (2D ?먮굦)
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 128; y++)
            {
                // ??釉붾줉 ?⑦꽩
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
        tex.filterMode = FilterMode.Point; // ?쎌? ?꾪듃 ?ㅽ???
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 128), new Vector2(0.5f, 0.5f), 64);
    }
    
    Sprite CreateDefaultFloorSprite()
    {
        Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color floorColor = new Color(0.25f, 0.25f, 0.3f);
        
        // 諛붾떏 ????⑦꽩
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                // ???寃쎄퀎??
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
        tex.filterMode = FilterMode.Point; // ?쎌? ?꾪듃 ?ㅽ???
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
                if ((x > 10 && x < 22 && y > 20 && y < 32) || // 癒몃━
                    (x > 14 && x < 18 && y > 10 && y < 20)) // 紐명넻
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


