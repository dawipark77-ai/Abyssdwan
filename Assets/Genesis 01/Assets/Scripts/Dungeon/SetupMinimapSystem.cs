using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 미니맵 시스템 자동 설정 스크립트
/// 씬에 필요한 오브젝트들을 자동으로 생성합니다
/// </summary>
public class SetupMinimapSystem : MonoBehaviour
{
    [ContextMenu("Setup Minimap System")]
    public void SetupSystem()
    {
        // 1. DungeonMinimap 오브젝트 찾기 또는 생성
        DungeonMinimap minimap = FindObjectOfType<DungeonMinimap>();
        if (minimap == null)
        {
            GameObject minimapObj = new GameObject("DungeonMinimap");
            minimap = minimapObj.AddComponent<DungeonMinimap>();
        }
        
        // 2. DungeonGenerator 생성
        DungeonGenerator generator = minimap.GetComponent<DungeonGenerator>();
        if (generator == null)
        {
            generator = minimap.gameObject.AddComponent<DungeonGenerator>();
        }
        
        // 3. DungeonEventManager 생성
        DungeonEventManager eventManager = FindObjectOfType<DungeonEventManager>();
        if (eventManager == null)
        {
            GameObject eventManagerObj = new GameObject("DungeonEventManager");
            eventManager = eventManagerObj.AddComponent<DungeonEventManager>();
        }
        
        // 4. MinimapPlayerController 생성
        MinimapPlayerController playerController = FindObjectOfType<MinimapPlayerController>();
        if (playerController == null)
        {
            GameObject playerObj = new GameObject("MinimapPlayerController");
            playerController = playerObj.AddComponent<MinimapPlayerController>();
        }
        
        // 5. 미니맵 카메라 생성
        Camera minimapCamera = GameObject.Find("MinimapCamera")?.GetComponent<Camera>();
        if (minimapCamera == null)
        {
            GameObject cameraObj = new GameObject("MinimapCamera");
            minimapCamera = cameraObj.AddComponent<Camera>();
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = 5f;
            minimapCamera.cullingMask = 1 << LayerMask.NameToLayer("Minimap");
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            minimapCamera.depth = 1;
        }
        
        // 6. 배경 오브젝트 생성
        SpriteRenderer background = GameObject.Find("DungeonBackground")?.GetComponent<SpriteRenderer>();
        if (background == null)
        {
            GameObject bgObj = new GameObject("DungeonBackground");
            background = bgObj.AddComponent<SpriteRenderer>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            bgObj.transform.localScale = new Vector3(20, 20, 1);
        }
        
        // 7. UI Canvas 생성
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // 8. 미니맵 UI 패널 생성
        GameObject minimapPanel = GameObject.Find("MinimapPanel");
        if (minimapPanel == null)
        {
            minimapPanel = new GameObject("MinimapPanel");
            minimapPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRect = minimapPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0, 0);
            panelRect.pivot = new Vector2(0, 0);
            panelRect.anchoredPosition = new Vector2(10, 10);
            panelRect.sizeDelta = new Vector2(200, 200);
            
            Image panelImage = minimapPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            
            // 미니맵 뷰포트 생성
            GameObject viewport = new GameObject("MinimapViewport");
            viewport.transform.SetParent(minimapPanel.transform, false);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;
            
            Mask mask = viewport.AddComponent<Mask>();
            Image maskImage = viewport.AddComponent<Image>();
            maskImage.color = new Color(1, 1, 1, 0.1f);
            
            // 미니맵 컨텐츠 생성
            GameObject content = new GameObject("MinimapContent");
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(1000, 1000);
            
            // MinimapUI 스크립트 추가
            MinimapUI minimapUI = minimapPanel.AddComponent<MinimapUI>();
            minimapUI.minimapPanel = minimapPanel;
            minimapUI.minimapViewport = viewportRect;
        }
        
        // 9. DungeonMinimap 필드 자동 설정
        if (minimapPanel != null)
        {
            RectTransform viewportRect = minimapPanel.transform.Find("MinimapViewport")?.GetComponent<RectTransform>();
            RectTransform contentRect = minimapPanel.transform.Find("MinimapViewport/MinimapContent")?.GetComponent<RectTransform>();
            
            minimap.minimapCamera = minimapCamera;
            minimap.minimapViewport = viewportRect;
            minimap.minimapContent = contentRect;
            minimap.backgroundRenderer = background;
            minimap.dungeonGenerator = generator;
            minimap.eventManager = eventManager;
            
            // 프리팹은 Inspector에서 수동으로 할당해야 합니다
            Debug.Log("프리팹 할당 필요: PlayerIcon.prefab과 RoomIcon.prefab을 DungeonMinimap의 Inspector에서 할당해주세요.");
        }
        
        // 10. 프리팹 경고
        Debug.Log("미니맵 시스템 설정이 완료되었습니다!");
        Debug.Log("Inspector에서 DungeonMinimap 오브젝트의 다음 필드들을 설정해주세요:");
        Debug.Log("- Player Icon Prefab: Assets/Scripts/Dungeon/Prefabs/PlayerIcon.prefab");
        Debug.Log("- Room Icon Prefab: Assets/Scripts/Dungeon/Prefabs/RoomIcon.prefab");
        Debug.Log("- Dungeon Background: 원하는 배경 스프라이트");
    }
}

