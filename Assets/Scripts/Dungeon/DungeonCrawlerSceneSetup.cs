using UnityEngine;

/// <summary>
/// 던전 크롤러 씬을 자동으로 설정하는 스크립트
/// 이 스크립트를 씬에 추가하면 자동으로 던전이 생성되고 플레이어가 배치됩니다.
/// </summary>
public class DungeonCrawlerSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool setupOnStart = true;
    public bool createPlayer = true;
    
    [Header("Player Settings")]
    public Vector3 playerStartPosition = new Vector3(0, 0, 2);
    
    void Awake()
    {
        // Awake에서도 실행 (씬이 로드되자마자)
        if (setupOnStart)
        {
            SetupScene();
        }
    }
    
    void Start()
    {
        // Start에서도 확인 (이중 체크)
        if (setupOnStart && GameObject.Find("Dungeon") == null)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Dungeon Crawler Scene")]
    public void SetupScene()
    {
        // 기존 3D 던전 제거
        GameObject oldDungeon = GameObject.Find("DungeonRoot");
        if (oldDungeon != null)
        {
            DestroyImmediate(oldDungeon);
            Debug.Log("[DungeonCrawlerSceneSetup] Removed old 3D dungeon");
        }
        
        // 기존 3D 던전 생성기 비활성화
        DungeonCrawlerGenerator oldGenerator = FindFirstObjectByType<DungeonCrawlerGenerator>();
        if (oldGenerator != null)
        {
            oldGenerator.enabled = false;
            oldGenerator.gameObject.SetActive(false);
            Debug.Log("[DungeonCrawlerSceneSetup] Disabled 3D dungeon generator");
        }
        
        // 스프라이트 기반 던전 생성
        SpriteBasedDungeon spriteDungeon = FindFirstObjectByType<SpriteBasedDungeon>();
        if (spriteDungeon == null)
        {
            GameObject spriteDungeonObj = new GameObject("SpriteBasedDungeon");
            spriteDungeon = spriteDungeonObj.AddComponent<SpriteBasedDungeon>();
            Debug.Log("[DungeonCrawlerSceneSetup] Created SpriteBasedDungeon");
        }
        
        // 던전 생성
        spriteDungeon.GenerateSpriteDungeon();
        
        // 플레이어 생성
        if (createPlayer)
        {
            CreatePlayer();
        }
        
        // 카메라 설정
        SetupCamera();
        
        // 2D 렌더링 설정 추가
        Setup2DRendering();
        
        Debug.Log("Dungeon Crawler scene setup complete!");
    }
    
    void CreatePlayer()
    {
        // 이미 플레이어가 있으면 스킵
        if (FindFirstObjectByType<FirstPersonController>() != null)
        {
            return;
        }
        
        // 플레이어 오브젝트 생성
        GameObject player = new GameObject("Player");
        player.transform.position = playerStartPosition;
        
        // CharacterController 추가
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0, 1f, 0);
        
        // FirstPersonController 추가
        FirstPersonController fpsController = player.AddComponent<FirstPersonController>();
        
        Debug.Log("Player created at position: " + playerStartPosition);
    }
    
    void SetupCamera()
    {
        // 플레이어 찾기
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        
        if (player != null)
        {
            // 플레이어의 카메라 확인
            Camera playerCam = player.GetComponentInChildren<Camera>();
            if (playerCam != null)
            {
                // 플레이어 카메라를 메인 카메라로 설정
                playerCam.tag = "MainCamera";
                playerCam.enabled = true;
                playerCam.gameObject.SetActive(true);
                Debug.Log("[DungeonCrawlerSceneSetup] Using player's camera");
            }
            else
            {
                // 플레이어에 카메라가 없으면 생성
                GameObject cameraObj = new GameObject("PlayerCamera");
                cameraObj.transform.SetParent(player.transform);
                cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                cameraObj.transform.localRotation = Quaternion.identity;
                
                Camera newCam = cameraObj.AddComponent<Camera>();
                newCam.tag = "MainCamera";
                newCam.clearFlags = CameraClearFlags.SolidColor;
                newCam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
                newCam.fieldOfView = 75f;
                newCam.nearClipPlane = 0.1f;
                newCam.farClipPlane = 50f;
                newCam.enabled = true;
                
                Debug.Log("[DungeonCrawlerSceneSetup] Created camera for player");
            }
            
            // 기존 메인 카메라가 있으면 비활성화 (플레이어 카메라와 충돌 방지)
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.parent != player.transform)
            {
                mainCamera.gameObject.SetActive(false);
                Debug.Log("[DungeonCrawlerSceneSetup] Disabled old main camera");
            }
        }
        else
        {
            // 플레이어가 없으면 기존 카메라 사용
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
            
            if (mainCamera != null)
            {
                mainCamera.tag = "MainCamera";
                mainCamera.enabled = true;
                mainCamera.gameObject.SetActive(true);
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
                Debug.Log("[DungeonCrawlerSceneSetup] Using existing camera");
            }
            else
            {
                Debug.LogError("[DungeonCrawlerSceneSetup] No camera found and no player to attach camera to!");
            }
        }
    }
    
    void Setup2DRendering()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera != null)
        {
            // 2D 렌더러 추가
            if (mainCamera.GetComponent<Dungeon2DRenderer>() == null)
            {
                mainCamera.gameObject.AddComponent<Dungeon2DRenderer>();
                Debug.Log("[DungeonCrawlerSceneSetup] 2D renderer added to camera");
            }
        }
    }
}


