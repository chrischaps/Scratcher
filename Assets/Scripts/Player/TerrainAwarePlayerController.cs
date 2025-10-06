using UnityEngine;
using Terrain;

public class TerrainAwarePlayerController : IsometricPlayerController
{
    [SerializeField] private LayerMask groundCheckLayer = 1;
    [SerializeField] private float groundCheckRadius = 0.1f;

    [Header("Movement Feedback")] [SerializeField]
    private ParticleSystem movementParticles;

    [SerializeField] private AudioSource movementAudio;
    private float currentSpeedModifier = 1f;

    private const TerrainType CurrentTerrainType = TerrainType.Grass;

    [Header("Terrain Integration")] private TerrainLayerManager terrainManager;

    protected override void Awake()
    {
        base.Awake();

        if (terrainManager == null)
            terrainManager = FindFirstObjectByType<TerrainLayerManager>();
    }

    private new void FixedUpdate()
    {
        CheckCurrentTerrain();
        HandleTerrainMovement();
    }

    private void OnDrawGizmosSelected()
    {
        // Draw ground check radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);

        // Show movement bounds
        if (terrainManager != null)
        {
            Gizmos.color = IsOnWalkableTerrain() ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }

    private void CheckCurrentTerrain()
    {
        if (terrainManager == null) return;

        var checkPosition = transform.position;

        // Get terrain info at current position
        if (terrainManager.IsWalkable(checkPosition))
            currentSpeedModifier = terrainManager.GetMovementSpeedModifier(checkPosition);
        else
            // Player is on unwalkable terrain - could implement sliding or stopping logic
            currentSpeedModifier = 0.5f; // Slow movement on unwalkable terrain
    }

    private void HandleTerrainMovement()
    {
        if (terrainManager != null)
        {
            var intendedPosition = transform.position + (Vector3)(ConvertToIsometric(moveInput) * moveSpeed *
                                                                  currentSpeedModifier *
                                                                  (isSprinting ? sprintMultiplier : 1f) *
                                                                  Time.fixedDeltaTime);

            // Check if the intended position is walkable
            if (terrainManager.IsWalkable(intendedPosition))
            {
                // Move normally with terrain speed modifier
                var isometricMovement = ConvertToIsometric(moveInput);
                var currentSpeed = moveSpeed * currentSpeedModifier * (isSprinting ? sprintMultiplier : 1f);
                rb.linearVelocity = isometricMovement * currentSpeed;

                UpdateMovementEffects();
            }
            else
            {
                // Stop movement if trying to move into unwalkable area
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            // Use default movement
            var isometricMovement = ConvertToIsometric(moveInput);
            var currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
            rb.linearVelocity = isometricMovement * currentSpeed;
        }
    }

    private void UpdateMovementEffects()
    {
        if (moveInput.magnitude > 0.1f)
        {
            // Update particle effects based on terrain
            if (movementParticles != null)
            {
                if (!movementParticles.isPlaying)
                    movementParticles.Play();

                // Modify particle color/rate based on terrain type
                var main = movementParticles.main;
                main.startColor = GetTerrainParticleColor(CurrentTerrainType);
            }

            // Play terrain-appropriate footstep sounds
            if (movementAudio != null && !movementAudio.isPlaying)
                // You can set different audio clips based on terrain type
                movementAudio.Play();
        }
        else
        {
            if (movementParticles != null && movementParticles.isPlaying)
                movementParticles.Stop();
        }
    }

    private Color GetTerrainParticleColor(TerrainType terrainType)
    {
        return terrainType switch
        {
            TerrainType.Grass => Color.green,
            TerrainType.Sand => Color.yellow,
            TerrainType.Stone => Color.gray,
            TerrainType.Dirt => new Color(0.6f, 0.4f, 0.2f),
            TerrainType.Wood => new Color(0.8f, 0.5f, 0.2f),
            _ => Color.white
        };
    }

    public bool IsOnWalkableTerrain()
    {
        return terrainManager != null && terrainManager.IsWalkable(transform.position);
    }

    public TerrainType GetCurrentTerrainType()
    {
        return CurrentTerrainType;
    }

    public float GetCurrentSpeedModifier()
    {
        return currentSpeedModifier;
    }
}