using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down 2D Warrior Controller using Unity's New Input System
/// Controls movement via mouse click and handles animation states
/// </summary>
public class WarriorController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.1f;
    
    [Header("Combat Settings")]
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float heavyAttackDuration = 0.7f;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    // Animation parameter names
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int Attack1Trigger = Animator.StringToHash("Attack1");
    private static readonly int Attack2Trigger = Animator.StringToHash("Attack2");
    private static readonly int GuardTrigger = Animator.StringToHash("Guard");
    private static readonly int IsGuarding = Animator.StringToHash("IsGuarding");
    
    // State tracking
    private Vector3 targetPosition;
    private bool isMoving;
    private bool isAttacking;
    private bool isGuarding;
    private float actionTimer;
    
    // Input System references
    private Mouse mouse;
    private Keyboard keyboard;
    
    // Current action being performed
    private enum ActionState { None, Attack1, Attack2, Guard }
    private ActionState currentAction = ActionState.None;

    private void Start()
    {
        // Initialize target position to current position
        targetPosition = transform.position;
        
        // Auto-get components if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Get Input System device references
        mouse = Mouse.current;
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        // Refresh device references if needed (handles device reconnection)
        if (mouse == null) mouse = Mouse.current;
        if (keyboard == null) keyboard = Keyboard.current;
        
        HandleInput();
        HandleMovement();
        HandleActionTimer();
        UpdateAnimations();
    }

    /// <summary>
    /// Handles all input using the New Input System
    /// </summary>
    private void HandleInput()
    {
        // Safety check for devices
        if (mouse == null) return;
        
        // Don't accept new movement/attack input while performing an action
        if (isAttacking)
        {
            // But allow guard release
            if (keyboard != null && keyboard.spaceKey.wasReleasedThisFrame)
            {
                StopGuard();
            }
            return;
        }
        
        // Handle guard input separately (can be released even while guarding)
        if (isGuarding)
        {
            if (keyboard != null && 
                (keyboard.spaceKey.wasReleasedThisFrame || keyboard.leftShiftKey.wasReleasedThisFrame))
            {
                StopGuard();
            }
            return;
        }
        
        // Right-click to move
        if (mouse.rightButton.wasPressedThisFrame)
        {
            SetMoveTarget();
        }
        
        // Left-click for Attack1 (front hand attack)
        if (mouse.leftButton.wasPressedThisFrame)
        {
            StartAttack1();
        }
        
        // Q key for Attack2 (heavy attack)
        if (keyboard != null && keyboard.qKey.wasPressedThisFrame)
        {
            StartAttack2();
        }
        
        // Hold Space or Shift for Guard
        if (keyboard != null && 
            (keyboard.spaceKey.wasPressedThisFrame || keyboard.leftShiftKey.wasPressedThisFrame))
        {
            StartGuard();
        }
    }

    /// <summary>
    /// Sets the movement target to the mouse position in world space
    /// </summary>
    private void SetMoveTarget()
    {
        if (mouse == null || Camera.main == null) return;
        
        Vector2 mouseScreenPos = mouse.position.ReadValue();
        Vector3 mousePos = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.transform.position.z * -1);
        targetPosition = Camera.main.ScreenToWorldPoint(mousePos);
        targetPosition.z = 0; // Keep on 2D plane
        isMoving = true;
    }

    /// <summary>
    /// Handles character movement towards target position
    /// </summary>
    private void HandleMovement()
    {
        // Don't move while attacking or guarding
        if (isAttacking || isGuarding)
        {
            isMoving = false;
            return;
        }
        
        if (!isMoving)
            return;
        
        // Calculate distance to target
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        // Check if we've reached the target
        if (distance <= stoppingDistance)
        {
            isMoving = false;
            return;
        }
        
        // Move towards target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Flip sprite based on movement direction
        FlipSprite(direction.x);
    }

    /// <summary>
    /// Flips the sprite based on horizontal direction
    /// </summary>
    private void FlipSprite(float horizontalDirection)
    {
        if (spriteRenderer == null) return;
        
        if (Mathf.Abs(horizontalDirection) > 0.01f)
        {
            spriteRenderer.flipX = horizontalDirection < 0;
        }
    }

    /// <summary>
    /// Starts the front hand attack (Attack1)
    /// </summary>
    private void StartAttack1()
    {
        isAttacking = true;
        currentAction = ActionState.Attack1;
        actionTimer = attackDuration;
        
        if (animator != null)
            animator.SetTrigger(Attack1Trigger);
        
        // Face towards mouse position
        FaceTowardsMouse();
    }

    /// <summary>
    /// Starts the heavy attack (Attack2)
    /// </summary>
    private void StartAttack2()
    {
        isAttacking = true;
        currentAction = ActionState.Attack2;
        actionTimer = heavyAttackDuration;
        
        if (animator != null)
            animator.SetTrigger(Attack2Trigger);
        
        // Face towards mouse position
        FaceTowardsMouse();
    }

    /// <summary>
    /// Starts the guard/defend state
    /// </summary>
    private void StartGuard()
    {
        isGuarding = true;
        currentAction = ActionState.Guard;
        
        if (animator != null)
        {
            animator.SetBool(IsGuarding, true);
            animator.SetTrigger(GuardTrigger);
        }
    }

    /// <summary>
    /// Stops the guard/defend state
    /// </summary>
    private void StopGuard()
    {
        isGuarding = false;
        currentAction = ActionState.None;
        
        if (animator != null)
            animator.SetBool(IsGuarding, false);
    }

    /// <summary>
    /// Faces the character towards the current mouse position
    /// </summary>
    private void FaceTowardsMouse()
    {
        if (mouse == null || Camera.main == null) return;
        
        Vector2 mouseScreenPos = mouse.position.ReadValue();
        Vector3 mousePos = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.transform.position.z * -1);
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        
        float direction = worldMousePos.x - transform.position.x;
        FlipSprite(direction);
    }

    /// <summary>
    /// Handles the action timer for attacks
    /// </summary>
    private void HandleActionTimer()
    {
        if (!isAttacking)
            return;
        
        actionTimer -= Time.deltaTime;
        
        if (actionTimer <= 0)
        {
            isAttacking = false;
            currentAction = ActionState.None;
        }
    }

    /// <summary>
    /// Updates animator parameters based on current state
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Set running animation based on movement
        animator.SetBool(IsRunning, isMoving && !isAttacking && !isGuarding);
    }

    /// <summary>
    /// Called by animation events when attack completes (optional)
    /// </summary>
    public void OnAttackComplete()
    {
        isAttacking = false;
        currentAction = ActionState.None;
    }

    /// <summary>
    /// Draws gizmos in the editor for debugging
    /// </summary>
    private void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
