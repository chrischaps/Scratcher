using UnityEngine;

[System.Serializable]
public enum GridType
{
    Isometric,
    Rectangular
}

[System.Serializable]
public class GridConfiguration
{
    [Header("Grid Type")]
    public GridType gridType = GridType.Isometric;

    [Header("Cell Settings")]
    public Vector3 cellSize = new Vector3(1f, 1f, 0f);
    public Vector3 cellGap = Vector3.zero;

    [Header("Movement Settings")]
    public bool useGridBasedMovement = false;
    public float movementSpeed = 5f;

    [Header("Camera Settings")]
    public Vector3 cameraOffset = new Vector3(0, 0, -10);
    public bool useCustomCameraAngles = false;

    [Header("Custom Camera Angles")]
    [Range(0f, 90f)] public float topDownAngle = 60f;
    [Range(0f, 360f)] public float topDownYRotation = 0f;
    [Range(-45f, 45f)] public float topDownZRotation = 0f;

    public Grid.CellLayout GetCellLayout()
    {
        return gridType == GridType.Isometric ? Grid.CellLayout.Isometric : Grid.CellLayout.Rectangle;
    }

    public Grid.CellSwizzle GetCellSwizzle()
    {
        return Grid.CellSwizzle.XYZ;
    }

    public Vector2 ConvertInputToMovement(Vector2 input)
    {
        switch (gridType)
        {
            case GridType.Isometric:
                return new Vector2(
                    (input.x - input.y) * 0.5f,
                    (input.x + input.y) * 0.5f
                ).normalized * input.magnitude;

            case GridType.Rectangular:
            default:
                return input;
        }
    }
}