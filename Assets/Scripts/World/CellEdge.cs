using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellEdge
{
    public Dictionary<Direction, Cell> neighbourCells;
    public BuildingPieceObject buildingPieceObject;

    public CellEdge()
    {
        neighbourCells = new Dictionary<Direction, Cell>();
    }

    public Vector3 GetCentrePosition()
    {
        if (neighbourCells.Count > 0)
        {
            Direction direction = neighbourCells.First().Key;
            Cell cell = neighbourCells[direction];
            Vector2Int offset2i = Mall.GetCellDirectionOffset(direction);
            float halfCellSize = Mall.Instance.CellSizeIncludingPadding / 2.0f;
            Vector3 offset = new(offset2i.x * halfCellSize, 0, offset2i.y * halfCellSize);

            return cell.GetCentrePosition() + offset;
        }
        return Vector3.zero;
    }

    public bool IsAtEdgeOfRoom(Room room)
    {
        foreach (Direction direction in neighbourCells.Keys)
        {
            Cell cell = neighbourCells[direction];
            if (cell == null || cell.room != room)
            {
                return true;
            }
        }

        return false;
    }

    // If both neighbouring cells are in a room
    public bool IsBorderingTwoRooms()
    {
        foreach (Direction direction in neighbourCells.Keys)
        {
            Cell cell = neighbourCells[direction];
            if (cell == null || cell.room == null)
            {
                return false;
            }
        }
        return true;
    }
}