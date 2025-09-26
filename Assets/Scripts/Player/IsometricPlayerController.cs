using UnityEngine;
using UnityEngine.InputSystem;

public class IsometricPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float sprintMultiplier = 1.5f;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    protected Vector2 moveInput;
    protected bool isSprinting;
    protected Rigidbody2D rb;
    private PlayerInput playerInput;

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

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void Update()
    {
        UpdateAnimations();
        HandleSpriteDirection();
    }

    private void HandleMovement()
    {
        // Convert input to isometric movement
        Vector2 isometricMovement = ConvertToIsometric(moveInput);

        // Apply speed and sprint modifier
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        // Move the player
        rb.linearVelocity = isometricMovement * currentSpeed;
    }

    protected Vector2 ConvertToIsometric(Vector2 input)
    {
        // Convert standard input to isometric coordinates
        // For 2D isometric, we typically rotate the movement by 45 degrees
        Vector2 isometric = new Vector2(
            (input.x - input.y) * 0.5f,
            (input.x + input.y) * 0.5f
        );

        return isometric.normalized * input.magnitude;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = moveInput.magnitude > 0.1f;
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

    public bool IsMoving => moveInput.magnitude > 0.1f;
    public Vector2 GetMoveInput() => moveInput;
    public bool IsSprinting => isSprinting;
}