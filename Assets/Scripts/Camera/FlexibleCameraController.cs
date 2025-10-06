using UnityEngine;
using UnityEngine.InputSystem;

public class FlexibleCameraController : MonoBehaviour
{
    [Header("Target Settings")] [SerializeField]
    private Transform target;

    [SerializeField] private Vector3 offset = new(0, 0, -10);

    [Header("Camera Type")] [SerializeField]
    private GridType cameraType = GridType.Isometric;

    [Header("Camera Angles")] [SerializeField]
    private Vector3 isometricRotation = new(30f, 45f, 0f);

    [SerializeField] private Vector3 topDownRotation = new(60f, 0f, 0f);
    [SerializeField] private bool useCustomAngles;

    [Header("Custom Angle Settings")] [SerializeField] [Range(0f, 90f)]
    private float topDownAngle = 60f;

    [SerializeField] [Range(0f, 360f)] private float topDownYRotation;
    [SerializeField] [Range(-45f, 45f)] private float topDownZRotation;

    [Header("Follow Settings")] [SerializeField]
    private float followSpeed = 5f;

    [SerializeField] private bool useSmoothing = true;

    [Header("Bounds (Optional)")] [SerializeField]
    private bool useBounds;

    [SerializeField] private Vector2 minBounds = new(-10, -10);
    [SerializeField] private Vector2 maxBounds = new(10, 10);

    [Header("Zoom Settings")] [SerializeField]
    private float minZoom = 3f;

    [SerializeField] private float maxZoom = 8f;
    [SerializeField] private float zoomSpeed = 2f;

    private Camera cam;
    private float targetZoom;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;

        // Set up isometric camera angle
        SetupView();
    }

    private void Start()
    {
        if (target == null)
        {
            // Try to find player if not assigned
            var player = FindObjectOfType<GridBasedPlayerController>();
            if (player != null)
                target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleCameraFollow();
        HandleZoom();
    }

    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            var center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0);
            var size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }

    private void SetupView()
    {
        // Always use orthographic camera for 2D games
        cam.orthographic = true;

        // Set rotation based on camera type
        SetCameraRotationForType(cameraType);
    }

    private void SetCameraRotationForType(GridType gridType)
    {
        Vector3 targetRotation;

        if (gridType == GridType.Isometric)
        {
            targetRotation = isometricRotation;
        }
        else // Rectangular/TopDown
        {
            if (useCustomAngles)
                targetRotation = new Vector3(topDownAngle, topDownYRotation, topDownZRotation);
            else
                targetRotation = topDownRotation;
        }

        transform.rotation = Quaternion.Euler(targetRotation);
    }

    private void HandleCameraFollow()
    {
        var targetPosition = target.position + offset;

        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        if (useSmoothing)
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        else
            transform.position = targetPosition;
    }

    private void HandleZoom()
    {
        // Handle zoom input using new Input System
        var scrollInput = Mouse.current?.scroll.ReadValue() ?? Vector2.zero;
        var scrollY = scrollInput.y / 120f; // Normalize scroll wheel input

        if (scrollY != 0)
        {
            targetZoom -= scrollY * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Smooth zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * followSpeed);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        useBounds = true;
        minBounds = min;
        maxBounds = max;
    }

    public void DisableBounds()
    {
        useBounds = false;
    }

    public void SetCameraType(GridType newCameraType)
    {
        cameraType = newCameraType;
        SetCameraRotationForType(cameraType);

        // Adjust offset based on camera type
        if (cameraType == GridType.Rectangular)
            offset = new Vector3(0, 0, -10); // Directly overhead
        else
            offset = new Vector3(0, 0, -10); // Isometric offset
    }

    public void ApplyLevelConfiguration(LevelConfiguration levelConfig)
    {
        if (levelConfig?.gridConfig != null)
        {
            SetCameraType(levelConfig.gridConfig.gridType);
            offset = levelConfig.gridConfig.cameraOffset;

            // Apply custom camera angles if specified
            if (levelConfig.gridConfig.useCustomCameraAngles)
            {
                useCustomAngles = true;
                topDownAngle = levelConfig.gridConfig.topDownAngle;
                topDownYRotation = levelConfig.gridConfig.topDownYRotation;
                topDownZRotation = levelConfig.gridConfig.topDownZRotation;

                // Re-apply camera rotation with new angles
                SetCameraRotationForType(cameraType);
            }
            else
            {
                useCustomAngles = false;
                SetCameraRotationForType(cameraType);
            }
        }
    }

    public void SetTopDownAngle(float angle)
    {
        topDownAngle = Mathf.Clamp(angle, 0f, 90f);
        if (cameraType == GridType.Rectangular) SetCameraRotationForType(cameraType);
    }

    public void SetTopDownRotation(float yRotation, float zRotation = 0f)
    {
        topDownYRotation = yRotation;
        topDownZRotation = Mathf.Clamp(zRotation, -45f, 45f);
        if (cameraType == GridType.Rectangular) SetCameraRotationForType(cameraType);
    }
}