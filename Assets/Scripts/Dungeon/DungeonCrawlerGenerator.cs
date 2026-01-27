using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 1인칭 던전 크롤러용 던전 생성기
/// 3D로 만들고 2D 스타일로 렌더링
/// </summary>
public class DungeonCrawlerGenerator : MonoBehaviour
{
    [Header("Dungeon Settings")]
    public int corridorLength = 30;
    public float corridorWidth = 4f;
    public float corridorHeight = 4f;
    public float wallThickness = 0.5f;
    public float archSpacing = 4f; // 아치 간격
    
    [Header("Materials")]
    public Material wallMaterial;
    public Material floorMaterial;
    public Material ceilingMaterial;
    
    [Header("Decoration")]
    public GameObject torchPrefab;
    public GameObject skeletonPrefab;
    public GameObject potteryPrefab;
    public float torchSpacing = 8f;
    public float decorationDensity = 0.4f;
    
    [Header("Lighting")]
    public Color torchLightColor = new Color(0.3f, 0.6f, 1f); // 푸른 불꽃
    public float torchLightIntensity = 2.5f;
    public float torchLightRange = 8f;
    
    private List<GameObject> dungeonObjects = new List<GameObject>();
    
    void Start()
    {
        // 자동 생성 비활성화 (스프라이트 기반 던전 사용)
        // GenerateDungeon();
    }
    
    public void GenerateDungeon()
    {
        ClearDungeon();
        CreateCorridor();
        AddDecorations();
        SetupLighting();
        Debug.Log($"[DungeonCrawlerGenerator] Dungeon generated! Created {dungeonObjects.Count} objects.");
    }
    
    void CreateCorridor()
    {
        GameObject dungeonRoot = new GameObject("Dungeon");
        dungeonRoot.transform.SetParent(transform);
        
        // 바닥
        CreateFloor(dungeonRoot.transform);
        
        // 벽
        CreateWalls(dungeonRoot.transform);
        
        // 천장
        CreateCeiling(dungeonRoot.transform);
        
        // 아치형 문들
        CreateArches(dungeonRoot.transform);
    }
    
    void CreateFloor(Transform parent)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(parent);
        floor.transform.localPosition = new Vector3(0, -corridorHeight / 2, corridorLength / 2);
        floor.transform.localScale = new Vector3(corridorWidth, 0.1f, corridorLength);
        
        if (floorMaterial != null)
        {
            floor.GetComponent<Renderer>().material = floorMaterial;
        }
        else
        {
            SetDefaultMaterial(floor, new Color(0.25f, 0.25f, 0.3f));
        }
        
        dungeonObjects.Add(floor);
    }
    
    void CreateWalls(Transform parent)
    {
        // 왼쪽 벽
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.SetParent(parent);
        leftWall.transform.localPosition = new Vector3(-corridorWidth / 2, 0, corridorLength / 2);
        leftWall.transform.localScale = new Vector3(wallThickness, corridorHeight, corridorLength);
        
        if (wallMaterial != null)
        {
            leftWall.GetComponent<Renderer>().material = wallMaterial;
        }
        else
        {
            SetDefaultMaterial(leftWall, new Color(0.35f, 0.35f, 0.4f));
        }
        
        dungeonObjects.Add(leftWall);
        
        // 오른쪽 벽
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.SetParent(parent);
        rightWall.transform.localPosition = new Vector3(corridorWidth / 2, 0, corridorLength / 2);
        rightWall.transform.localScale = new Vector3(wallThickness, corridorHeight, corridorLength);
        
        if (wallMaterial != null)
        {
            rightWall.GetComponent<Renderer>().material = wallMaterial;
        }
        else
        {
            SetDefaultMaterial(rightWall, new Color(0.35f, 0.35f, 0.4f));
        }
        
        dungeonObjects.Add(rightWall);
    }
    
    void CreateArches(Transform parent)
    {
        int archCount = Mathf.FloorToInt(corridorLength / archSpacing);
        for (int i = 1; i < archCount; i++)
        {
            float zPos = i * archSpacing;
            
            // 아치 상단 (간단한 박스로 표현)
            GameObject archTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archTop.name = $"ArchTop_{i}";
            archTop.transform.SetParent(parent);
            archTop.transform.localPosition = new Vector3(0, corridorHeight / 2 - 0.5f, zPos);
            archTop.transform.localScale = new Vector3(corridorWidth * 0.85f, 0.4f, 0.4f);
            
            if (wallMaterial != null)
            {
                archTop.GetComponent<Renderer>().material = wallMaterial;
            }
            else
            {
                SetDefaultMaterial(archTop, new Color(0.35f, 0.35f, 0.4f));
            }
            
            dungeonObjects.Add(archTop);
        }
    }
    
    void CreateCeiling(Transform parent)
    {
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(parent);
        ceiling.transform.localPosition = new Vector3(0, corridorHeight / 2, corridorLength / 2);
        ceiling.transform.localScale = new Vector3(corridorWidth, 0.1f, corridorLength);
        
        if (ceilingMaterial != null)
        {
            ceiling.GetComponent<Renderer>().material = ceilingMaterial;
        }
        else
        {
            SetDefaultMaterial(ceiling, new Color(0.2f, 0.2f, 0.25f));
        }
        
        dungeonObjects.Add(ceiling);
    }
    
    void AddDecorations()
    {
        // 화로 배치 (왼쪽 벽 근처)
        for (float z = 2f; z < corridorLength - 2f; z += torchSpacing)
        {
            CreateTorch(new Vector3(-corridorWidth / 2 + 0.6f, -corridorHeight / 2 + 0.4f, z));
        }
        
        // 해골/뼈 장식 (랜덤 배치)
        for (int i = 0; i < corridorLength * decorationDensity; i++)
        {
            if (Random.value < 0.6f)
            {
                CreateSkeleton(new Vector3(
                    Random.Range(-corridorWidth / 2 + 0.5f, corridorWidth / 2 - 0.5f),
                    -corridorHeight / 2 + 0.1f,
                    Random.Range(1f, corridorLength - 1f)
                ));
            }
        }
        
        // 도자기 조각 배치
        for (int i = 0; i < corridorLength * decorationDensity * 0.3f; i++)
        {
            CreatePottery(new Vector3(
                Random.Range(-corridorWidth / 2 + 0.5f, corridorWidth / 2 - 0.5f),
                -corridorHeight / 2 + 0.1f,
                Random.Range(1f, corridorLength - 1f)
            ));
        }
    }
    
    void CreateTorch(Vector3 position)
    {
        GameObject torch;
        
        if (torchPrefab != null)
        {
            torch = Instantiate(torchPrefab, position, Quaternion.identity);
        }
        else
        {
            // 기본 화로 생성
            torch = new GameObject("Torch");
            torch.transform.position = position;
            
            // 화로 받침대
            GameObject torchBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torchBase.transform.SetParent(torch.transform);
            torchBase.transform.localPosition = Vector3.zero;
            torchBase.transform.localScale = new Vector3(0.4f, 0.25f, 0.4f);
            SetDefaultMaterial(torchBase, new Color(0.2f, 0.15f, 0.1f));
            
            // 불꽃 (스프라이트 또는 파티클)
            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.transform.SetParent(torch.transform);
            flame.transform.localPosition = new Vector3(0, 0.4f, 0);
            flame.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
            Material flameMat = new Material(Shader.Find("Standard"));
            flameMat.color = torchLightColor;
            flameMat.SetFloat("_Metallic", 0f);
            flameMat.SetFloat("_Glossiness", 0.5f);
            flameMat.EnableKeyword("_EMISSION");
            flameMat.SetColor("_EmissionColor", torchLightColor * 2f);
            flame.GetComponent<Renderer>().material = flameMat;
        }
        
        // 조명 추가
        Light torchLight = torch.AddComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.color = torchLightColor;
        torchLight.intensity = torchLightIntensity;
        torchLight.range = torchLightRange;
        torchLight.shadows = LightShadows.Soft;
        
        torch.transform.SetParent(transform);
        dungeonObjects.Add(torch);
    }
    
    void CreateSkeleton(Vector3 position)
    {
        GameObject skeleton;
        
        if (skeletonPrefab != null)
        {
            skeleton = Instantiate(skeletonPrefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
        else
        {
            // 간단한 해골 표현
            skeleton = new GameObject("Skeleton");
            skeleton.transform.position = position;
            skeleton.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            // 해골 머리
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(skeleton.transform);
            head.transform.localPosition = new Vector3(0, 0.15f, 0);
            head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            SetDefaultMaterial(head, Color.white);
            
            // 뼈 조각들 (여러 개)
            for (int i = 0; i < 4; i++)
            {
                GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bone.transform.SetParent(skeleton.transform);
                bone.transform.localPosition = new Vector3(
                    Random.Range(-0.3f, 0.3f), 
                    -0.1f, 
                    Random.Range(-0.3f, 0.3f)
                );
                bone.transform.localScale = new Vector3(0.1f, Random.Range(0.2f, 0.4f), 0.1f);
                bone.transform.localRotation = Quaternion.Euler(
                    Random.Range(0, 360), 
                    Random.Range(0, 360), 
                    Random.Range(0, 360)
                );
                SetDefaultMaterial(bone, Color.white);
            }
        }
        
        skeleton.transform.SetParent(transform);
        dungeonObjects.Add(skeleton);
    }
    
    void CreatePottery(Vector3 position)
    {
        GameObject pottery;
        
        if (potteryPrefab != null)
        {
            pottery = Instantiate(potteryPrefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
        }
        else
        {
            // 간단한 도자기 표현
            pottery = new GameObject("Pottery");
            pottery.transform.position = position;
            pottery.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            // 도자기 몸체
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.transform.SetParent(pottery.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.2f, 0.15f, 0.2f);
            SetDefaultMaterial(body, new Color(0.4f, 0.25f, 0.15f));
            
            // 깨진 조각 (일부는)
            if (Random.value < 0.5f)
            {
                for (int i = 0; i < Random.Range(2, 4); i++)
                {
                    GameObject fragment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fragment.transform.SetParent(pottery.transform);
                    fragment.transform.localPosition = new Vector3(
                        Random.Range(-0.15f, 0.15f),
                        -0.1f,
                        Random.Range(-0.15f, 0.15f)
                    );
                    fragment.transform.localScale = new Vector3(0.08f, 0.05f, 0.08f);
                    fragment.transform.localRotation = Quaternion.Euler(
                        Random.Range(0, 360),
                        Random.Range(0, 360),
                        Random.Range(0, 360)
                    );
                    SetDefaultMaterial(fragment, new Color(0.4f, 0.25f, 0.15f));
                }
            }
        }
        
        pottery.transform.SetParent(transform);
        dungeonObjects.Add(pottery);
    }
    
    void SetupLighting()
    {
        // 환경 조명 설정 (더 밝게)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.15f, 0.15f, 0.2f);
        RenderSettings.ambientEquatorColor = new Color(0.1f, 0.1f, 0.15f);
        RenderSettings.ambientGroundColor = new Color(0.05f, 0.05f, 0.1f);
        RenderSettings.ambientIntensity = 0.5f; // 더 밝게
        
        // 안개 효과
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.1f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.015f; // 약간 줄임
    }
    
    void SetDefaultMaterial(GameObject obj, Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Metallic", 0.1f);
        mat.SetFloat("_Glossiness", 0.2f);
        
        // 2D 스타일을 위한 텍스처 설정
        // 메인 텍스처가 있으면 Point 필터 적용
        if (mat.mainTexture != null)
        {
            Texture2D tex = mat.mainTexture as Texture2D;
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point; // 픽셀 아트 스타일
            }
        }
        
        obj.GetComponent<Renderer>().material = mat;
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


