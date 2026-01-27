using UnityEngine;

/// <summary>
/// 던전 씬을 자동으로 설정하는 스크립트
/// 이 스크립트를 씬에 추가하면 자동으로 던전이 생성됩니다.
/// </summary>
public class DungeonSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool setupOnStart = true;
    public bool createPlayer = true;
    
    [Header("Player Settings")]
    public Vector3 playerStartPosition = new Vector3(0, 0, 2);
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Dungeon Scene")]
    public void SetupScene()
    {
        // 던전 생성기 찾기 또는 생성
        DungeonGenerator generator = FindObjectOfType<DungeonGenerator>();
        if (generator == null)
        {
            GameObject generatorObj = new GameObject("DungeonGenerator");
            generator = generatorObj.AddComponent<DungeonGenerator>();
        }
        
        // 던전 생성
        generator.GenerateDungeon();
        
        // 플레이어 생성
        if (createPlayer)
        {
            CreatePlayer();
        }
        
        // 카메라 설정
        SetupCamera();
        
        Debug.Log("Dungeon scene setup complete!");
    }
    
    void CreatePlayer()
    {
        // 이미 플레이어가 있으면 스킵
        if (FindObjectOfType<FirstPersonController>() != null)
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
        
        // 플레이어 콜라이더 (캡슐 모양)
        // CharacterController가 이미 콜라이더 역할을 함
        
        Debug.Log("Player created at position: " + playerStartPosition);
    }
    
    void SetupCamera()
    {
        // 메인 카메라 찾기
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null)
        {
            // 카메라 설정
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            
            // 플레이어에 카메라가 없으면 메인 카메라를 플레이어에 연결
            FirstPersonController player = FindObjectOfType<FirstPersonController>();
            if (player != null && player.GetComponentInChildren<Camera>() == null)
            {
                mainCamera.transform.SetParent(player.transform);
                mainCamera.transform.localPosition = new Vector3(0, 1.6f, 0);
            }
        }
    }
}


