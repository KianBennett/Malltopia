using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private MeshRenderer gridPlanePrefab;

    [Header("Containers")]
    [SerializeField] private Transform roomContainer;
    [SerializeField] private Transform objectsContainer;
    [SerializeField] private Transform wallPiecesContainer;
    [SerializeField] private Transform fixedCharactersContainer;
    [SerializeField] private Transform gridPlaneContainer;

    [Header("Layout")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private CellAreaDimensions[] cellAreaDimensions;

    [Header("Texture")]
    [SerializeField] private Material floorMaterial;
    [SerializeField] private TextureSet defaultFloorTextureSet;

    private List<Cell> cells;
    private List<Room> rooms;
    private List<ItemObject> objects;
    private List<CellArea> cellAreas;
    private List<Character> characters;

    private Vector2Int coordMin, coordMax;

    //private List<TextureSet> allFloorTextureSets;
    private Texture2DArray mainTextureArray;
    private Texture2DArray normalTextureArray;
    private Texture2DArray roughnessTextureArray;
    private Texture2DArray specularTextureArray;

    private System.Diagnostics.Stopwatch stopwatch;

    public Cell[] Cells { get { return cells.ToArray(); } }
    public Character[] Characters { get { return characters.ToArray(); } }
    public Transform ObjectsContainer { get { return objectsContainer; } }
    public Transform WallPiecesContainer { get { return wallPiecesContainer; } }
    public Transform FixedCharactersContainers { get { return fixedCharactersContainer; } }

    // Global toggle for generating navmesh so we can place a large number of rooms/objects without generating a new navmesh every time
    public static bool NavMeshGenerationEnabled = true;

    void Awake()
    {
        cells = new();
        rooms = new();
        objects = new();
        cellAreas = new();
        characters = new();
        coordMin = new();
        coordMax = new();
        stopwatch = new();

        floorMaterial.SetFloat("_CellSize", Mall.Instance.CellSize);
        floorMaterial.SetFloat("_CellPadding", Mall.Instance.CellPadding);

        // Create cell areas from dimensions
        foreach (CellAreaDimensions dimensions in cellAreaDimensions)
        {
            CellArea cellArea = new(dimensions, this);
            cells.AddRange(cellArea.GetAllCells());
            cellAreas.Add(cellArea);

            // Create grid plane
            MeshRenderer gridPlaneMesh = Instantiate(gridPlanePrefab, cellArea.GetWorldCentrePos() + Vector3.up * 0.01f, Quaternion.identity, gridPlaneContainer);
            Vector3 gridPlaneScale = cellArea.GetWorldSize() / 10.0f + Vector3.up * 1.0f;
            gridPlaneMesh.transform.localScale = new Vector3(gridPlaneScale.x, 1, gridPlaneScale.z);
            cellArea.SetGridPlane(gridPlaneMesh);
        }

        foreach(Cell cell in cells)
        {
            for(int i = 0; i < 4; i++)
            {
                Direction direction = (Direction) i;
                Cell neighbourCell = GetCell(cell.Coords + Mall.GetCellDirectionOffset(direction));

                CellEdge edge = new();
                edge.neighbourCells.Add(direction, cell);
                cell.edges[i] = edge;

                if(neighbourCell != null)
                {
                    cell.neighbours[i] = neighbourCell;
                    edge.neighbourCells.Add(Mall.GetOppositeDirection(direction), neighbourCell);
                    neighbourCell.edges[(int) Mall.GetOppositeDirection(direction)] = edge;
                }
            }

            if (cell.i < coordMin.x) coordMin.x = cell.i;
            if (cell.j < coordMin.y) coordMin.y = cell.j;
            if (cell.i > coordMax.x) coordMax.x = cell.i;
            if (cell.j > coordMax.y) coordMax.y = cell.j;
        }

        RebuildNavMesh();
        initFloorTextureArrays();
        UpdateFloorMaterial();
    }

    public void RebuildNavMesh()
    {
        if (!NavMeshGenerationEnabled) return;

        // Building the navmesh is a potentially perf-sensitive operation that will increase as the game world expands
        // So it's a good idea to keep an eye on the time it takes
        stopwatch.Restart();

        navMeshSurface.BuildNavMesh();

        // Recalculate paths for all characters, might need optimising to only reculculate necessary paths
        foreach(Character character in characters)
        {
            if(character)
            {
                character.Movement.RecalculateCurrentPath();
            }
        }

        stopwatch.Stop();

        int totalObjectCount = objects.Count;
        foreach(Room room in rooms)
        {
            totalObjectCount += room.FindItemObjects<ItemObject>().Length;
            totalObjectCount += room.FindBuildingPieceObjects<BuildingPieceObject>().Length;
        }
        Debug.LogFormat("Rebuilding navmesh for {0} objects ({1}ms)", totalObjectCount, stopwatch.ElapsedMilliseconds);
    }

    private void initFloorTextureArrays()
    {
        TextureSet[] allFloorTextureSets = BuildingSystem.Instance.FloorTextureSets;

        Vector2Int textureSize = new(defaultFloorTextureSet.mainTexture.width, defaultFloorTextureSet.mainTexture.height);
        TextureFormat normalFormat = defaultFloorTextureSet.normals.format;

        mainTextureArray = new(textureSize.x, textureSize.y, allFloorTextureSets.Length, TextureFormat.RGBA32, true, false);
        normalTextureArray = new(textureSize.x, textureSize.y, allFloorTextureSets.Length, TextureFormat.RGBA32, true, true);
        roughnessTextureArray = new(textureSize.x, textureSize.y, allFloorTextureSets.Length, TextureFormat.RGBA32, true, false);
        specularTextureArray = new(textureSize.x, textureSize.y, allFloorTextureSets.Length, TextureFormat.RGBA32, true, false);

        //mainTextureArray.anisoLevel = 4;

        for (int i = 0; i < allFloorTextureSets.Length; i++)
        {
            if (allFloorTextureSets[i].mainTexture != null) mainTextureArray.SetPixels32(allFloorTextureSets[i].mainTexture.GetPixels32(0), i, 0);
            if (allFloorTextureSets[i].normals != null) normalTextureArray.SetPixels32(allFloorTextureSets[i].normals.GetPixels32(0), i, 0);
            if (allFloorTextureSets[i].roughness != null) roughnessTextureArray.SetPixels32(allFloorTextureSets[i].roughness.GetPixels32(0), i, 0);
            if (allFloorTextureSets[i].specular != null) specularTextureArray.SetPixels32(allFloorTextureSets[i].specular.GetPixels32(0), i, 0);
        }

        mainTextureArray.Apply();
        normalTextureArray.Apply();
        roughnessTextureArray.Apply();
        specularTextureArray.Apply();

        floorMaterial.SetTexture("_MainTextures", mainTextureArray);
        floorMaterial.SetTexture("_NormalTextures", normalTextureArray);
        floorMaterial.SetTexture("_RoughnessTextures", roughnessTextureArray);
        floorMaterial.SetTexture("_SpecularTextures", specularTextureArray);
        floorMaterial.SetFloat("_TextureCount", allFloorTextureSets.Length);
    }

    public void UpdateFloorMaterial()
    {
        Vector2Int floorSize = GetFloorCellSize();
        Texture2D cellsTexture = new(floorSize.x, floorSize.y);

        Color32[] colours = cellsTexture.GetPixels32(0);

        TextureSet[] allFloorTextureSets = BuildingSystem.Instance.FloorTextureSets;

        for (int i = 0; i < cellsTexture.width; i++)
        {
            for (int j = 0; j < cellsTexture.height; j++)
            {
                int index = j * cellsTexture.width + i;
                Cell cell = GetCell(i, j);

                if(cell != null)
                {
                    if (cell.room != null)
                    {
                        int textureSetIndex = Array.IndexOf(allFloorTextureSets, cell.room.FloorTextureSet);
                        // alpha channel can't go above 1 so we need to represent the index as a fraction <1 instead
                        float textureSetIndexAsPercentage = (float) textureSetIndex / allFloorTextureSets.Length;
                        //cellsTexture.SetPixel(i, j, new Color(1, 1, 1, textureSetIndex * 0.1f));
                        colours[index] = new Color(1, 1, 1, textureSetIndexAsPercentage);
                    }
                    else
                    {
                        //cellsTexture.SetPixel(i, j, new Color(1, 1, 1, 0));
                        colours[index] = new Color(1, 1, 1, 0);
                    }
                }
            }
        }

        cellsTexture.SetPixels32(colours);
        cellsTexture.Apply();

        floorMaterial.SetTexture("_CellsTexture", cellsTexture);
    }

    public Cell GetCell(int i, int j)
    {
        foreach(CellArea cellArea in cellAreas)
        {
            if(cellArea.IsCoordInCellArea(i, j))
            {
                return cellArea.GetCell(i, j);
            }
        }
        return null;
    }

    public Cell GetCell(Vector2Int coord)
    {
        return GetCell(coord.x, coord.y);
    }

    public Cell GetCellFromWorldPos(Vector3 worldPos)
    {
        return GetCell(Mall.Instance.CellCoordsFromWorldPos(worldPos));
    }

    public Vector2Int GetFloorCellSize()
    {
        return (coordMax - coordMin + Vector2Int.one).Abs();
    }

    public Room GetRoomForCell(int i, int j)
    {
        return GetCell(i, j)?.room;
    }

    public Cell[] GetEmptyCellsNotInRoom()
    {
        List<Cell> emptyCells = new();

        foreach(Cell cell in cells)
        {
            if(!cell.itemObject && cell.room == null)
            {
                emptyCells.Add(cell);
            }
        }

        return emptyCells.ToArray();
    }

    public List<Cell> GetCellsFromCorners(Vector2Int cornerA, Vector2Int cornerB)
    {
        List<Cell> cells = new();

        Vector2Int min = new(Mathf.Min(cornerA.x, cornerB.x), Mathf.Min(cornerA.y, cornerB.y));
        Vector2Int max = new(Mathf.Max(cornerA.x, cornerB.x), Mathf.Max(cornerA.y, cornerB.y));

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                Cell cell = GetCell(i, j);
                cells.Add(cell);
            }
        }

        return cells;
    }

    public List<Cell> GetCellsFromCellToMousePos(Cell startCell)
    {
        return GetCellsFromCorners(startCell.Coords, Mall.Instance.GetMousePositionCellCoords());
    }

    // Gets the adjacent edge that is still in the same room
    public CellEdge GetRoomEdgeAdjacentToCellEdge(CellEdge edge, Room room, Direction directionFromRoomCell, bool right, int distance = 1)
    {
        if (distance == 0)
        {
            Debug.LogWarning("Distance is 0, are you sure?");
            return edge;
        }

        if (edge.neighbourCells.ContainsKey(directionFromRoomCell))
        {
            Cell roomCell = edge.neighbourCells[directionFromRoomCell];
            Vector2Int offset = Mall.GetCellDirectionOffset((Direction) MathUtil.Mod((int) directionFromRoomCell + (right ? -1 : 1), 4));
            Cell cell = GetCell(roomCell.i + (offset.x * distance), roomCell.j + (offset.y * distance));

            if (cell != null && cell.room == room)
            {
                CellEdge adjacentEdge = cell.GetEdge(directionFromRoomCell);
                if(adjacentEdge.IsAtEdgeOfRoom(room))
                {
                    return adjacentEdge;
                }
            }
        }

        return null;
    }

    public Room BuildRoom(RoomBuildingParams roomParams, List<Cell> roomCells)
    {
        Room room = new GameObject(roomParams.Name).AddComponent<Room>();
        room.transform.SetParent(roomContainer, true);
        room.Init(roomCells, this, roomParams);

        foreach (Cell cell in roomCells)
        {
            cell.room = room;

            for (int i = 0; i < 4; i++)
            {
                Direction direction = (Direction) i;

                CellEdge edge = cell.GetEdge(direction);
                if (edge == null) continue;

                Vector2Int offset = Mall.GetCellDirectionOffset(direction);
                Vector2Int neighbourCoords = cell.Coords + offset;

                Cell neighbourCell = GetCell(neighbourCoords);

                // If the neighbour cell in that direction is outside of the room the cell is in (but still in the mall), there is a wall there
                if (neighbourCell != null && roomCells.Where(o => o.Coords == neighbourCoords).Count() == 0)
                {
                    BuildingPieceObject wallPiece = AddBuildingPieceToRoom(edge, direction, BuildingSystem.Instance.WallPiecePrefab, room);
                    wallPiece.UpdateWallMaterials();

                    if (edge.buildingPieceObject != null)
                    {
                        Destroy(edge.buildingPieceObject.gameObject);
                    }
                    edge.buildingPieceObject = wallPiece;
                }
            }

            if(cell.trashObject)
            {
                TrashManager.Instance.RemoveTrashObjectFromCell(cell);
            }
        }

        RebuildNavMesh();

        return room;
    }

    public void RegisterRoom(Room room)
    {
        rooms.Add(room);
        UpdateFloorMaterial();
    }

    public void UnregisterRoom(Room room)
    {
        rooms.Remove(room);
    }

    public void RegisterObject(ItemObject itemObject)
    {
        objects.Add(itemObject);
    }

    public void UnregisterObject(ItemObject itemObject)
    {
        objects.Remove(itemObject);
    }
    
    public void RegisterCharacter(Character character)
    {
        characters.Add(character);
    }

    public void UnregisterCharacter(Character character)
    {
        characters.Remove(character);
    }

    public BuildingPieceObject AddBuildingPieceToRoom(CellEdge cellEdge, Direction direction, BuildingPieceObject prefab, Room room)
    {
        return Mall.CreateBuildingPiece(cellEdge, direction, prefab, wallPiecesContainer);
    }

    public ItemObject AddItemInRoom(Cell cell, Direction direction, ItemObject prefab, Room room)
    {
        return Mall.CreateItemObject(cell, direction, prefab, objectsContainer);
    }

    public ItemObject AddItem(Cell cell, Direction direction, ItemObject prefab)
    {
        return Mall.CreateItemObject(cell, direction, prefab, objectsContainer);
    }

    public Room[] FindRoomsOfType(RoomBuildingParams roomParams)
    {
        return rooms.Where(o => o.RoomParams == roomParams).ToArray();
    }

    public Room FindNearestRoomOfType(RoomBuildingParams roomParams, Vector3 origin)
    {
        return FindRoomsOfType(roomParams)
            .OrderBy(o => Vector3.Distance(o.FindNearestEntrance(origin).cellEdge.GetCentrePosition(), origin))
            .FirstOrDefault();
    }

    public Room[] FindShops()
    {
        return rooms.Where(o => o.RoomParams is ShopParams).ToArray();
    }

    public T[] FindItemObjectsOfType<T>() where T : ItemObject
    {
        return objects.Where(o => o is T).Select(o => o as T).ToArray();
    }

    public T[] FindItemObjectsInRange<T>(Vector3 origin, float range, ItemObject.ObjectType filter) where T : ItemObject
    {
        return FindItemObjectsOfType<T>()
            .Where(o => Vector3.Distance(origin, o.cell.GetCentrePosition()) <= range && o.ItemObjectType == filter)
            .ToArray();
    }

    // This isn't very efficient so avoid using multiple times per frame
    public BuildingPieceObject[] FindBuildingObjectsInRange(Vector3 origin, float range, params BuildingPieceObject.PieceType[] filters)
    {
        List<BuildingPieceObject> buildingObjects = new();
        foreach (Room room in rooms)
        {
            buildingObjects.AddRange(room.FindBuildingPieceObjects<BuildingPieceObject>().Where(o => Vector3.Distance(origin, o.cellEdge.GetCentrePosition()) <= range && filters.Contains(o.type)));
        }
        return buildingObjects.ToArray();
    }

    public T FindNearestObjectOfType<T>(Vector3 origin) where T : ItemObject
    {
        return FindItemObjectsOfType<T>()
            .OrderBy(o => Vector3.Distance(origin, o.cell.GetCentrePosition()))
            .FirstOrDefault();
    }

    public ItemObjectSeat.SittingPoint FindNearestVacantSittingPoint(Vector3 origin)
    {
        return FindItemObjectsOfType<ItemObjectSeat>()
            .Where(o => o.HasVacantSittingPoint() && (o.cell.room == null || !o.cell.room.RoomParams.staffOnly))
            .Select(o => o.GetNearestUnnocupiedSittingPoint(origin))
            .OrderBy(o => Vector3.Distance(origin, o.GetSeatCell().GetCentrePosition()))
            .FirstOrDefault();
    }

    public Cell[] GetEmptyCells()
    {
        return cells.Where(o => o.itemObject == null).ToArray();
    }
}
