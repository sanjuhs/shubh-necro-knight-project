using UnityEngine;

/// <summary>
/// Smooth camera follow script with boundary clamping.
/// The camera follows the player smoothly and stops at map edges
/// to prevent showing areas beyond the playable map.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target to follow (usually the player)")]
    [SerializeField] private Transform target;
    
    [Tooltip("Offset from the target position")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    
    [Header("Smoothing Settings")]
    [Tooltip("How smoothly the camera follows the target (lower = smoother, higher = snappier)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float smoothSpeed = 0.125f;
    
    [Tooltip("Use unscaled time (camera follows even when game is paused)")]
    [SerializeField] private bool useUnscaledTime = false;
    
    [Header("Boundary Settings")]
    [Tooltip("Enable camera boundaries")]
    [SerializeField] private bool useBoundaries = true;
    
    [Tooltip("Enable camera following (can be disabled to lock camera in place)")]
    [SerializeField] private bool enableFollowing = true;
    
    [Tooltip("Minimum X position the camera can reach")]
    [SerializeField] private float minX = -10f;
    
    [Tooltip("Maximum X position the camera can reach")]
    [SerializeField] private float maxX = 10f;
    
    [Tooltip("Minimum Y position the camera can reach")]
    [SerializeField] private float minY = -10f;
    
    [Tooltip("Maximum Y position the camera can reach")]
    [SerializeField] private float maxY = 10f;
    
    [Header("Debug")]
    [Tooltip("Show boundary gizmos in Scene view")]
    [SerializeField] private bool showBoundaryGizmos = true;
    
    [Tooltip("Color of the boundary gizmos")]
    [SerializeField] private Color boundaryColor = Color.yellow;
    
    // Cached camera reference
    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;
    
    private void Start()
    {
        // Cache camera reference
        cam = GetComponent<Camera>();
        
        // Auto-find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("[CameraFollow] Auto-assigned player as target");
            }
            else
            {
                // Try to find by name
                var warrior = GameObject.Find("Warrior");
                if (warrior != null)
                {
                    target = warrior.transform;
                    Debug.Log("[CameraFollow] Auto-assigned 'Warrior' as target");
                }
                else
                {
                    Debug.LogWarning("[CameraFollow] No target assigned and couldn't find Player or Warrior!");
                }
            }
        }
        
        // Calculate camera bounds based on orthographic size
        CalculateCameraBounds();
    }
    
    private void CalculateCameraBounds()
    {
        if (cam != null && cam.orthographic)
        {
            camHalfHeight = cam.orthographicSize;
            camHalfWidth = camHalfHeight * cam.aspect;
        }
    }
    
    private void LateUpdate()
    {
        if (target == null || !enableFollowing) return;
        
        // Recalculate bounds if camera size changes (e.g., zoom)
        if (cam != null && cam.orthographic)
        {
            camHalfHeight = cam.orthographicSize;
            camHalfWidth = camHalfHeight * cam.aspect;
        }
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly interpolate to desired position
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * deltaTime * 60f);
        
        // Apply boundary clamping
        if (useBoundaries)
        {
            smoothedPosition = ClampToBoundaries(smoothedPosition);
        }
        
        // Apply the new position
        transform.position = smoothedPosition;
    }
    
    /// <summary>
    /// Clamps the camera position to stay within the defined boundaries,
    /// accounting for the camera's viewport size.
    /// </summary>
    private Vector3 ClampToBoundaries(Vector3 position)
    {
        // Clamp X position (accounting for camera width)
        float clampedX = Mathf.Clamp(position.x, minX + camHalfWidth, maxX - camHalfWidth);
        
        // Clamp Y position (accounting for camera height)
        float clampedY = Mathf.Clamp(position.y, minY + camHalfHeight, maxY - camHalfHeight);
        
        // Keep the Z position (camera depth)
        return new Vector3(clampedX, clampedY, position.z);
    }
    
    /// <summary>
    /// Sets the camera boundaries at runtime.
    /// </summary>
    public void SetBoundaries(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
    }
    
    /// <summary>
    /// Enables or disables camera boundaries.
    /// </summary>
    public void SetUseBoundaries(bool enabled)
    {
        useBoundaries = enabled;
    }
    
    /// <summary>
    /// Centers the camera at a specific position (ignoring target and boundaries).
    /// </summary>
    public void CenterAtPosition(Vector3 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
    
    /// <summary>
    /// Enables or disables camera following.
    /// </summary>
    public void SetEnableFollowing(bool enabled)
    {
        enableFollowing = enabled;
    }
    
    /// <summary>
    /// Sets a new target for the camera to follow.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    /// <summary>
    /// Instantly moves the camera to the target position (no smoothing).
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        
        Vector3 targetPosition = target.position + offset;
        
        if (useBoundaries)
        {
            targetPosition = ClampToBoundaries(targetPosition);
        }
        
        transform.position = targetPosition;
    }
    
    /// <summary>
    /// Draws boundary gizmos in the Scene view for easy visualization.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showBoundaryGizmos || !useBoundaries) return;
        
        Gizmos.color = boundaryColor;
        
        // Draw the boundary rectangle
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        
        // Draw boundary lines
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
        
        // Draw corner markers
        float markerSize = 0.5f;
        Gizmos.DrawWireSphere(bottomLeft, markerSize);
        Gizmos.DrawWireSphere(bottomRight, markerSize);
        Gizmos.DrawWireSphere(topLeft, markerSize);
        Gizmos.DrawWireSphere(topRight, markerSize);
        
        // Draw camera bounds preview (if we have a camera)
        Camera sceneCam = GetComponent<Camera>();
        if (sceneCam != null && sceneCam.orthographic)
        {
            Gizmos.color = new Color(boundaryColor.r, boundaryColor.g, boundaryColor.b, 0.3f);
            
            float halfHeight = sceneCam.orthographicSize;
            float halfWidth = halfHeight * sceneCam.aspect;
            
            // Draw camera viewport rectangle
            Vector3 camPos = transform.position;
            Vector3 camBL = new Vector3(camPos.x - halfWidth, camPos.y - halfHeight, 0);
            Vector3 camBR = new Vector3(camPos.x + halfWidth, camPos.y - halfHeight, 0);
            Vector3 camTL = new Vector3(camPos.x - halfWidth, camPos.y + halfHeight, 0);
            Vector3 camTR = new Vector3(camPos.x + halfWidth, camPos.y + halfHeight, 0);
            
            Gizmos.DrawLine(camBL, camBR);
            Gizmos.DrawLine(camBR, camTR);
            Gizmos.DrawLine(camTR, camTL);
            Gizmos.DrawLine(camTL, camBL);
        }
    }
}
