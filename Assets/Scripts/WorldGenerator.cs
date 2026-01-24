using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Generates a procedural world using Unity's Tilemap system
/// Attach this to a GameObject with a Tilemap component to create a world of specified size
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Tilemap))]
public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] private float worldWidth = 20f;
    [SerializeField] private float worldHeight = 15f;
    
    [Header("Tile Settings")]
    [SerializeField] private RuleTile ruleTile;
    [SerializeField] private Tile tilePrefab;
    
    [Header("Editor Settings")]
    [Tooltip("Automatically update when properties change in Inspector")]
    [SerializeField] private bool autoUpdateInEditor = true;
    
    private Tilemap tilemap;
    
    // Track previous values to detect changes
    private float lastWorldWidth;
    private float lastWorldHeight;
    
    // Public properties to expose bounds
    public float WorldWidth => worldWidth;
    public float WorldHeight => worldHeight;
    
    /// <summary>
    /// Gets the world-space bounds of the tilemap.
    /// </summary>
    public Bounds GetWorldBounds()
    {
        Vector3 center = transform.position;
        Vector3 size = new Vector3(worldWidth, worldHeight, 0);
        return new Bounds(center, size);
    }
    
    /// <summary>
    /// Gets the min/max coordinates of the world.
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
        tilemap = GetComponent<Tilemap>();
        GenerateWorld();
        CacheCurrentValues();
    }
    
    private void OnEnable()
    {
        // Regenerate when enabled (covers both Play mode and Edit mode)
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();
        
        GenerateWorld();
        CacheCurrentValues();
    }

    /// <summary>
    /// Generates the tilemap world based on worldWidth and worldHeight
    /// </summary>
    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();
        
        if (tilemap == null) return;
        
        if (ruleTile == null)
        {
            Debug.LogWarning("[WorldGenerator] No RuleTile assigned. Please assign a RuleTile in the inspector.");
            return;
        }
        
        // Clear existing tiles
        tilemap.ClearAllTiles();
        
        // Calculate grid dimensions based on world size
        int gridWidth = Mathf.RoundToInt(worldWidth);
        int gridHeight = Mathf.RoundToInt(worldHeight);
        
        // Calculate starting position (centered)
        int startX = -gridWidth / 2;
        int startY = -gridHeight / 2;
        
        // Fill the tilemap with the rule tile
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int tilePos = new Vector3Int(startX + x, startY + y, 0);
                tilemap.SetTile(tilePos, ruleTile);
            }
        }
        
        Debug.Log($"[WorldGenerator] Filled world with {gridWidth}x{gridHeight} tiles using RuleTile: {ruleTile.name}");
    }

    /// <summary>
    /// Update world when values change in editor (works in Edit mode too!)
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
        
        // Get tilemap if needed
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();
        
        if (tilemap == null) return;
        
        // Regenerate the world
        GenerateWorld();
        CacheCurrentValues();
        
        // Mark scene as dirty so changes are saved
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(tilemap);
        }
    }
    #endif
    
    /// <summary>
    /// Checks if any property values have changed since last generation.
    /// </summary>
    private bool HasValuesChanged()
    {
        return !Mathf.Approximately(worldWidth, lastWorldWidth) ||
               !Mathf.Approximately(worldHeight, lastWorldHeight);
    }
    
    /// <summary>
    /// Caches current values to detect future changes.
    /// </summary>
    private void CacheCurrentValues()
    {
        lastWorldWidth = worldWidth;
        lastWorldHeight = worldHeight;
    }
    
    /// <summary>
    /// Force regenerate the world (useful from other scripts or context menu)
    /// </summary>
    [ContextMenu("Force Regenerate")]
    public void ForceRegenerate()
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();
        
        GenerateWorld();
        CacheCurrentValues();
        
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(tilemap);
        }
        #endif
    }
}
