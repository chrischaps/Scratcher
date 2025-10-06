using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Terrain;

public class FishingController : MonoBehaviour
{
    [Header("Fishing Settings")] [SerializeField]
    private float fishingRange = 3f;

    [SerializeField] private LayerMask waterLayerMask = 1;
    [SerializeField] private Transform rodTip;

    [Header("Fishing Line")] [SerializeField]
    private LineRenderer fishingLine;

    [SerializeField] private Transform bobber;

    [Header("UI References")] [SerializeField]
    private GameObject fishingUI;

    [SerializeField] private GameObject interactionPrompt;
    private Vector3 castTarget;
    private WaterZone currentWaterZone;
    private bool hasCast;
    private bool isFishing;

    private bool isNearWater;

    public Action<FishData, float> OnFishCaught;
    private Camera playerCamera;

    private GridBasedPlayerController playerController;

    [Header("Terrain Integration")] private TerrainLayerManager terrainManager;

    private void Awake()
    {
        playerController = GetComponent<GridBasedPlayerController>();
        playerCamera = Camera.main;

        if (terrainManager == null)
            terrainManager = FindObjectOfType<TerrainLayerManager>();

        if (fishingLine == null)
            fishingLine = GetComponentInChildren<LineRenderer>();

        if (fishingLine != null)
        {
            fishingLine.positionCount = 2;
            fishingLine.enabled = false;
        }

        if (bobber != null)
            bobber.gameObject.SetActive(false);
    }

    private void Update()
    {
        CheckWaterProximity();
        UpdateFishingLine();
        UpdateUI();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, fishingRange);

        if (hasCast)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(castTarget, 0.2f);
        }
    }

    private void CheckWaterProximity()
    {
        // Check both traditional water colliders and tilemap water
        var waterCollider = Physics2D.OverlapCircle(transform.position, fishingRange, waterLayerMask);
        var tilemapWaterNearby = false;

        if (terrainManager != null)
            // Check for water tiles within fishing range
            for (float angle = 0; angle < 360; angle += 45)
            {
                var checkPos = transform.position + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * fishingRange,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * fishingRange,
                    0
                );

                if (terrainManager.IsWater(checkPos))
                {
                    tilemapWaterNearby = true;
                    break;
                }
            }

        var wasNearWater = isNearWater;
        isNearWater = waterCollider != null || tilemapWaterNearby;

        if (isNearWater && !wasNearWater)
        {
            if (waterCollider != null)
                currentWaterZone = waterCollider.GetComponent<WaterZone>();
            else
                // Create temporary water zone info for tilemap water
                currentWaterZone = CreateTilemapWaterZone();
        }
        else if (!isNearWater && wasNearWater)
        {
            currentWaterZone = null;
            if (isFishing)
                StopFishing();
        }
    }

    private WaterZone CreateTilemapWaterZone()
    {
        // This creates a temporary water zone based on tilemap data
        // In practice, the TerrainLayerManager should handle this automatically
        if (terrainManager == null) return null;

        var tempZone = new GameObject("TempWaterZone");
        tempZone.transform.position = transform.position;

        var zone = tempZone.AddComponent<WaterZone>();

        // You could enhance this to get water type from the tilemap
        WaterType waterType = terrainManager.GetWaterType(transform.position);

        // Set up the zone based on tilemap properties
        // This would need additional methods in WaterZone to configure it

        return zone;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed || !isNearWater) return;

        if (!isFishing)
            StartFishing();
        else if (hasCast)
            // Try to reel in or catch fish
            AttemptCatch();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && isFishing && hasCast) StopFishing();
    }

    private void StartFishing()
    {
        if (playerController.IsMoving) return;

        isFishing = true;

        // Get mouse position in world space for cast direction
        var mousePos = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        var direction = (mousePos - transform.position).normalized;
        castTarget = transform.position + direction * fishingRange;

        StartCoroutine(CastLine());
    }

    private IEnumerator CastLine()
    {
        if (fishingLine != null)
            fishingLine.enabled = true;

        if (bobber != null)
        {
            bobber.gameObject.SetActive(true);
            bobber.position = transform.position;
        }

        // Animate casting
        var castTime = 1f;
        var elapsed = 0f;
        var startPos = rodTip != null ? rodTip.position : transform.position;

        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / castTime;

            if (bobber != null)
            {
                var currentPos = Vector3.Lerp(startPos, castTarget, t);
                // Add arc to the cast
                currentPos.y += Mathf.Sin(t * Mathf.PI) * 2f;
                bobber.position = currentPos;
            }

            yield return null;
        }

        if (bobber != null)
            bobber.position = castTarget;

        hasCast = true;

        // Start bite detection
        StartCoroutine(WaitForBite());
    }

    private IEnumerator WaitForBite()
    {
        var waitTime = Random.Range(2f, 8f);
        yield return new WaitForSeconds(waitTime);

        if (hasCast && currentWaterZone != null)
            // Fish is biting - show some indication
            StartCoroutine(BiteIndication());
    }

    private IEnumerator BiteIndication()
    {
        // Simple bobber animation to indicate bite
        if (bobber != null)
        {
            var originalPos = bobber.position;
            var duration = 0.3f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bobber.position = originalPos + Vector3.up * Mathf.Sin(elapsed * 20f) * 0.2f;
                yield return null;
            }

            bobber.position = originalPos;
        }
    }

    private void AttemptCatch()
    {
        if (currentWaterZone != null)
        {
            var caughtFish = currentWaterZone.TryGetFish();
            if (caughtFish != null)
            {
                var weight = Random.Range(caughtFish.minWeight, caughtFish.maxWeight);
                OnFishCaught?.Invoke(caughtFish, weight);
            }
        }

        StopFishing();
    }

    private void StopFishing()
    {
        isFishing = false;
        hasCast = false;

        if (fishingLine != null)
            fishingLine.enabled = false;

        if (bobber != null)
            bobber.gameObject.SetActive(false);

        StopAllCoroutines();
    }

    private void UpdateFishingLine()
    {
        if (fishingLine != null && fishingLine.enabled)
        {
            var startPos = rodTip != null ? rodTip.position : transform.position;
            var endPos = bobber != null ? bobber.position : castTarget;

            fishingLine.SetPosition(0, startPos);
            fishingLine.SetPosition(1, endPos);
        }
    }

    private void UpdateUI()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(isNearWater && !isFishing);

        if (fishingUI != null) fishingUI.SetActive(isFishing);
    }
}