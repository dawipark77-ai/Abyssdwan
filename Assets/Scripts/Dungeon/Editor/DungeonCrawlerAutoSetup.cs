using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class DungeonCrawlerAutoSetup
{
    static DungeonCrawlerAutoSetup()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }
    
    static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
    {
        // DungeonCrawler_01 씬인지 확인
        if (scene.name == "DungeonCrawler_01")
        {
            // 이미 설정되어 있는지 확인
            if (GameObject.Find("DungeonCrawlerGenerator") == null)
            {
                Debug.Log("[DungeonCrawlerAutoSetup] Setting up Dungeon Crawler scene...");
                
                // 던전 생성기 생성
                GameObject generatorObj = new GameObject("DungeonCrawlerGenerator");
                DungeonCrawlerGenerator generator = generatorObj.AddComponent<DungeonCrawlerGenerator>();
                
                // 플레이어 생성
                GameObject playerObj = new GameObject("Player");
                playerObj.transform.position = new Vector3(0, 0, 2);
                
                CharacterController controller = playerObj.AddComponent<CharacterController>();
                controller.height = 2f;
                controller.radius = 0.5f;
                controller.center = new Vector3(0, 1f, 0);
                
                FirstPersonController fpsController = playerObj.AddComponent<FirstPersonController>();
                
                // 카메라 설정
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    mainCamera.transform.SetParent(playerObj.transform);
                    mainCamera.transform.localPosition = new Vector3(0, 1.6f, 0);
                    mainCamera.transform.localRotation = Quaternion.identity;
                }
                
                // 던전 생성
                generator.GenerateDungeon();
                
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log("[DungeonCrawlerAutoSetup] Scene setup complete!");
            }
        }
    }
}








