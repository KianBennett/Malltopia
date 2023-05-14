using System.Linq;
using UnityEngine;

public class Cell
{
    public int i, j;
    public CellArea cellArea;
    public Floor floor;
    public Room room;
    public CellEdge[] edges;
    public Cell[] neighbours;
    public ItemObject itemObject;
    public ItemObjectTrash trashObject;
    public int customerCount;

    public Vector2Int Coords { get { return new Vector2Int(i, j); } }

    public Cell(int i, int j, CellArea cellArea, Floor floor)
    {
        this.i = i;
        this.j = j;
        this.cellArea = cellArea;
        room = null;
        edges = new CellEdge[4];
        neighbours = new Cell[4];
        this.floor = floor;
    }

    public Cell GetNeighbour(Direction direction)
    {
        return neighbours[(int) direction];
    }

    public CellEdge GetEdge(Direction direction)
    {
        return edges[(int) direction];
    }

    public Vector3 GetCentrePosition()
    {
        float halfCellSize = Mall.Instance.CellSizeIncludingPadding / 2.0f;
        return Mall.Instance.CellCoordToWorldPos(i, j) + new Vector3(halfCellSize, 0, halfCellSize);
    }

    public Vector3 GetRandomPosition()
    {
        float halfCellSize = Mall.Instance.CellSizeIncludingPadding / 2.0f;
        float offsetX = (halfCellSize * Random.Range(0.5f, 1.5f));
        float offsetZ = (halfCellSize * Random.Range(0.5f, 1.5f));
        return Mall.Instance.CellCoordToWorldPos(i, j) + new Vector3(offsetX, 0, offsetZ);
    }
    
    public bool IsCellValidForRoomBuilding()
    {
        bool nextToRoomEntrance = edges.Any(o => o.buildingPieceObject != null && o.buildingPieceObject is BuildingPieceEntrance);
        bool nextToMallEntrance = Mall.Instance.IsCellInEntranceArea(this);

        return room == null && !itemObject && !nextToRoomEntrance && !nextToMallEntrance;
    }
}