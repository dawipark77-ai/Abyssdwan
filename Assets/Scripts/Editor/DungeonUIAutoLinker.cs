using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.Events;
using Genesis01.Dungeon; // Added namespace

public class DungeonUIAutoLinker : EditorWindow
{
    [MenuItem("Tools/Genesis/Auto Link Dungeon UI")]
    public static void LinkUI()
    {
        DungeonPlayerMovement player = FindFirstObjectByType<DungeonPlayerMovement>();
        if (player == null)
        {
            Debug.LogWarning("Could not find 'DungeonPlayerMovement' script in the scene! This tool requires DungeonPlayerMovement to link UI buttons.");
            Debug.LogWarning("If you're using a different player movement script, you may need to update this tool or manually link the buttons.");
            return;
        }

        // Define button names to look for and the method to link
        LinkButton("Forward", player, "OnClickForward");
        LinkButton("Back", player, "OnClickBackward"); // Matches user's "Back" button
        LinkButton("Backward", player, "OnClickBackward");
        LinkButton("Turn Left", player, "OnClickTurnLeft");
        LinkButton("Turn Right", player, "OnClickTurnRight");
        
        // Also try simple arrow names
        LinkButton("Up", player, "OnClickForward");
        LinkButton("Down", player, "OnClickBackward");
        LinkButton("Left", player, "OnClickTurnLeft");
        LinkButton("Right", player, "OnClickTurnRight");

        // Strafe Buttons
        LinkButton("L Side", player, "OnClickStrafeLeft");
        LinkButton("R Side", player, "OnClickStrafeRight");
        LinkButton("Strafe Left", player, "OnClickStrafeLeft");
        LinkButton("Strafe Right", player, "OnClickStrafeRight");

        Debug.Log("UI Linking Complete! Check the Console for details.");
    }

    private static void LinkButton(string buttonName, DungeonPlayerMovement target, string methodName)
    {
        // Find button by name (including inactive ones if possible, but usually active)
        GameObject btnObj = GameObject.Find(buttonName);
        
        // If not found, try finding it inside a Canvas
        if (btnObj == null)
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var b in allButtons)
            {
                if (b.name.Equals(buttonName, System.StringComparison.OrdinalIgnoreCase))
                {
                    btnObj = b.gameObject;
                    break;
                }
            }
        }

        if (btnObj != null)
        {
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                // Create the action delegate
                UnityEngine.Events.UnityAction action = System.Delegate.CreateDelegate(
                    typeof(UnityEngine.Events.UnityAction), 
                    target, 
                    methodName
                ) as UnityEngine.Events.UnityAction;

                // Add the listener editor-side (Persistent)
                UnityEventTools.AddPersistentListener(btn.onClick, action);
                
                // Mark as dirty to save changes
                EditorUtility.SetDirty(btn);
                Debug.Log($"Linked '{buttonName}' to {methodName}");
            }
        }
    }
}
