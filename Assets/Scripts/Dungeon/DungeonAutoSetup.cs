using UnityEngine;
using Genesis01.Dungeon;


/// <summary>
/// This script automatically sets up the dungeon scene when it runs.
/// Attach this to an empty GameObject in the scene.
/// </summary>
public class DungeonAutoSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;
    
    void Awake()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }

    void SetupScene()
    {
        // Check if already setup
        if (GameObject.Find("DungeonGenerator") != null)
        {
            Debug.Log("Scene already setup.");
            return;
        }

        Debug.Log("Auto-setting up dungeon scene...");

        // 1. Setup Lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.skybox = null;
        
        // 2. Create Prefabs (for reference, not used by DungeonGenerator)
        GameObject wallPrefab = CreatePrefab("Wall_Prefab", PrimitiveType.Cube, new Color(0.3f, 0.3f, 0.35f));
        GameObject floorPrefab = CreatePrefab("Floor_Prefab", PrimitiveType.Plane, new Color(0.2f, 0.2f, 0.25f));
        
        // 3. Create Dungeon Generator
        GameObject genObj = new GameObject("DungeonGenerator");
        DungeonGenerator gen = genObj.AddComponent<DungeonGenerator>();
        // Note: DungeonGenerator creates geometry directly, not from prefabs
        // If you need prefab-based generation, use DungeonMap instead
        gen.cellSize = 2.0f;
        
        // 4. Create Player
        GameObject playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(2, 1, 16);
        
        DungeonPlayerMovement movement = playerObj.AddComponent<DungeonPlayerMovement>();
        movement.dungeonGenerator = gen;
        
        // 5. Create Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.transform.parent = playerObj.transform;
        camObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.nearClipPlane = 0.1f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        
        // 6. Create Torch
        GameObject torchObj = new GameObject("Torch");
        torchObj.transform.parent = camObj.transform;
        torchObj.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
        Light light = torchObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 10.0f;
        light.color = new Color(0.3f, 0.8f, 1.0f);
        light.intensity = 1.5f;
        light.shadows = LightShadows.Soft;
        
        TorchLight torchScript = torchObj.AddComponent<TorchLight>();
        torchScript.torchLight = light;

        Debug.Log("Dungeon setup complete!");
        
        // Destroy self after setup
        Destroy(gameObject);
    }

    GameObject CreatePrefab(string name, PrimitiveType type, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        
        Renderer rend = obj.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        rend.sharedMaterial = mat;
        
        if (type == PrimitiveType.Plane)
        {
            obj.transform.localScale = new Vector3(0.2f, 1, 0.2f);
        }
        else if (type == PrimitiveType.Cube)
        {
            obj.transform.localScale = new Vector3(2, 2, 2);
        }

        obj.SetActive(false);
        return obj;
    }
}
