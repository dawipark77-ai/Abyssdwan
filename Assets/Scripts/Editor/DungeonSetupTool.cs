using UnityEngine;
using Genesis01.Dungeon;
using UnityEditor;

using UnityEditor.SceneManagement;

public class DungeonSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Dungeon Scene")]
    public static void SetupDungeon()
    {
        // 1. Setup Lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.skybox = null;
        
        // 2. Create Prefabs (for reference, not used by DungeonGenerator)
        GameObject wallPrefab = CreatePrefab("Wall_Prefab", PrimitiveType.Cube, Color.gray);
        GameObject floorPrefab = CreatePrefab("Floor_Prefab", PrimitiveType.Plane, Color.darkGray);
        
        // 3. Create Dungeon Generator
        GameObject genObj = GameObject.Find("DungeonGenerator");
        if (genObj == null)
        {
            genObj = new GameObject("DungeonGenerator");
            DungeonGenerator gen = genObj.AddComponent<DungeonGenerator>();
            // Note: DungeonGenerator creates geometry directly, not from prefabs
            // If you need prefab-based generation, use DungeonMap instead
            gen.cellSize = 2.0f; 
        }

        // 4. Create Player
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj == null)
        {
            playerObj = new GameObject("Player");
            playerObj.transform.position = new Vector3(2, 1, 16); // Start position
            
            // Add Movement
            DungeonPlayerMovement movement = playerObj.AddComponent<DungeonPlayerMovement>();
            movement.dungeonGenerator = genObj.GetComponent<DungeonGenerator>();
            
            // Add Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.transform.parent = playerObj.transform;
            camObj.transform.localPosition = new Vector3(0, 0.6f, 0); // Eye height
            Camera cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.nearClipPlane = 0.1f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            
            // Add Torch
            GameObject torchObj = new GameObject("Torch");
            torchObj.transform.parent = camObj.transform;
            torchObj.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            Light light = torchObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10.0f;
            light.color = new Color(0.3f, 0.8f, 1.0f); // Cyan-ish
            light.intensity = 1.5f;
            light.shadows = LightShadows.Soft;
            
            TorchLight torchScript = torchObj.AddComponent<TorchLight>();
            torchScript.torchLight = light;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Dungeon Scene Setup Complete! Press Play to test.");
    }

    private static GameObject CreatePrefab(string name, PrimitiveType type, Color color)
    {
        // Check if exists in scene first to avoid duplicates during testing
        // But for prefabs, we usually want them in Assets. 
        // For this prototype, let's just create them in the scene and link them.
        // If we wanted to be proper, we'd save them to disk.
        
        // Let's check if we have them in a "Prefabs" folder in the scene or project
        // For simplicity, we create a hidden folder in scene or just use scene objects as "prefabs" (which works for Instantiate)
        
        GameObject existing = GameObject.Find(name);
        if (existing != null) return existing;

        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        
        // Material
        Renderer rend = obj.GetComponent<Renderer>();
        rend.sharedMaterial = new Material(Shader.Find("Standard"));
        rend.sharedMaterial.color = color;
        
        // If Plane, scale it down to be 1x1 (Plane is 10x10 by default)
        if (type == PrimitiveType.Plane)
        {
            obj.transform.localScale = new Vector3(0.2f, 1, 0.2f); // 2x2 size match
        }
        else if (type == PrimitiveType.Cube)
        {
            obj.transform.localScale = new Vector3(2, 2, 2); // 2x2x2
        }

        // Deactivate so it doesn't clutter the view, we just use it as a source
        obj.SetActive(false);
        
        return obj;
    }
}
