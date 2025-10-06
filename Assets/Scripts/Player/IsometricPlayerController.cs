using UnityEngine;
using UnityEngine.InputSystem;

public class IsometricPlayerController : MonoBehaviour
{
    [Header("Movement Settings")] [SerializeField]
    protected float moveSpeed = 5f;

    [SerializeField] protected float sprintMultiplier = 1.5f;

    [Header("Animation")] [SerializeField] protected Animator animator;

    [SerializeField] protected SpriteRenderer spriteRenderer;
    protected bool isSprinting;

    protected Vector2 moveInput;
    private PlayerInput playerInput;
    protected Rigidbody2D rb;

    public bool IsMoving => moveInput.magnitude > 0.1f;
    public bool IsSprinting => isSprinting;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        // Configure physics for 2D top-down movement
        if (rb != null)
        {
            rb.gravityScale = 0f; // No gravity for top-down
            rb.linearDamping = 8f; // High drag to stop sliding
            rb.freezeRotation = true; // Prevent rotation
        }

        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateAnimations();
        HandleSpriteDirection();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            isSprinting = true;
        else if (context.canceled)
            isSprinting = false;
    }

    private void HandleMovement()
    {
        // Convert input to isometric movement
        var isometricMovement = ConvertToIsometric(moveInput);

        // Apply speed and sprint modifier
        var currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        // Move the player
        rb.linearVelocity = isometricMovement * currentSpeed;
    }

    protected Vector2 ConvertToIsometric(Vector2 input)
    {
        // Convert standard input to isometric coordinates
        // For 2D isometric, we typically rotate the movement by 45 degrees
        var isometric = new Vector2(
            (input.x - input.y) * 0.5f,
            (input.x + input.y) * 0.5f
        );

        return isometric.normalized * input.magnitude;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        var isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsSprinting", isSprinting && isMoving);

        // Set movement direction for animation blend tree
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
    }

    private void HandleSpriteDirection()
    {
        if (spriteRenderer == null || moveInput.magnitude < 0.1f) return;

        // Flip sprite based on horizontal movement
        if (moveInput.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (moveInput.x < -0.1f)
            spriteRenderer.flipX = true;
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
}