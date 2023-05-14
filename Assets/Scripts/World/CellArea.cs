using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[System.Serializable]
public class CellAreaDimensions
{
    public Vector2Int originPos;
    public Vector2Int size;
}

public class CellArea
{
    private CellAreaDimensions dimensions;
    private Cell[,] cells;
    private NavMeshSurface navMeshSurface;
    private MeshRenderer gridPlane;

    public CellAreaDimensions Dimensions { get { return dimensions; } }
    public NavMeshSurface Surface { get { return navMeshSurface; } }
    public MeshRenderer GridPlane { get { return gridPlane; } }

    public CellArea(CellAreaDimensions dimensions, Floor floor)
    {
        this.dimensions = dimensions;

        cells = new Cell[dimensions.size.x, dimensions.size.y];

        for(int i = 0; i < dimensions.size.x; i++)
        {
            for(int j = 0; j < dimensions.size.y; j++)
            {
                Cell cell = new(dimensions.originPos.x + i, dimensions.originPos.y + j, this, floor);
                cells[i, j] = cell;
            }
        }
    }

    public void SetNavMeshSurface(NavMeshSurface surface)
    {
        navMeshSurface = surface;
    }
    
    public void SetGridPlane(MeshRenderer gridPlane)
    {
        this.gridPlane = gridPlane;
    }

    public Cell GetCell(int i, int j)
    {
        return cells[i - dimensions.originPos.x, j - dimensions.originPos.y];
    }

    public bool IsCoordInCellArea(int i, int j)
    {
        return i >= dimensions.originPos.x && i < dimensions.originPos.x + dimensions.size.x &&
            j >= dimensions.originPos.y && j < dimensions.originPos.y + dimensions.size.y;
    }
    
    public Cell[] GetAllCells()
    {
        List<Cell> allCells = new();

        for (int i = 0; i < dimensions.size.x; i++)
        {
            for (int j = 0; j < dimensions.size.y; j++)
            {
                allCells.Add(cells[i, j]);
            }
        }

        return allCells.ToArray();
    }

    public Vector3 GetWorldCentrePos()
    {
        return Mall.Instance.CellCoordToWorldPos(dimensions.originPos.x, dimensions.originPos.y) + GetWorldSize() * 0.5f;
    }
    
    public Vector3 GetWorldSize()
    {
        return new Vector3(dimensions.size.x, 0, dimensions.size.y) * Mall.Instance.CellSizeIncludingPadding;
    }
}
