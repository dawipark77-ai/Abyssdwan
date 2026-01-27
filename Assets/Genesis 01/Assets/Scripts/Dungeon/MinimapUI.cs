using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 미니맵 UI 관리
/// 미니맵 표시/숨김, 크기 조절 등
/// </summary>
public class MinimapUI : MonoBehaviour
{
    [Header("UI 참조")]
    public GameObject minimapPanel;
    public RectTransform minimapViewport;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Slider zoomSlider;
    
    [Header("설정")]
    [SerializeField] private bool startVisible = true;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2f;
    [SerializeField] private float defaultZoom = 1f;
    
    private bool isVisible = true;
    private Camera minimapCamera;
    
    private void Start()
    {
        isVisible = startVisible;
        UpdateVisibility();
        
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleMinimap);
        }
        
        if (zoomSlider != null)
        {
            zoomSlider.minValue = minZoom;
            zoomSlider.maxValue = maxZoom;
            zoomSlider.value = defaultZoom;
            zoomSlider.onValueChanged.AddListener(OnZoomChanged);
        }
        
        minimapCamera = FindObjectOfType<Camera>();
    }
    
    /// <summary>
    /// 미니맵 표시/숨김 토글
    /// </summary>
    public void ToggleMinimap()
    {
        isVisible = !isVisible;
        UpdateVisibility();
    }
    
    /// <summary>
    /// 미니맵 표시 상태 업데이트
    /// </summary>
    private void UpdateVisibility()
    {
        if (minimapPanel != null)
        {
            minimapPanel.SetActive(isVisible);
        }
    }
    
    /// <summary>
    /// 줌 변경 처리
    /// </summary>
    private void OnZoomChanged(float value)
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = value;
        }
        
        if (minimapViewport != null)
        {
            minimapViewport.localScale = Vector3.one * value;
        }
    }
    
    /// <summary>
    /// 미니맵 표시
    /// </summary>
    public void ShowMinimap()
    {
        isVisible = true;
        UpdateVisibility();
    }
    
    /// <summary>
    /// 미니맵 숨김
    /// </summary>
    public void HideMinimap()
    {
        isVisible = false;
        UpdateVisibility();
    }
}

