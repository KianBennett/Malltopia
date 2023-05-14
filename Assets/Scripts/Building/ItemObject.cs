using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class ItemObject : WorldObject
{
    public enum ObjectType { Default, Decoration, Trash }

    [SerializeField] private SpriteRenderer directionIndicator;
    [SerializeField] private GameObject[] tempOnlyObjects;

    [SerializeField] private ObjectType type;
    [SerializeField] private Vector2Int gridSize = new(1, 1);

    [Header("Placement Rules")]
    [SerializeField] private Direction[] DirectionsRequiringWall;
    [SerializeField] private Direction[] DirectionsRequiringSpace;

    [HideInInspector] public Cell cell;

    public ObjectType ItemObjectType { get { return type; } }

    protected override void Update()
    {
        base.Update();

        // Rotate object towards direction facing
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler((int) directionFacing * 90 * Vector3.up), Time.deltaTime * 400);
    }

    public override void Init()
    {
        base.Init();

        // Assign this object to all occupied cells and remove trash
        foreach (Cell cell in GetOccupiedCells(cell, directionFacing, out _))
        {
            if (cell != null)
            {
                cell.itemObject = this;
                if (cell.trashObject)
                {
                    TrashManager.Instance.RemoveTrashObjectFromCell(cell);
                }
            }
        }

        Mall.CurrentFloor.RegisterObject(this);
        Mall.CurrentFloor.RebuildNavMesh();

        if(directionIndicator)
        {
            directionIndicator.gameObject.SetActive(false);
        }
        foreach(GameObject tempOnlyObjects in tempOnlyObjects)
        {
            tempOnlyObjects.SetActive(false);
        }
    }

    public override void SetAsTempObject(bool temp, bool valid = true)
    {
        base.SetAsTempObject(temp, valid);

        if(directionIndicator)
        {
            directionIndicator.gameObject.SetActive(temp);

            if (temp)
            {
                directionIndicator.color = valid ? MallGridVisualiser.Instance.CellColourValid : MallGridVisualiser.Instance.CellColourInvalid;
            }
        }
    }

    public bool IsCellValid(Cell cell, Direction direction)
    {
        foreach(Direction relativeDirection in DirectionsRequiringWall)
        {
            Direction worldDirection = Mall.GetDirectionRelativeToDirection(relativeDirection, direction);

            if (cell.GetEdge(worldDirection).buildingPieceObject == null && cell.GetNeighbour(worldDirection) != null)
            {
                return false;
            }
        }

        foreach(Direction relativeDirection in DirectionsRequiringSpace)
        {
            Direction worldDirection = Mall.GetDirectionRelativeToDirection(relativeDirection, direction);
            Cell neighbour = cell.GetNeighbour(worldDirection);

            if (cell.GetEdge(worldDirection).buildingPieceObject != null || neighbour == null || neighbour.itemObject != null)
            {
                return false;
            }
        }

        return true;
    }

    public Cell[] GetOccupiedCells(Cell originCell, Direction direction, out bool blockedByWall)
    {
        List<Cell> cells = new();

        Direction forward = Mall.GetDirectionRelativeToDirection(Direction.North, direction);
        Direction right = Mall.GetDirectionRelativeToDirection(Direction.East, direction);

        blockedByWall = false;

        for (int i = 0; i < gridSize.x; i++)
        {
            for(int j = 0; j < gridSize.y; j++)
            {
                Vector2 localCoord = new(i, j);
                localCoord = localCoord.Rotate(-90 * (int) direction);

                Vector2Int globalCoord = originCell.Coords + localCoord.ToVector2Int();

                Cell c = originCell.floor.GetCell(globalCoord);

                if (c == null) continue;

                // For all cells but those at the edges (i.e. cells with another occupying cell ahead of it),
                // check if there's a wall in the way
                if((i < gridSize.x - 1 && c.GetEdge(right)?.buildingPieceObject != null) || 
                    (j < gridSize.y - 1 && c.GetEdge(forward)?.buildingPieceObject != null))
                {
                    blockedByWall = true;
                }

                cells.Add(c);
            }
        }

        return cells.ToArray();
    }

    public Cell[] GetSurroundingCells(bool includeDiagonals = true)
    {
        List<Cell> cells = new();

        Direction forward = Mall.GetDirectionRelativeToDirection(Direction.North, DirectionFacing);
        Direction right = Mall.GetDirectionRelativeToDirection(Direction.East, DirectionFacing);

        for (int i = -1; i < gridSize.x + 1; i++)
        {
            for (int j = -1; j < gridSize.y + 1; j++)
            {
                if(!includeDiagonals)
                {
                    bool isDiagonal = (i < 0 && j < 0) || (i < 0 && j == gridSize.y) || (i == gridSize.x && j == gridSize.y) || (i == gridSize.x && j < 0);
                    if(isDiagonal)
                    {
                        continue;
                    }
                }

                Vector2 localCoord = new(i, j);
                localCoord = localCoord.Rotate(-90 * (int)DirectionFacing);

                Vector2Int globalCoord = cell.Coords + localCoord.ToVector2Int();

                Cell c = cell.floor.GetCell(globalCoord);

                cells.Add(c);
            }
        }

        return cells.ToArray();
    }
    
    public Cell[] GetCellsRequiringSpace()
    {
        List<Cell> cells = new();

        foreach (Direction relativeDirection in DirectionsRequiringSpace)
        {
            Direction worldDirection = Mall.GetDirectionRelativeToDirection(relativeDirection, directionFacing);
            Cell neighbour = cell.GetNeighbour(worldDirection);
            cells.Add(neighbour);
        }

        return cells.ToArray();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Mall.CurrentFloor.UnregisterObject(this);
        foreach(Cell cell in GetOccupiedCells(cell, directionFacing, out bool blockedByWall))
        {
            if(cell != null)
            {
                cell.itemObject = null;
            }
        }

        // Restore 75% of the cost of the item
        if(PlayerController.Instance) PlayerController.Instance.AddMoney((int) (Cost * 0.75f));
    }
}