using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DungeonInputBinder : MonoBehaviour
{
    // Auto-binds UI buttons to Player controls
    private DungeonGridPlayer player;

    void Start()
    {
        player = FindFirstObjectByType<DungeonGridPlayer>();
        if (player == null)
        {
            Debug.LogError("DungeonInputBinder: No Player found!");
            return;
        }

        BindButton("Forward", player.MoveNorth); // "Up"
        BindButton("Back", player.MoveSouth);    // "Down"
        BindButton("Left", player.MoveWest);     // "Left"
        BindButton("Right", player.MoveEast);    // "Right"
        BindButton("L Side", player.MoveWest);   // "Left Strafe" -> Just Left
        BindButton("R Side", player.MoveEast);   // "Right Strafe" -> Just Right
        
        // Also try standard names just in case
        BindButton("Btn_Forward", player.MoveNorth);
        BindButton("Btn_Left", player.MoveWest);
        BindButton("Btn_Right", player.MoveEast);
        
        Debug.Log("DungeonInputBinder: Binding Complete.");
    }

    private void BindButton(string nameToken, UnityEngine.Events.UnityAction action)
    {
        // Find button by exact name or checking text child
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var btn in allButtons)
        {
            // Case 1: GameObject name contains token
            if (btn.name.IndexOf(nameToken, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                btn.onClick.RemoveAllListeners(); // Clear old refs
                btn.onClick.AddListener(action);
                Debug.Log($"Bound '{nameToken}' to button '{btn.name}'");
                continue;
            }

            // Case 2: Text child contains token
            var text = btn.GetComponentInChildren<Text>();
            if (text != null && text.text.IndexOf(nameToken, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
                Debug.Log($"Bound '{nameToken}' to button '{btn.name}' (via Text)");
            }
            // TMP support if needed, but standard Text is likely used in prototypes
        }
    }
}
