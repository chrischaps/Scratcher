using System;
using UnityEngine;

[Serializable]
public enum GridType
{
    Isometric,
    Rectangular
}

[Serializable]
public class GridConfiguration
{
    [Header("Grid Type")] public GridType gridType = GridType.Isometric;

    [Header("Cell Settings")] public Vector3 cellSize = new(1f, 1f, 0f);

    public Vector3 cellGap = Vector3.zero;

    [Header("Movement Settings")] public bool useGridBasedMovement;

    public float movementSpeed = 5f;

    [Header("Camera Settings")] public Vector3 cameraOffset = new(0, 0, -10);

    public bool useCustomCameraAngles;

    [Header("Custom Camera Angles")] [Range(0f, 90f)]
    public float topDownAngle = 60f;

    [Range(0f, 360f)] public float topDownYRotation;
    [Range(-45f, 45f)] public float topDownZRotation;

    public GridLayout.CellLayout GetCellLayout()
    {
        return gridType == GridType.Isometric ? GridLayout.CellLayout.Isometric : GridLayout.CellLayout.Rectangle;
    }

    public GridLayout.CellSwizzle GetCellSwizzle()
    {
        return GridLayout.CellSwizzle.XYZ;
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