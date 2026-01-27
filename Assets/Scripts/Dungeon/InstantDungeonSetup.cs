using UnityEngine;
using Genesis01.Dungeon;

/// <summary>
/// INSTANT SETUP: Just add this component to ANY object and it will setup everything immediately.
/// Right-click in Hierarchy -> 3D Object -> Cube, then add this component to it.
/// </summary>
[ExecuteInEditMode]
public class InstantDungeonSetup : MonoBehaviour
{
    void OnEnable()
    {
        // Only run once
        if (GameObject.Find("DungeonGenerator") != null)
        {
            Debug.Log("Dungeon already exists.");
            return;
        }

        Debug.Log("=== INSTANT DUNGEON SETUP STARTING ===");
        SetupEverything();
        Debug.Log("=== SETUP COMPLETE! Press PLAY ===");
    }

    void SetupEverything()
    {
        // 1. LIGHTING
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.skybox = null;
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.08f;

        // 2. CREATE WALL PREFAB
        GameObject wallPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallPrefab.name = "Wall_Prefab";
        wallPrefab.transform.localScale = new Vector3(2, 2, 2);
        Material wallMat = new Material(Shader.Find("Standard"));
        wallMat.color = new Color(0.25f, 0.28f, 0.3f);
        wallPrefab.GetComponent<Renderer>().sharedMaterial = wallMat;
        wallPrefab.SetActive(false);

        // 3. CREATE FLOOR PREFAB
        GameObject floorPrefab = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floorPrefab.name = "Floor_Prefab";
        floorPrefab.transform.localScale = new Vector3(0.2f, 1, 0.2f);
        Material floorMat = new Material(Shader.Find("Standard"));
        floorMat.color = new Color(0.15f, 0.17f, 0.2f);
        floorPrefab.GetComponent<Renderer>().sharedMaterial = floorMat;
        floorPrefab.SetActive(false);

        // 4. CREATE DUNGEON GENERATOR
        GameObject genObj = new GameObject("DungeonGenerator");
        DungeonGenerator gen = genObj.AddComponent<DungeonGenerator>();
        // Note: DungeonGenerator creates geometry directly, not from prefabs
        // If you need prefab-based generation, use DungeonMap instead

        // 5. CREATE PLAYER
        GameObject playerObj = new GameObject("Player");
        playerObj.transform.position = new Vector3(2, 1, 16);
        
        DungeonPlayerMovement movement = playerObj.AddComponent<DungeonPlayerMovement>();
        movement.dungeonGenerator = gen;

        // 6. CREATE CAMERA
        GameObject camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        camObj.transform.parent = playerObj.transform;
        camObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 50f;
        cam.fieldOfView = 60f;

        // 7. CREATE TORCH LIGHT
        GameObject torchObj = new GameObject("Torch");
        torchObj.transform.parent = camObj.transform;
        torchObj.transform.localPosition = new Vector3(0.3f, -0.3f, 0.5f);
        
        Light torchLight = torchObj.AddComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.color = new Color(0.4f, 0.85f, 1.0f);
        torchLight.intensity = 2.0f;
        torchLight.range = 12f;
        torchLight.shadows = LightShadows.Soft;
        
        TorchLight flicker = torchObj.AddComponent<TorchLight>();
        flicker.torchLight = torchLight;
        flicker.minIntensity = 1.5f;
        flicker.maxIntensity = 2.5f;

        Debug.Log("?Lighting configured");
        Debug.Log("?Prefabs created");
        Debug.Log("?Generator ready");
        Debug.Log("?Player & Camera ready");
        Debug.Log("?Torch ready");
    }
}
