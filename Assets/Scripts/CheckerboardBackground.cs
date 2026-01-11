using UnityEngine;

/// <summary>
/// Generates a procedural checkerboard background texture
/// Attach this to a Quad or Sprite to create a checkerboard pattern
/// Updates in real-time when properties are changed in the Inspector (even in Edit mode!)
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class CheckerboardBackground : MonoBehaviour
{
    [Header("Checkerboard Settings")]
    [SerializeField] private int textureSize = 512;
    [SerializeField] private int tileSize = 32;
    [SerializeField] private Color color1 = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray
    [SerializeField] private Color color2 = new Color(0.7f, 0.7f, 0.7f, 1f); // Darker gray
    
    [Header("Size Settings")]
    [SerializeField] private float worldWidth = 20f;
    [SerializeField] private float worldHeight = 15f;
    
    [Header("Editor Settings")]
    [Tooltip("Automatically update when properties change in Inspector")]
    [SerializeField] private bool autoUpdateInEditor = true;
    
    private SpriteRenderer spriteRenderer;
    
    // Track previous values to detect changes
    private int lastTextureSize;
    private int lastTileSize;
    private Color lastColor1;
    private Color lastColor2;
    private float lastWorldWidth;
    private float lastWorldHeight;
    
    // Public properties to expose bounds
    public float WorldWidth => worldWidth;
    public float WorldHeight => worldHeight;
    
    /// <summary>
    /// Gets the world-space bounds of the checkerboard background.
    /// </summary>
    public Bounds GetWorldBounds()
    {
        Vector3 center = transform.position;
        Vector3 size = new Vector3(worldWidth, worldHeight, 0);
        return new Bounds(center, size);
    }
    
    /// <summary>
    /// Gets the min/max coordinates of the background.
    /// </summary>
    public void GetBoundaryCoordinates(out float minX, out float maxX, out float minY, out float maxY)
    {
        Vector3 pos = transform.position;
        float halfWidth = worldWidth / 2f;
        float halfHeight = worldHeight / 2f;
        
        minX = pos.x - halfWidth;
        maxX = pos.x + halfWidth;
        minY = pos.y - halfHeight;
        maxY = pos.y + halfHeight;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GenerateCheckerboard();
        CacheCurrentValues();
    }
    
    private void OnEnable()
    {
        // Regenerate when enabled (covers both Play mode and Edit mode)
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        GenerateCheckerboard();
        CacheCurrentValues();
    }

    /// <summary>
    /// Generates the checkerboard texture and applies it to the sprite renderer
    /// </summary>
    [ContextMenu("Generate Checkerboard")]
    public void GenerateCheckerboard()
    {
        // Create texture
        Texture2D texture = new Texture2D(textureSize, textureSize);
        texture.filterMode = FilterMode.Point; // Crisp pixels for pixel art style
        texture.wrapMode = TextureWrapMode.Repeat;
        
        // Fill with checkerboard pattern
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // Determine which tile we're in
                int tileX = x / tileSize;
                int tileY = y / tileSize;
                
                // Alternate colors based on tile position
                bool isEvenTile = (tileX + tileY) % 2 == 0;
                texture.SetPixel(x, y, isEvenTile ? color1 : color2);
            }
        }
        
        texture.Apply();
        
        // Create sprite from texture
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            textureSize / worldWidth // Pixels per unit to match world size
        );
        
        // Apply to sprite renderer
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = sprite;
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.size = new Vector2(worldWidth, worldHeight);
        
        // Set sorting order to be behind everything
        spriteRenderer.sortingOrder = -100;
    }

    /// <summary>
    /// Update checkerboard when values change in editor (works in Edit mode too!)
    /// </summary>
    private void OnValidate()
    {
        if (!autoUpdateInEditor) return;
        
        // Use delayed call to avoid issues with OnValidate timing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += OnValidateDelayed;
        #endif
    }
    
    #if UNITY_EDITOR
    private void OnValidateDelayed()
    {
        // Check if object still exists (might have been destroyed)
        if (this == null) return;
        
        // Check if values actually changed
        if (!HasValuesChanged()) return;
        
        // Get sprite renderer if needed
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null) return;
        
        // Regenerate the checkerboard
        GenerateCheckerboard();
        CacheCurrentValues();
        
        // Mark scene as dirty so changes are saved
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(spriteRenderer);
        }
    }
    #endif
    
    /// <summary>
    /// Checks if any property values have changed since last generation.
    /// </summary>
    private bool HasValuesChanged()
    {
        return textureSize != lastTextureSize ||
               tileSize != lastTileSize ||
               color1 != lastColor1 ||
               color2 != lastColor2 ||
               !Mathf.Approximately(worldWidth, lastWorldWidth) ||
               !Mathf.Approximately(worldHeight, lastWorldHeight);
    }
    
    /// <summary>
    /// Caches current values to detect future changes.
    /// </summary>
    private void CacheCurrentValues()
    {
        lastTextureSize = textureSize;
        lastTileSize = tileSize;
        lastColor1 = color1;
        lastColor2 = color2;
        lastWorldWidth = worldWidth;
        lastWorldHeight = worldHeight;
    }
    
    /// <summary>
    /// Force regenerate the checkerboard (useful from other scripts or context menu)
    /// </summary>
    [ContextMenu("Force Regenerate")]
    public void ForceRegenerate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        GenerateCheckerboard();
        CacheCurrentValues();
        
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(spriteRenderer);
        }
        #endif
    }
}
