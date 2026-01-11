using UnityEngine;

/// <summary>
/// Automatically syncs camera bounds with a CheckerboardBackground or any SpriteRenderer.
/// Attach this to the Main Camera alongside CameraFollow.
/// </summary>
[RequireComponent(typeof(CameraFollow))]
public class CameraBoundsFromBackground : MonoBehaviour
{
    [Header("Background Reference")]
    [Tooltip("The CheckerboardBackground to sync bounds from. If not set, will auto-find.")]
    [SerializeField] private CheckerboardBackground checkerboardBackground;
    
    [Tooltip("Alternative: Use any SpriteRenderer as the boundary source")]
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
    
    [Header("Sync Settings")]
    [Tooltip("Automatically sync bounds on Start")]
    [SerializeField] private bool syncOnStart = true;
    
    [Tooltip("Continuously sync bounds every frame (useful if background changes at runtime)")]
    [SerializeField] private bool continuousSync = false;
    
    [Tooltip("Add padding/margin inside the boundaries (shrinks the playable area)")]
    [SerializeField] private float boundaryPadding = 0f;
    
    [Header("Debug")]
    [Tooltip("Log sync operations to console")]
    [SerializeField] private bool debugLog = true;
    
    // Cached reference
    private CameraFollow cameraFollow;
    
    private void Awake()
    {
        cameraFollow = GetComponent<CameraFollow>();
    }
    
    private void Start()
    {
        // Auto-find background if not assigned
        if (checkerboardBackground == null && backgroundSpriteRenderer == null)
        {
            FindBackground();
        }
        
        // Sync on start if enabled
        if (syncOnStart)
        {
            SyncBounds();
        }
    }
    
    private void Update()
    {
        // Continuous sync if enabled
        if (continuousSync)
        {
            SyncBounds();
        }
    }
    
    /// <summary>
    /// Attempts to find a CheckerboardBackground or Background SpriteRenderer in the scene.
    /// </summary>
    private void FindBackground()
    {
        // First, try to find CheckerboardBackground
        checkerboardBackground = FindFirstObjectByType<CheckerboardBackground>();
        
        if (checkerboardBackground != null)
        {
            if (debugLog)
                Debug.Log($"[CameraBoundsFromBackground] Auto-found CheckerboardBackground: {checkerboardBackground.gameObject.name}");
            return;
        }
        
        // Try to find by name
        GameObject bgObject = GameObject.Find("Background");
        if (bgObject != null)
        {
            checkerboardBackground = bgObject.GetComponent<CheckerboardBackground>();
            if (checkerboardBackground != null)
            {
                if (debugLog)
                    Debug.Log("[CameraBoundsFromBackground] Found CheckerboardBackground on 'Background' object");
                return;
            }
            
            // Fall back to SpriteRenderer
            backgroundSpriteRenderer = bgObject.GetComponent<SpriteRenderer>();
            if (backgroundSpriteRenderer != null)
            {
                if (debugLog)
                    Debug.Log("[CameraBoundsFromBackground] Found SpriteRenderer on 'Background' object");
                return;
            }
        }
        
        // Last resort: find any large SpriteRenderer (likely the background)
        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        float largestArea = 0f;
        
        foreach (var renderer in allRenderers)
        {
            if (renderer.sprite == null) continue;
            
            float area = renderer.bounds.size.x * renderer.bounds.size.y;
            if (area > largestArea)
            {
                largestArea = area;
                backgroundSpriteRenderer = renderer;
            }
        }
        
        if (backgroundSpriteRenderer != null && debugLog)
        {
            Debug.Log($"[CameraBoundsFromBackground] Auto-found largest SpriteRenderer as background: {backgroundSpriteRenderer.gameObject.name}");
        }
        
        if (checkerboardBackground == null && backgroundSpriteRenderer == null)
        {
            Debug.LogWarning("[CameraBoundsFromBackground] Could not find any background! Please assign one manually.");
        }
    }
    
    /// <summary>
    /// Syncs the camera bounds from the background.
    /// Call this manually if you change the background at runtime.
    /// </summary>
    [ContextMenu("Sync Bounds Now")]
    public void SyncBounds()
    {
        if (cameraFollow == null)
        {
            cameraFollow = GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                Debug.LogError("[CameraBoundsFromBackground] No CameraFollow component found on this camera!");
                return;
            }
        }
        
        float minX, maxX, minY, maxY;
        
        // Get bounds from CheckerboardBackground (preferred)
        if (checkerboardBackground != null)
        {
            checkerboardBackground.GetBoundaryCoordinates(out minX, out maxX, out minY, out maxY);
            
            if (debugLog && !continuousSync)
            {
                Debug.Log($"[CameraBoundsFromBackground] Synced from CheckerboardBackground: " +
                         $"X({minX:F1} to {maxX:F1}), Y({minY:F1} to {maxY:F1})");
            }
        }
        // Fall back to SpriteRenderer bounds
        else if (backgroundSpriteRenderer != null)
        {
            Bounds bounds = backgroundSpriteRenderer.bounds;
            minX = bounds.min.x;
            maxX = bounds.max.x;
            minY = bounds.min.y;
            maxY = bounds.max.y;
            
            if (debugLog && !continuousSync)
            {
                Debug.Log($"[CameraBoundsFromBackground] Synced from SpriteRenderer '{backgroundSpriteRenderer.gameObject.name}': " +
                         $"X({minX:F1} to {maxX:F1}), Y({minY:F1} to {maxY:F1})");
            }
        }
        else
        {
            if (debugLog)
                Debug.LogWarning("[CameraBoundsFromBackground] No background source found to sync from!");
            return;
        }
        
        // Apply padding
        minX += boundaryPadding;
        maxX -= boundaryPadding;
        minY += boundaryPadding;
        maxY -= boundaryPadding;
        
        // Apply to CameraFollow
        cameraFollow.SetBoundaries(minX, maxX, minY, maxY);
    }
    
    /// <summary>
    /// Sets a new CheckerboardBackground and syncs bounds.
    /// </summary>
    public void SetBackground(CheckerboardBackground newBackground)
    {
        checkerboardBackground = newBackground;
        backgroundSpriteRenderer = null;
        SyncBounds();
    }
    
    /// <summary>
    /// Sets a new SpriteRenderer as background and syncs bounds.
    /// </summary>
    public void SetBackground(SpriteRenderer newBackground)
    {
        checkerboardBackground = null;
        backgroundSpriteRenderer = newBackground;
        SyncBounds();
    }
}
