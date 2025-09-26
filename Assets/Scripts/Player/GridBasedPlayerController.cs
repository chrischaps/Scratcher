using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GridBasedPlayerController : MonoBehaviour
{
    [Header("Grid Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useGridMovement = false;
    [SerializeField] private Grid gameGrid;
    [SerializeField] private GridType gridType = GridType.Isometric;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Terrain Integration")]
    [SerializeField] private TerrainLayerManager terrainManager;

    private Vector2 moveInput;
    private bool isSprinting;
    private bool isMoving = false;
    private Vector3 targetPosition;

    private void Awake()
    {
        if (gameGrid == null)
            gameGrid = FindObjectOfType<Grid>();

        if (terrainManager == null)
            terrainManager = FindObjectOfType<TerrainLayerManager>();

        if (animator == null)
            animator = GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        targetPosition = transform.position;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (useGridMovement)
        {
            // For grid movement, only accept input when not currently moving
            if (!isMoving && context.performed)
            {
                Vector2 input = context.ReadValue<Vector2>();
                TryMoveToAdjacentTile(input);
            }
        }
        else
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            isSprinting = true;
        else if (context.canceled)
            isSprinting = false;
    }

    private void Update()
    {
        if (useGridMovement)
        {
            UpdateGridMovement();
        }
        else
        {
            UpdateSmoothMovement();
        }

        UpdateAnimations();
        HandleSpriteDirection();
    }

    private void TryMoveToAdjacentTile(Vector2 input)
    {
        if (gameGrid == null) return;

        // Convert input to grid direction
        Vector3Int direction = Vector3Int.zero;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            direction.x = input.x > 0 ? 1 : -1;
        }
        else if (input.y != 0)
        {
            direction.y = input.y > 0 ? 1 : -1;
        }

        if (direction == Vector3Int.zero) return;

        // Get current grid position
        Vector3Int currentCell = gameGrid.WorldToCell(transform.position);
        Vector3Int targetCell = currentCell + direction;

        // Check if target position is walkable
        Vector3 worldTargetPos = gameGrid.CellToWorld(targetCell);
        // Center the character in the grid cell
        worldTargetPos += new Vector3(gameGrid.cellSize.x * 0.5f, gameGrid.cellSize.y * 0.5f, 0f);

        if (terrainManager == null || terrainManager.IsWalkable(worldTargetPos))
        {
            targetPosition = worldTargetPos;
            isMoving = true;
        }
    }

    private void UpdateGridMovement()
    {
        if (isMoving)
        {
            // Move towards target position
            float currentSpeed = moveSpeed * (isSprinting ? 1.5f : 1f);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

            // Check if we've reached the target
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    private void UpdateSmoothMovement()
    {
        if (moveInput.magnitude > 0.1f)
        {
            Vector2 convertedInput = ConvertInputForGridType(moveInput);
            Vector3 intendedPosition = transform.position + (Vector3)(convertedInput * moveSpeed * Time.deltaTime);

            // Check terrain if available
            if (terrainManager == null || terrainManager.IsWalkable(intendedPosition))
            {
                transform.position = intendedPosition;
            }
        }
    }

    private Vector2 ConvertInputForGridType(Vector2 input)
    {
        switch (gridType)
        {
            case GridType.Isometric:
                // Convert standard input to isometric coordinates
                return new Vector2(
                    (input.x - input.y) * 0.5f,
                    (input.x + input.y) * 0.5f
                ).normalized * input.magnitude;

            case GridType.Rectangular:
            default:
                // Use input directly for top-down rectangular movement
                return input;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool moving = useGridMovement ? isMoving : moveInput.magnitude > 0.1f;
        animator.SetBool("IsMoving", moving);
        animator.SetBool("IsSprinting", isSprinting && moving);

        if (!useGridMovement)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
        }
    }

    private void HandleSpriteDirection()
    {
        if (spriteRenderer == null) return;

        Vector2 currentInput = useGridMovement ?
            ((targetPosition - transform.position).magnitude > 0.1f ? (Vector2)(targetPosition - transform.position).normalized : Vector2.zero) :
            moveInput;

        if (currentInput.magnitude > 0.1f)
        {
            if (currentInput.x > 0.1f)
                spriteRenderer.flipX = false;
            else if (currentInput.x < -0.1f)
                spriteRenderer.flipX = true;
        }
    }

    public void SetGridMovement(bool enabled)
    {
        useGridMovement = enabled;
        if (enabled)
        {
            // Snap to nearest grid position (centered)
            if (gameGrid != null)
            {
                Vector3Int currentCell = gameGrid.WorldToCell(transform.position);
                Vector3 centeredPosition = gameGrid.CellToWorld(currentCell) +
                    new Vector3(gameGrid.cellSize.x * 0.5f, gameGrid.cellSize.y * 0.5f, 0f);
                transform.position = centeredPosition;
                targetPosition = transform.position;
            }
        }
    }

    public void SetGridType(GridType newGridType)
    {
        gridType = newGridType;

        // Update the grid's cell layout if we have access to it
        if (gameGrid != null)
        {
            gameGrid.cellLayout = gridType == GridType.Isometric ? Grid.CellLayout.Isometric : Grid.CellLayout.Rectangle;
        }

        // Snap to grid if using grid movement
        if (useGridMovement)
        {
            SetGridMovement(true);
        }
    }

    public void ApplyLevelConfiguration(LevelConfiguration levelConfig)
    {
        if (levelConfig == null) return;

        SetGridType(levelConfig.gridConfig.gridType);
        moveSpeed = levelConfig.gridConfig.movementSpeed;
        SetGridMovement(levelConfig.gridConfig.useGridBasedMovement);

        if (levelConfig.gridConfig != null)
        {
            levelConfig.ApplyToGrid(gameGrid);
        }
    }

    public bool IsMoving => useGridMovement ? isMoving : moveInput.magnitude > 0.1f;
    public GridType GetCurrentGridType() => gridType;
}