using UnityEngine;

/// <summary>
/// DungeonCrawler_02 씬 자동 설정
/// 3D 공간에 2D 스타일로 렌더링되는 던전을 생성합니다.
/// </summary>
public class DungeonCrawler02SceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool setupOnStart = true;
    
    [Header("Player Settings")]
    public Vector3 playerStartPosition = new Vector3(0, 1f, 2); // Y=1f (CharacterController center)
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Dungeon Crawler 02 Scene")]
    public void SetupScene()
    {
        Debug.Log("[DungeonCrawler02SceneSetup] Setting up scene...");
        
        // 기존 던전 제거
        GameObject oldDungeon = GameObject.Find("Dungeon02_Root");
        if (oldDungeon != null)
        {
            DestroyImmediate(oldDungeon);
        }
        
        // 던전 생성기 찾기 또는 생성
        DungeonCrawler02Generator generator = FindFirstObjectByType<DungeonCrawler02Generator>();
        if (generator == null)
        {
            GameObject generatorObj = new GameObject("DungeonCrawler02Generator");
            generator = generatorObj.AddComponent<DungeonCrawler02Generator>();
        }
        
        // 던전 생성
        generator.GenerateDungeon();
        
        // 플레이어 생성
        CreatePlayer();
        
        // 카메라 설정
        SetupCamera();
        
        Debug.Log("[DungeonCrawler02SceneSetup] Scene setup complete!");
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
        // 플레이어 위치: 바닥 위 (CharacterController center가 1f이므로 Y=1f)
        player.transform.position = playerStartPosition;
        
        // CharacterController 추가
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.5f;
        controller.center = new Vector3(0, 1f, 0); // center가 1f이므로 발은 0f
        
        // FirstPersonController 추가
        FirstPersonController fpsController = player.AddComponent<FirstPersonController>();
        
        Debug.Log("[DungeonCrawler02SceneSetup] Player created");
    }
    
    void SetupCamera()
    {
        // 플레이어 찾기
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        
        if (player != null)
        {
            // 플레이어의 카메라 확인
            Camera playerCam = player.GetComponentInChildren<Camera>();
            if (playerCam == null)
            {
                // 플레이어에 카메라 생성
                GameObject cameraObj = new GameObject("PlayerCamera");
                cameraObj.transform.SetParent(player.transform);
                cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
                cameraObj.transform.localRotation = Quaternion.identity;
                
                playerCam = cameraObj.AddComponent<Camera>();
                playerCam.tag = "MainCamera";
                playerCam.clearFlags = CameraClearFlags.SolidColor;
                playerCam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
                playerCam.fieldOfView = 75f;
                playerCam.nearClipPlane = 0.1f;
                playerCam.farClipPlane = 50f;
                playerCam.enabled = true;
                
                Debug.Log("[DungeonCrawler02SceneSetup] Created player camera");
            }
            else
            {
                playerCam.tag = "MainCamera";
                playerCam.enabled = true;
                playerCam.gameObject.SetActive(true);
            }
            
            // 기존 메인 카메라 비활성화
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.parent != player.transform)
            {
                mainCamera.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("[DungeonCrawler02SceneSetup] Player not found");
        }
    }
}

