using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FishingController : MonoBehaviour
{
    [Header("Fishing Settings")]
    [SerializeField] private float fishingRange = 3f;
    [SerializeField] private LayerMask waterLayerMask = 1;
    [SerializeField] private Transform rodTip;

    [Header("Terrain Integration")]
    [SerializeField] private TerrainLayerManager terrainManager;

    [Header("Fishing Line")]
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] private Transform bobber;

    [Header("UI References")]
    [SerializeField] private GameObject fishingUI;
    [SerializeField] private GameObject interactionPrompt;

    private GridBasedPlayerController playerController;
    private Camera playerCamera;

    private bool isNearWater = false;
    private bool isFishing = false;
    private bool hasCast = false;
    private Vector3 castTarget;
    private WaterZone currentWaterZone;

    public System.Action<FishData, float> OnFishCaught;

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

    private void CheckWaterProximity()
    {
        // Check both traditional water colliders and tilemap water
        Collider2D waterCollider = Physics2D.OverlapCircle(transform.position, fishingRange, waterLayerMask);
        bool tilemapWaterNearby = false;

        if (terrainManager != null)
        {
            // Check for water tiles within fishing range
            for (float angle = 0; angle < 360; angle += 45)
            {
                Vector3 checkPos = transform.position + new Vector3(
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
        }

        bool wasNearWater = isNearWater;
        isNearWater = waterCollider != null || tilemapWaterNearby;

        if (isNearWater && !wasNearWater)
        {
            if (waterCollider != null)
            {
                currentWaterZone = waterCollider.GetComponent<WaterZone>();
            }
            else
            {
                // Create temporary water zone info for tilemap water
                currentWaterZone = CreateTilemapWaterZone();
            }
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

        GameObject tempZone = new GameObject("TempWaterZone");
        tempZone.transform.position = transform.position;

        WaterZone zone = tempZone.AddComponent<WaterZone>();

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
        {
            StartFishing();
        }
        else if (hasCast)
        {
            // Try to reel in or catch fish
            AttemptCatch();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && isFishing && hasCast)
        {
            StopFishing();
        }
    }

    private void StartFishing()
    {
        if (playerController.IsMoving) return;

        isFishing = true;

        // Get mouse position in world space for cast direction
        Vector3 mousePos = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 direction = (mousePos - transform.position).normalized;
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
        float castTime = 1f;
        float elapsed = 0f;
        Vector3 startPos = rodTip != null ? rodTip.position : transform.position;

        while (elapsed < castTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / castTime;

            if (bobber != null)
            {
                Vector3 currentPos = Vector3.Lerp(startPos, castTarget, t);
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
        float waitTime = Random.Range(2f, 8f);
        yield return new WaitForSeconds(waitTime);

        if (hasCast && currentWaterZone != null)
        {
            // Fish is biting - show some indication
            StartCoroutine(BiteIndication());
        }
    }

    private IEnumerator BiteIndication()
    {
        // Simple bobber animation to indicate bite
        if (bobber != null)
        {
            Vector3 originalPos = bobber.position;
            float duration = 0.3f;
            float elapsed = 0f;

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
            FishData caughtFish = currentWaterZone.TryGetFish();
            if (caughtFish != null)
            {
                float weight = Random.Range(caughtFish.minWeight, caughtFish.maxWeight);
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
            Vector3 startPos = rodTip != null ? rodTip.position : transform.position;
            Vector3 endPos = bobber != null ? bobber.position : castTarget;

            fishingLine.SetPosition(0, startPos);
            fishingLine.SetPosition(1, endPos);
        }
    }

    private void UpdateUI()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isNearWater && !isFishing);
        }

        if (fishingUI != null)
        {
            fishingUI.SetActive(isFishing);
        }
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
}