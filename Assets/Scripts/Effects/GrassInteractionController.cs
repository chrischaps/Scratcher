using UnityEngine;

/// <summary>
/// Updates grass shader interaction position based on target (usually player) position
/// Attach this to your player or a manager object
/// </summary>
public class GrassInteractionController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("The transform to track (usually the player). If null, uses this object's position.")]
    [SerializeField] private Transform targetTransform;

    [Tooltip("Material(s) with the AnimatedGrass2D shader to update")]
    [SerializeField] private Material[] grassMaterials;

    [Tooltip("Auto-find all materials using the AnimatedGrass2D shader in the scene")]
    [SerializeField] private bool autoFindMaterials = true;

    [Header("Performance")]
    [Tooltip("Update rate in seconds. Lower = smoother but more expensive. 0 = every frame.")]
    [SerializeField] private float updateInterval = 0.02f;

    private float lastUpdateTime;
    private static readonly int InteractionPositionID = Shader.PropertyToID("_InteractionPosition");

    private void Start()
    {
        if (targetTransform == null)
            targetTransform = transform;

        if (autoFindMaterials)
            FindGrassMaterials();

        if (grassMaterials == null || grassMaterials.Length == 0)
            Debug.LogWarning("GrassInteractionController: No grass materials assigned or found!");
    }

    private void Update()
    {
        // Update at specified interval
        if (updateInterval > 0 && Time.time - lastUpdateTime < updateInterval)
            return;

        lastUpdateTime = Time.time;
        UpdateGrassInteraction();
    }

    private void UpdateGrassInteraction()
    {
        if (grassMaterials == null || grassMaterials.Length == 0)
            return;

        Vector3 position = targetTransform.position;
        Vector4 shaderPosition = new Vector4(position.x, position.y, position.z, 0);

        foreach (var material in grassMaterials)
        {
            if (material != null)
                material.SetVector(InteractionPositionID, shaderPosition);
        }
    }

    private void FindGrassMaterials()
    {
        // Find all SpriteRenderers in scene
        var spriteRenderers = FindObjectsOfType<SpriteRenderer>();
        var foundMaterials = new System.Collections.Generic.List<Material>();

        foreach (var sr in spriteRenderers)
        {
            if (sr.sharedMaterial != null &&
                sr.sharedMaterial.shader.name == "Custom/AnimatedGrass2D")
            {
                // Use sharedMaterial for instances that share the same material
                if (!foundMaterials.Contains(sr.sharedMaterial))
                    foundMaterials.Add(sr.sharedMaterial);
            }
        }

        // Also check Tilemap renderers
        var tilemapRenderers = FindObjectsOfType<UnityEngine.Tilemaps.TilemapRenderer>();
        foreach (var tr in tilemapRenderers)
        {
            if (tr.sharedMaterial != null &&
                tr.sharedMaterial.shader.name == "Custom/AnimatedGrass2D")
            {
                if (!foundMaterials.Contains(tr.sharedMaterial))
                    foundMaterials.Add(tr.sharedMaterial);
            }
        }

        grassMaterials = foundMaterials.ToArray();
        Debug.Log($"GrassInteractionController: Found {grassMaterials.Length} grass material(s)");
    }

    // Public method to manually set materials
    public void SetGrassMaterials(Material[] materials)
    {
        grassMaterials = materials;
    }

    // Public method to add a material
    public void AddGrassMaterial(Material material)
    {
        if (material == null) return;

        var list = new System.Collections.Generic.List<Material>();
        if (grassMaterials != null)
            list.AddRange(grassMaterials);

        if (!list.Contains(material))
        {
            list.Add(material);
            grassMaterials = list.ToArray();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw interaction radius for first material
        if (grassMaterials != null && grassMaterials.Length > 0 && grassMaterials[0] != null)
        {
            float radius = grassMaterials[0].GetFloat("_InteractionRadius");
            Vector3 pos = targetTransform != null ? targetTransform.position : transform.position;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pos, radius);
        }
    }
}
