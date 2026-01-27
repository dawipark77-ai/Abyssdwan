using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class CreateDungeonCrawler02Scene : EditorWindow
{
    [MenuItem("Tools/Create Dungeon Crawler 02 Scene")]
    static void CreateScene()
    {
        // 새 씬 생성
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // 씬 이름 설정
        string scenePath = "Assets/Genesis 01/Assets/DungeonCrawler_02.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        // 기본 오브젝트 제거 (새 씬에 기본으로 생성되는 것들)
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            DestroyImmediate(mainCamera);
        }
        
        GameObject light = GameObject.Find("Directional Light");
        if (light != null)
        {
            DestroyImmediate(light);
        }
        
        // 씬 설정 오브젝트 생성
        GameObject setupObj = new GameObject("SceneSetup");
        DungeonCrawler02SceneSetup setup = setupObj.AddComponent<DungeonCrawler02SceneSetup>();
        setup.setupOnStart = true;
        setup.playerStartPosition = new Vector3(0, 0, 2);
        
        // 씬 저장
        EditorSceneManager.SaveScene(newScene);
        
        Debug.Log($"Created new scene: {scenePath}");
        EditorUtility.DisplayDialog("Scene Created", $"Dungeon Crawler 02 scene created at:\n{scenePath}", "OK");
    }
}








