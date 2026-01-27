using UnityEngine;
using UnityEngine.UI;

public class DungeonMenuController : MonoBehaviour
{
    [Header("Panels")]
    // The panel to open when "Status" is clicked
    public GameObject statusPanel;
    
    // Add other panels here later (Item, Equip, etc.)
    // public GameObject itemPanel; 
    // public GameObject equipPanel;

    [Header("Buttons")]
    public Button statusButton;

    void Start()
    {
        // Auto-link button click if assigned
        if (statusButton != null)
        {
            statusButton.onClick.AddListener(OpenStatus);
        }
    }

    public void OpenStatus()
    {
        // Close other panels if needed later
        // CloseAllPanels();

        if (statusPanel != null)
        {
            bool isActive = statusPanel.activeSelf;
            statusPanel.SetActive(!isActive); // Toggle functionality
        }
    }

    // Helper to close everything (for later expansion)
    public void CloseAllPanels()
    {
        if (statusPanel != null) statusPanel.SetActive(false);
    }
}
