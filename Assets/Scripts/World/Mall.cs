using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public enum Direction { North, East, South, West }

public class Mall : Singleton<Mall>
{
    [SerializeField] private float cellSize, cellPadding;
    [SerializeField] private int cellDivisions;
    [SerializeField] private Floor[] floors;
    [SerializeField] private BoxCollider customerSpawnArea;
    [SerializeField] private CellAreaDimensions entranceCells;

    private Floor currentFloor;

    public float CellSize { get { return cellSize; } }
    public float CellPadding { get { return cellPadding; } }
    public float CellSizeIncludingPadding { get { return cellSize + cellPadding * 2; } }
    public int CellDivisions { get { return cellDivisions; } }
    public BoxCollider CustomerSpawnArea { get { return customerSpawnArea; } }

    public static Floor CurrentFloor { get { return Instance?.currentFloor; } }
    public static Floor GroundFloor { get { return Instance?.floors[0]; } }

    protected override void Awake()
    {
        base.Awake();

        if (floors.Length > 0)
        {
            currentFloor = floors[0];
        }
    }

    public Vector3 CellCoordToWorldPos(int i, int j)
    {
        return new Vector3(i * CellSizeIncludingPadding, 0, j * CellSizeIncludingPadding);
    }

    public Vector3 CellCoordToWorldPos(Vector2Int coord)
    {
        return new Vector3(coord.x * CellSizeIncludingPadding, 0, coord.y * CellSizeIncludingPadding);
    }

    public Vector2Int CellCoordsFromWorldPos(Vector3 worldPos)
    {
        int i = Mathf.FloorToInt(worldPos.x / CellSizeIncludingPadding);
        int j = Mathf.FloorToInt(worldPos.z / CellSizeIncludingPadding);

        return new Vector2Int(i, j);
    }

    public Vector2Int GetMousePositionCellCoords()
    {
        CameraController.Instance.GetMousePointOnGround(out Vector3 mousePoint);
        return CellCoordsFromWorldPos(mousePoint);
    }

    public bool IsCellInEntranceArea(Cell cell)
    {
        return cell.i >= entranceCells.originPos.x && cell.i < entranceCells.originPos.x + entranceCells.size.x &&
            cell.j >= entranceCells.originPos.y && cell.j < entranceCells.originPos.y + entranceCells.size.y;
    }

    public static Vector2Int GetCellDirectionOffset(Direction direction)
    {
        return direction switch
        {
            Direction.North => Vector2Int.up,
            Direction.East => Vector2Int.right,
            Direction.South => Vector2Int.down,
            Direction.West => Vector2Int.left,
            _ => Vector2Int.zero,
        };
    }

    public static Direction GetDirectionFromOffset(Vector2Int offset)
    {
        offset.Clamp(-Vector2Int.one, Vector2Int.one);

        // Can't use switch statement because Vector2Int.up etc isn't constant
        if (offset == Vector2Int.up) return Direction.North;
        if (offset == Vector2Int.right) return Direction.East;
        if (offset == Vector2Int.down) return Direction.South;
        if (offset == Vector2Int.left) return Direction.West;

        Debug.LogWarningFormat("Trying to get a cardinal direction from a non-cardinal offset: {0}", offset);
        return default;
    }

    public static Direction GetDirectionRelativeToDirection(Direction direction, Direction relativeTo)
    {
        return (Direction) MathUtil.Mod((int) relativeTo + (int) direction, 4);
    }

    public static Direction GetOppositeDirection(Direction direction)
    {
        return (Direction) MathUtil.Mod((int) direction + 2, 4);
    }

    public static BuildingPieceObject CreateBuildingPiece(CellEdge cellEdge, Direction direction, BuildingPieceObject prefab, Transform parent)
    {
        float halfCellSize = Instance.CellSizeIncludingPadding / 2.0f;
        Vector2Int offset2i = GetCellDirectionOffset(direction);
        Vector3 offset = new(offset2i.x * halfCellSize, 0, offset2i.y * halfCellSize);

        BuildingPieceObject buildingPiece = Instantiate(prefab);
        buildingPiece.transform.SetParent(parent);
        buildingPiece.transform.position = cellEdge.GetCentrePosition();
        buildingPiece.transform.rotation = Quaternion.Euler(Vector3.up * (Vector3.SignedAngle(Vector3.forward, offset, Vector3.up) + 180));
        buildingPiece.SetDirection(direction);
        buildingPiece.cellEdge = cellEdge;

        return buildingPiece;
    }

    public static ItemObject CreateItemObject(Cell cell, Direction direction, ItemObject prefab, Transform parent)
    {
        ItemObject item = Instantiate(prefab);
        item.transform.SetParent(parent);
        item.transform.position = cell.GetCentrePosition();
        item.transform.rotation = Quaternion.Euler((int) direction * 90 * Vector3.up);
        item.SetDirection(direction);
        item.cell = cell;

        return item;
    }

    public Vector3 GetRandomCustomerSpawnPoint()
    {
        Vector3 min = customerSpawnArea.transform.position - customerSpawnArea.size / 2;
        Vector3 max = customerSpawnArea.transform.position + customerSpawnArea.size / 2;
        float x = Random.Range(min.x, max.x);
        float z = Random.Range(min.z, max.z);

        return new Vector3(x, 0, z);
    }
}
