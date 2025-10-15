using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for GridRenderer with helpful buttons
/// </summary>
[CustomEditor(typeof(GridRenderer))]
public class GridRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridRenderer gridRenderer = (GridRenderer)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Auto-Size to Tilemaps"))
        {
            AutoSizeToTilemaps(gridRenderer);
        }

        EditorGUILayout.HelpBox(
            "Tip: Use 'Auto-Size to Tilemaps' to automatically calculate grid size based on your tilemap bounds.",
            MessageType.Info);
    }

    private void AutoSizeToTilemaps(GridRenderer gridRenderer)
    {
        var grid = gridRenderer.GetComponent<Grid>();
        if (grid == null)
        {
            Debug.LogWarning("No Grid component found!");
            return;
        }

        // Find all tilemaps under this grid
        var tilemaps = grid.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>();
        if (tilemaps.Length == 0)
        {
            Debug.LogWarning("No Tilemaps found under Grid!");
            return;
        }

        // Calculate bounds
        BoundsInt? combinedBounds = null;
        foreach (var tilemap in tilemaps)
        {
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;

            if (combinedBounds == null)
            {
                combinedBounds = bounds;
            }
            else
            {
                BoundsInt current = combinedBounds.Value;
                Vector3Int min = Vector3Int.Min(current.min, bounds.min);
                Vector3Int max = Vector3Int.Max(current.max, bounds.max);
                combinedBounds = new BoundsInt(min, max - min);
            }
        }

        if (combinedBounds.HasValue)
        {
            BoundsInt bounds = combinedBounds.Value;
            Vector2Int center = new Vector2Int(
                bounds.xMin + bounds.size.x / 2,
                bounds.yMin + bounds.size.y / 2
            );

            SerializedObject so = new SerializedObject(gridRenderer);
            so.FindProperty("gridCenter").vector2IntValue = center;
            so.FindProperty("gridSize").vector2IntValue = new Vector2Int(bounds.size.x, bounds.size.y);
            so.ApplyModifiedProperties();

            Debug.Log($"Grid auto-sized to: Center={center}, Size={bounds.size}");
        }
    }
}

/// <summary>
/// Menu items for quickly adding grid renderer to scene
/// </summary>
public static class GridRendererMenu
{
    [MenuItem("GameObject/2D Object/Grid with Renderer", false, 10)]
    public static void CreateGridWithRenderer()
    {
        GameObject gridObject = new GameObject("Grid");
        Grid grid = gridObject.AddComponent<Grid>();
        GridRenderer gridRenderer = gridObject.AddComponent<GridRenderer>();

        // Set default values
        SerializedObject so = new SerializedObject(gridRenderer);
        so.FindProperty("gridSize").vector2IntValue = new Vector2Int(50, 50);
        so.FindProperty("showInPlayMode").boolValue = true;
        so.FindProperty("showInEditor").boolValue = true;
        so.ApplyModifiedProperties();

        Selection.activeGameObject = gridObject;
        Undo.RegisterCreatedObjectUndo(gridObject, "Create Grid with Renderer");
    }

    [MenuItem("GameObject/Add Grid Renderer", false, 10)]
    public static void AddGridRendererToSelected()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("Please select a GameObject with a Grid component first!");
            return;
        }

        Grid grid = selected.GetComponent<Grid>();
        if (grid == null)
        {
            Debug.LogWarning("Selected GameObject doesn't have a Grid component!");
            return;
        }

        if (selected.GetComponent<GridRenderer>() != null)
        {
            Debug.LogWarning("GameObject already has a GridRenderer component!");
            return;
        }

        GridRenderer gridRenderer = selected.AddComponent<GridRenderer>();
        Undo.RegisterCreatedObjectUndo(gridRenderer, "Add Grid Renderer");

        Debug.Log("GridRenderer added! Use 'Auto-Size to Tilemaps' button in Inspector.");
    }

    [MenuItem("GameObject/Add Grid Renderer", true)]
    public static bool ValidateAddGridRenderer()
    {
        return Selection.activeGameObject != null &&
               Selection.activeGameObject.GetComponent<Grid>() != null;
    }
}
