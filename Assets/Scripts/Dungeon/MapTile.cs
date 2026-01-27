using UnityEngine;
using UnityEngine.UI;

namespace Genesis01.Dungeon
{
    public class MapTile : MonoBehaviour
    {
        [Header("Wall Images")]
        public Image wallNorth;
        public Image wallSouth;
        public Image wallEast;
        public Image wallWest;

        [Header("Base")]
        public Image floorImage;

        public void Setup(bool north, bool south, bool east, bool west)
        {
            if (wallNorth) wallNorth.gameObject.SetActive(north);
            if (wallSouth) wallSouth.gameObject.SetActive(south);
            if (wallEast) wallEast.gameObject.SetActive(east);
            if (wallWest) wallWest.gameObject.SetActive(west);
        }

        public void SetVisited(bool visited)
        {
            // Simple visualization: change alpha or color
            if (floorImage)
            {
                Color c = floorImage.color;
                c.a = visited ? 1f : 0f; // Invisible if not visited
                floorImage.color = c;
            }

            // Also hide walls if not visited?
            // Depending on design, you might want to show walls but darken them.
            // For now, let's just hide everything if not visited.
            gameObject.SetActive(visited); 
        }
        
        // Optional: Method to set color for customization
        public void SetColor(Color floorColor, Color wallColor)
        {
            if (floorImage) floorImage.color = floorColor;
            if (wallNorth) wallNorth.color = wallColor;
            if (wallSouth) wallSouth.color = wallColor;
            if (wallEast) wallEast.color = wallColor;
            if (wallWest) wallWest.color = wallColor;
        }
    }
}
