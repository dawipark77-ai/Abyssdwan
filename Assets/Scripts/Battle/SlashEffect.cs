using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlashEffect : MonoBehaviour
{
    private Image slashImage;
    private RectTransform rectTransform;
    
    void Awake()
    {
        slashImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        // Use Additive shader to make black transparent
        Material additiveMat = new Material(Shader.Find("Mobile/Particles/Additive"));
        slashImage.material = additiveMat;
    }
    
    public void PlaySlashEffect(Vector3 targetPosition)
    {
        StartCoroutine(SlashAnimation(targetPosition));
    }
    
    private IEnumerator SlashAnimation(Vector3 targetPosition)
    {
        // Load the sprite sheet
        Texture2D spriteSheet = Resources.Load<Texture2D>("Effects/StraightSlash");
        
        if (spriteSheet == null)
        {
            Debug.LogError("[SlashEffect] Failed to load StraightSlash sprite sheet!");
            Destroy(gameObject);
            yield break;
        }
        
        // Convert world position to screen position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(targetPosition);
        rectTransform.position = screenPos;
        
        // Set rotation (Identity as the sprite is already diagonal)
        rectTransform.rotation = Quaternion.identity;
        
        // 4 frames arranged horizontally in a single row
        int totalFrames = 4;
        int sheetWidth = spriteSheet.width;
        int sheetHeight = spriteSheet.height;
        int frameWidth = sheetWidth / totalFrames;
        int frameHeight = sheetHeight;
        
        // Animation settings
        float frameDuration = 0.05f; // 50ms per frame
        
        // Play animation frames
        for (int i = 0; i < totalFrames; i++)
        {
            // Create sprite from texture region
            Rect frameRect = new Rect(
                i * frameWidth,
                0,
                frameWidth,
                frameHeight
            );
            
            Sprite frameSprite = Sprite.Create(
                spriteSheet,
                frameRect,
                new Vector2(0.5f, 0.5f),
                100f
            );
            
            slashImage.sprite = frameSprite;
            slashImage.color = Color.white;
            
            // Scale based on frame (grow then shrink slightly)
            float scale = 1f + (i == 2 ? 0.2f : 0f); // Peak at frame 3
            rectTransform.localScale = new Vector3(scale, scale, 1f);
            
            yield return new WaitForSeconds(frameDuration);
        }
        
        // Destroy this effect
        Destroy(gameObject);
    }
}

