using UnityEngine;

/// <summary>
/// Renders grid lines for the tilemap grid
/// Useful for debugging and level design
/// </summary>
[RequireComponent(typeof(Grid))]
public class GridRenderer : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Show grid lines in play mode")]
    [SerializeField] private bool showInPlayMode = true;

    [Tooltip("Show grid lines in editor (Scene view)")]
    [SerializeField] private bool showInEditor = true;

    [Header("Appearance")]
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private float lineWidth = 0.02f;

    [Header("Grid Area")]
    [Tooltip("Center of the grid area")]
    [SerializeField] private Vector2Int gridCenter = Vector2Int.zero;

    [Tooltip("Size of the grid area (in tiles)")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(50, 50);

    [Header("Performance")]
    [Tooltip("Use a mesh for better performance with large grids")]
    [SerializeField] private bool useMeshRenderer = true;

    private Grid grid;
    private Material lineMaterial;
    private GameObject gridMeshObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        grid = GetComponent<Grid>();

        if (useMeshRenderer)
            CreateGridMesh();
    }

    private void OnEnable()
    {
        if (gridMeshObject != null)
            gridMeshObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (gridMeshObject != null)
            gridMeshObject.SetActive(false);
    }

    private void CreateGridMesh()
    {
        // Create a child object for the grid mesh
        gridMeshObject = new GameObject("GridMesh");
        gridMeshObject.transform.SetParent(transform);
        gridMeshObject.transform.localPosition = Vector3.zero;
        gridMeshObject.layer = gameObject.layer;

        meshFilter = gridMeshObject.AddComponent<MeshFilter>();
        meshRenderer = gridMeshObject.AddComponent<MeshRenderer>();

        // Create material for grid lines
        CreateLineMaterial();
        meshRenderer.material = lineMaterial;
        meshRenderer.sortingLayerName = "Default";
        meshRenderer.sortingOrder = 1000; // Render on top

        // Generate the mesh
        UpdateGridMesh();
    }

    private void CreateLineMaterial()
    {
        // Create a simple unlit material for the grid
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        lineMaterial = new Material(shader);
        lineMaterial.color = gridColor;

        // Make it render transparently
        lineMaterial.SetFloat("_Surface", 1); // Transparent
        lineMaterial.SetFloat("_Blend", 0); // Alpha blend
        lineMaterial.renderQueue = 3000;
    }

    private void UpdateGridMesh()
    {
        if (meshFilter == null) return;

        Mesh mesh = new Mesh();
        mesh.name = "Grid";

        Vector3 cellSize = grid.cellSize;
        Vector2Int halfSize = gridSize / 2;
        Vector2Int min = gridCenter - halfSize;
        Vector2Int max = gridCenter + halfSize;

        // Calculate vertex and index counts for quads (lines with width)
        int horizontalLines = gridSize.y + 1;
        int verticalLines = gridSize.x + 1;
        int totalLines = horizontalLines + verticalLines;
        int vertexCount = totalLines * 4; // 4 vertices per line (quad)
        int indexCount = totalLines * 6; // 6 indices per line (2 triangles)

        Vector3[] vertices = new Vector3[vertexCount];
        int[] indices = new int[indexCount];
        Color[] colors = new Color[vertexCount];

        int vertexIndex = 0;
        int indexIndex = 0;

        float halfWidth = lineWidth * 0.5f;

        // Horizontal lines
        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 startWorld = grid.CellToWorld(new Vector3Int(min.x, min.y + y, 0));
            Vector3 endWorld = grid.CellToWorld(new Vector3Int(max.x, min.y + y, 0));

            // Convert to local space
            Vector3 start = transform.InverseTransformPoint(startWorld);
            Vector3 end = transform.InverseTransformPoint(endWorld);

            // Calculate perpendicular direction in local space
            Vector3 lineDir = (end - start).normalized;
            Vector3 perpendicular = new Vector3(-lineDir.y, lineDir.x, 0) * halfWidth;

            vertices[vertexIndex + 0] = start - perpendicular;
            vertices[vertexIndex + 1] = start + perpendicular;
            vertices[vertexIndex + 2] = end + perpendicular;
            vertices[vertexIndex + 3] = end - perpendicular;

            // Set colors
            for (int i = 0; i < 4; i++)
                colors[vertexIndex + i] = gridColor;

            // Set indices (two triangles)
            indices[indexIndex + 0] = vertexIndex + 0;
            indices[indexIndex + 1] = vertexIndex + 1;
            indices[indexIndex + 2] = vertexIndex + 2;
            indices[indexIndex + 3] = vertexIndex + 0;
            indices[indexIndex + 4] = vertexIndex + 2;
            indices[indexIndex + 5] = vertexIndex + 3;

            vertexIndex += 4;
            indexIndex += 6;
        }

        // Vertical lines
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 startWorld = grid.CellToWorld(new Vector3Int(min.x + x, min.y, 0));
            Vector3 endWorld = grid.CellToWorld(new Vector3Int(min.x + x, max.y, 0));

            // Convert to local space
            Vector3 start = transform.InverseTransformPoint(startWorld);
            Vector3 end = transform.InverseTransformPoint(endWorld);

            // Calculate perpendicular direction in local space
            Vector3 lineDir = (end - start).normalized;
            Vector3 perpendicular = new Vector3(-lineDir.y, lineDir.x, 0) * halfWidth;

            vertices[vertexIndex + 0] = start - perpendicular;
            vertices[vertexIndex + 1] = start + perpendicular;
            vertices[vertexIndex + 2] = end + perpendicular;
            vertices[vertexIndex + 3] = end - perpendicular;

            // Set colors
            for (int i = 0; i < 4; i++)
                colors[vertexIndex + i] = gridColor;

            // Set indices (two triangles)
            indices[indexIndex + 0] = vertexIndex + 0;
            indices[indexIndex + 1] = vertexIndex + 1;
            indices[indexIndex + 2] = vertexIndex + 2;
            indices[indexIndex + 3] = vertexIndex + 0;
            indices[indexIndex + 4] = vertexIndex + 2;
            indices[indexIndex + 5] = vertexIndex + 3;

            vertexIndex += 4;
            indexIndex += 6;
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void OnDrawGizmos()
    {
        if (!showInEditor || useMeshRenderer) return;

        if (grid == null)
            grid = GetComponent<Grid>();

        DrawGridGizmos();
    }

    private void OnRenderObject()
    {
        if (!showInPlayMode || !Application.isPlaying || useMeshRenderer) return;

        DrawGridLines();
    }

    private void DrawGridGizmos()
    {
        Gizmos.color = gridColor;

        Vector2Int halfSize = gridSize / 2;
        Vector2Int min = gridCenter - halfSize;
        Vector2Int max = gridCenter + halfSize;

        // Draw horizontal lines
        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 start = grid.CellToWorld(new Vector3Int(min.x, min.y + y, 0));
            Vector3 end = grid.CellToWorld(new Vector3Int(max.x, min.y + y, 0));
            Gizmos.DrawLine(start, end);
        }

        // Draw vertical lines
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = grid.CellToWorld(new Vector3Int(min.x + x, min.y, 0));
            Vector3 end = grid.CellToWorld(new Vector3Int(min.x + x, max.y, 0));
            Gizmos.DrawLine(start, end);
        }
    }

    private void DrawGridLines()
    {
        // Fallback: Draw using GL if not using mesh renderer
        if (lineMaterial == null)
            CreateLineMaterial();

        GL.PushMatrix();
        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(gridColor);

        Vector2Int halfSize = gridSize / 2;
        Vector2Int min = gridCenter - halfSize;
        Vector2Int max = gridCenter + halfSize;

        // Horizontal lines
        for (int y = 0; y <= gridSize.y; y++)
        {
            Vector3 start = grid.CellToWorld(new Vector3Int(min.x, min.y + y, 0));
            Vector3 end = grid.CellToWorld(new Vector3Int(max.x, min.y + y, 0));
            GL.Vertex(start);
            GL.Vertex(end);
        }

        // Vertical lines
        for (int x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = grid.CellToWorld(new Vector3Int(min.x + x, min.y, 0));
            Vector3 end = grid.CellToWorld(new Vector3Int(min.x + x, max.y, 0));
            GL.Vertex(start);
            GL.Vertex(end);
        }

        GL.End();
        GL.PopMatrix();
    }

    private void OnValidate()
    {
        // Update mesh when values change in inspector
        if (Application.isPlaying && useMeshRenderer && meshFilter != null)
        {
            UpdateGridMesh();
            if (meshRenderer != null && lineMaterial != null)
                lineMaterial.color = gridColor;
        }
    }

    // Public methods for runtime control
    public void SetGridVisible(bool visible)
    {
        showInPlayMode = visible;
        if (gridMeshObject != null)
            gridMeshObject.SetActive(visible);
    }

    public void SetGridColor(Color color)
    {
        gridColor = color;
        if (lineMaterial != null)
            lineMaterial.color = color;
    }

    public void SetGridSize(Vector2Int size)
    {
        gridSize = size;
        if (useMeshRenderer)
            UpdateGridMesh();
    }

    public void SetGridCenter(Vector2Int center)
    {
        gridCenter = center;
        if (useMeshRenderer)
            UpdateGridMesh();
    }
}
