using UnityEngine;

/// <summary>
/// Abyssdawn_Dungeon_2D 01 씬 자동 설정
/// PNG 이미지를 배경으로 사용하는 던전 씬을 설정합니다.
/// </summary>
public class AbyssdawnDungeonSetup : MonoBehaviour
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
    
    [ContextMenu("Setup Abyssdawn Dungeon Scene")]
    public void SetupScene()
    {
        Debug.Log("[AbyssdawnDungeonSetup] Setting up scene...");
        
        // 배경 설정 오브젝트 찾기 또는 생성
        DungeonBackground2D background = FindFirstObjectByType<DungeonBackground2D>();
        if (background == null)
        {
            GameObject bgObj = new GameObject("DungeonBackground");
            background = bgObj.AddComponent<DungeonBackground2D>();
            Debug.Log("[AbyssdawnDungeonSetup] Created DungeonBackground component");
        }
        
        // 배경이 설정되어 있으면 생성
        if (background.backgroundSprite != null)
        {
            background.CreateBackground();
        }
        else
        {
            Debug.LogWarning("[AbyssdawnDungeonSetup] Background sprite not assigned! Please drag PNG image to DungeonBackground component.");
        }
        
        // 플레이어 생성
        CreatePlayer();
        
        // 카메라 설정
        SetupCamera();
        
        Debug.Log("[AbyssdawnDungeonSetup] Scene setup complete!");
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
        
        Debug.Log("[AbyssdawnDungeonSetup] Player created");
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
                
                Debug.Log("[AbyssdawnDungeonSetup] Created player camera");
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
            Debug.LogWarning("[AbyssdawnDungeonSetup] Player not found");
        }
    }
}

