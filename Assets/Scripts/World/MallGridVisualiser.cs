using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
*   A grid that shows each cell and padding for the mall floor
*/

public class MallGridVisualiser : Singleton<MallGridVisualiser>
{
    public enum VisMode { Building, CellCustomerCount }

    [Header("Parameters")]
    [SerializeField] private float subdivisionLineWidth;
    [SerializeField] private float cameraDistToShowSubdivisionLines;

    [Header("Materials")]
    [SerializeField] private Material gridMaterial;

    [Header("Colours")]
    [SerializeField] private Color cellColourValid;
    [SerializeField] private Color cellColourInvalid;
    [SerializeField] private Color cellColourHeatmapLow, cellColourHeatmapMid, cellColourHeatmapHigh;

    private VisMode visMode;
    private List<Tuple<Cell, bool>> cellsToFill; // Coordinate, IsValid
    private Texture2D cellFillTexture;
    private bool cellsToFillIsDirty;

    public Color CellColourValid => cellColourValid;
    public Color CellColourInvalid => cellColourInvalid;

    protected override void Awake()
    {
        base.Awake();
        cellsToFill = new();
    }

    void Start()
    {
        gridMaterial.SetInt("_ShowGrid", 0);
        gridMaterial.SetInt("_ShowDivisionLines", 0);
        gridMaterial.SetFloat("_CellSize", Mall.Instance.CellSize);
        gridMaterial.SetFloat("_CellPadding", Mall.Instance.CellPadding);

        Vector2Int floorSize = Mall.CurrentFloor.GetFloorCellSize();
        cellFillTexture = new(floorSize.x, floorSize.y);

        for (int i = 0; i < cellFillTexture.width; i++)
        {
            for (int j = 0; j < cellFillTexture.height; j++)
            {
                cellFillTexture.SetPixel(i, j, new Color(1, 1, 1, 0));
            }
        }
        cellFillTexture.Apply();
        gridMaterial.SetTexture("_CellFillTexture", cellFillTexture);
    }

    void Update()
    {
        switch(visMode)
        {
            case VisMode.Building:
            {
                if (!PlayerController.Instance) break;

                gridMaterial.SetInt("_ShowGrid", PlayerController.Instance.PlayerMode == PlayerMode.Build ? 1 : 0);
                gridMaterial.SetInt("_ShowDivisionLines", CameraController.Instance.GetCameraDistAbsolute() <= cameraDistToShowSubdivisionLines ? 1 : 0);

                if (cellsToFillIsDirty)
                {
                    for (int i = 0; i < cellFillTexture.width; i++)
                    {
                        for (int j = 0; j < cellFillTexture.height; j++)
                        {
                            cellFillTexture.SetPixel(i, j, new Color(1, 1, 1, 0));
                        }
                    }

                    foreach (var tile in cellsToFill)
                    {
                        Vector2Int coord = tile.Item1.Coords;
                        bool isValid = tile.Item2;

                        cellFillTexture.SetPixel(coord.x, coord.y, isValid ? cellColourValid : cellColourInvalid);
                    }
                    cellFillTexture.Apply();
                    cellsToFill.Clear();
                    cellsToFillIsDirty = false;
                }
            }
            break;

            case VisMode.CellCustomerCount:
            {
                if (!PlayerController.Instance) break;

                gridMaterial.SetInt("_ShowGrid", 1);
                gridMaterial.SetInt("_ShowDivisionLines", 0);

                for (int i = 0; i < cellFillTexture.width; i++)
                {
                    for (int j = 0; j < cellFillTexture.height; j++)
                    {
                        Cell cell = Mall.CurrentFloor.GetCell(i, j);

                        if(cell != null)
                        {
                            float intensity = Mall.CurrentFloor.GetCell(i, j).customerCount / 30.0f;

                            if (intensity > 0)
                            {
                                if (intensity < 0.5f)
                                {
                                    // White to orange
                                    cellFillTexture.SetPixel(i, j, Color.Lerp(cellColourHeatmapLow, cellColourHeatmapMid, intensity * 2));
                                }
                                else
                                {
                                    // Orange to red
                                    cellFillTexture.SetPixel(i, j, Color.Lerp(cellColourHeatmapMid, cellColourHeatmapHigh, (intensity - 0.5f) * 2));
                                }
                            }
                        }
                    }
                }

                cellFillTexture.Apply();
                cellsToFill.Clear();
            }
            break;
        }
    }

    public void AddCellsToFill(List<Tuple<Cell, bool>> cells)
    {
        cellsToFill?.AddRange(cells);
        cellsToFillIsDirty = true;
    }

    public void ToggleCellCustomerCountVisMode()
    {
        visMode = visMode != VisMode.CellCustomerCount ? VisMode.CellCustomerCount : VisMode.Building;
    }
}
