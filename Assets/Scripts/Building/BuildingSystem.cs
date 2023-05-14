using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSystem : Singleton<BuildingSystem>
{
    public enum BuildingStage { None, ChooseStartCell, ChooseEndCell, PlaceRoomObjects, PlaceSingleObject }

    [SerializeField] private Transform tempBuildingObjectsParent;
    [SerializeField] private RoomBuildingParams[] roomParams;
    [SerializeField] private WorldObject[] commonAreaItemObjects;
    [SerializeField] private TextureSet[] floorTextureSets;
    [SerializeField] private WallMaterialSet[] wallMaterialSets;
    [SerializeField] private BuildingPieceObject wallPiecePrefab;
    [SerializeField] private WorldObjectList worldObjectList;

    private BuildingStage stage;
    private int selectedRoomParamsIndex;
    private Room currentRoom;
    private int roomObjectToPlace;
    private WorldObject singleObjectToPlace;
    private Direction objectDirection;
    private bool selectionIsValid;
    private ItemObject itemTempObject;

    private Cell startCell, currentCell;
    List<Tuple<Cell, bool>> cellsToFill;
    private WorldObject mousedOverObject;

    private List<WorldObject> tempObjects;
    private List<WorldObject> hiddenObjects;

    public RoomBuildingParams[] Rooms { get { return roomParams; } }
    public WorldObject[] CommonAreaObjects { get { return commonAreaItemObjects; } }
    public RoomBuildingParams SelectedRoomParams { get { return roomParams[selectedRoomParamsIndex]; } }
    public WorldObject SelectedSingleObject { get { return singleObjectToPlace; } }
    public BuildingStage Stage { get { return stage; } }
    public TextureSet[] FloorTextureSets { get { return floorTextureSets; } }
    public WallMaterialSet[] WallMaterialSets { get { return wallMaterialSets; } }
    public BuildingPieceObject WallPiecePrefab { get { return wallPiecePrefab; } }
    public WorldObjectList WorldObjectList { get { return worldObjectList; } }

    [SerializeField] private GameObject BuildingCostTooltip;

    protected override void Awake()
    {
        base.Awake();

        cellsToFill = new();
        tempObjects = new();
        hiddenObjects = new();
    }

    void Update()
    {
        if (PlayerController.Instance && PlayerController.Instance.PlayerMode != PlayerMode.Build) return;

        Vector2Int mousePosCell = Mall.Instance.GetMousePositionCellCoords();

        Cell newCurrentCell = Mall.CurrentFloor.GetCell(mousePosCell);
        bool currentCellHasChanged = newCurrentCell != currentCell;
        currentCell = newCurrentCell;

        bool currentCellIsValid = currentCell != null && currentCell.IsCellValidForRoomBuilding();

        int cellsToFillLastCount = cellsToFill.Count;
        cellsToFill.Clear();

        switch(stage)
        {
            case BuildingStage.ChooseStartCell:

                // Fill in the current grid cell
                if (currentCell != null)
                {
                    cellsToFill.Add(new(currentCell, currentCellIsValid));
                }

                if(currentCellHasChanged)
                {
                    updateTempRoomWallObjects();
                }

                // Clicking on a valid cell starts building that room which can no longer be changed
                if (Input.GetMouseButtonDown(0) && 
                        currentCellIsValid && 
                        !EventSystem.current.IsPointerOverGameObject() /*&& 
                        PlayerController.Instance.RemoveMoney(SelectedRoomParams.Cost)*/)
                {
                    startCell = currentCell;
                    ChangeBuildingStage(BuildingStage.ChooseEndCell);
                }

                break;

            case BuildingStage.ChooseEndCell:

                if (startCell == null) break;

                List<Cell> cells = Mall.CurrentFloor.GetCellsFromCellToMousePos(startCell);

                // If one cell is invalid (e.g. inside another room) then all cells are invalid
                bool allCellsAreValid = true;
                foreach(var cell in cells)
                {
                    if(cell == null || !cell.IsCellValidForRoomBuilding()) allCellsAreValid = false;
                }

                // If either of the room dimensions are less than the minimumn required size, the room is invalid
                bool tooSmall = false;
                if(currentCell != null)
                {
                    int width = Mathf.Abs(currentCell.i - startCell.i);
                    int height = Mathf.Abs(currentCell.j - startCell.j);
                    if(width < SelectedRoomParams.MinSize.x - 1 || height < SelectedRoomParams.MinSize.y - 1)
                    {
                        tooSmall = true;
                        allCellsAreValid = false;
                    }
                }
                // If no cell is highlighted (i.e. the mouse cursor is outside the bounds of the mall) the room is invalid
                // We do this check here instead of earlier so the highlighted cells don't dissapear every time the
                // cursor leaves the mall
                else
                {
                    allCellsAreValid = false;
                }

                int totalCost = SelectedRoomParams.Cost + SelectedRoomParams.CostPerTile * cells.Count;

                if (totalCost > PlayerController.Instance.Money) allCellsAreValid = false;

                foreach (Cell cell in cells)
                {
                    if(cell != null)
                    {
                        cellsToFill.Add(new(cell, allCellsAreValid));
                    }
                }

                // Updating the temp wall objects requires destroying/spawning several objects in the same frame, so we only
                // want to do this when absolutely necessary to avoid perf hits
                if (currentCellHasChanged || cellsToFill.Count != cellsToFillLastCount)
                {
                    updateTempRoomWallObjects();
                }

                HUD.Instance.RoomBuildStatusPanel.UpdateStatus(totalCost, tooSmall);

                // Clicking on a cell when all cells are valid creates the room and progresses to object placement
                if (allCellsAreValid && 
                    InputHandler.Instance.WasPressedThisFrame("Select") && 
                    !EventSystem.current.IsPointerOverGameObject() && 
                    (roomParams[selectedRoomParamsIndex].Cost + (roomParams[selectedRoomParamsIndex].CostPerTile * cells.Count)) < PlayerController.Instance.Money)
                {
                    currentRoom = Mall.CurrentFloor.BuildRoom(SelectedRoomParams, cellsToFill.Select(o => o.Item1).ToList());
                    ChangeBuildingStage(BuildingStage.PlaceRoomObjects);
                    PlayerController.Instance.RemoveMoney(currentRoom.RoomParams.Cost + (currentRoom.RoomParams.CostPerTile * cells.Count));
                    AkSoundEngine.PostEvent("placeWalls", gameObject);
                }

                break;
                
            case BuildingStage.PlaceRoomObjects:

                if(currentRoom == null || roomObjectToPlace > SelectedRoomParams.ObjectsToPlace.Length - 1) 
                {
                    break;
                }

                WorldObject objectPrefab = SelectedRoomParams.ObjectsToPlace[roomObjectToPlace];
                if(objectPrefab is BuildingPieceObject) updateBuildingPiecePlacement(objectPrefab as BuildingPieceObject, true, goToNextItem);
                if(objectPrefab is ItemObject) updateItemObjectPlacement(objectPrefab as ItemObject, currentCellHasChanged, true, false, goToNextItem);

                break;

            case BuildingStage.PlaceSingleObject:

                if (singleObjectToPlace == null)
                {
                    break;
                }

                //objectPrefab = singleObjectPrefabs[singleObjectToPlace];
                objectPrefab = singleObjectToPlace;
                if (objectPrefab is BuildingPieceObject) updateBuildingPiecePlacement(objectPrefab as BuildingPieceObject, false, onPlaceSingleItem);
                if (objectPrefab is ItemObject) updateItemObjectPlacement(objectPrefab as ItemObject, currentCellHasChanged, currentRoom != null, true, onPlaceSingleItem);

                break;
        }

        MallGridVisualiser.Instance.AddCellsToFill(cellsToFill);
    }

    public void ChangeBuildingStage(BuildingStage newStage)
    {
        stage = newStage;

        switch(stage)
        {
            case BuildingStage.ChooseStartCell:
                currentRoom = null;
                startCell = null;
                currentCell = null;
                break;
            case BuildingStage.ChooseEndCell:
                HUD.Instance.RoomBuildStatusPanel.Open(SelectedRoomParams.Name);
                break;
            case BuildingStage.PlaceRoomObjects:
                selectionIsValid = true;
                roomObjectToPlace = 0;
                objectDirection = default;
                break;
            case BuildingStage.None:
                startCell = null;
                currentCell = null;
                HUD.Instance.RoomBuildStatusPanel.Hide();
                break;
        }

        cellsToFill.Clear();
        updateTempRoomWallObjects();
        resetHiddenObjects();
    }

    public void ChangeRoom(int index)
    {
        selectedRoomParamsIndex = index;
    }

    public void ChangeSingleObject(WorldObject worldObject)
    {
        singleObjectToPlace = worldObject;
        resetHiddenObjects();
        clearTempObjects();
    }

    public void SetCurrentRoom(Room room)
    {
        currentRoom = room;
    }

    public void CancelBuilding()
    {
        if(currentRoom)
        {
            currentRoom.DestroyRoom();
        }
        ChangeBuildingStage(BuildingStage.None);
    }

    private void clearTempObjects()
    {
        foreach (WorldObject tempObject in tempObjects)
        {
            Destroy(tempObject.gameObject);
        }

        tempObjects.Clear();
        itemTempObject = null;
    }

    private void hideObject(WorldObject objectToHide)
    {
        if(!objectToHide) return;
        objectToHide.SetHidden(true);
        hiddenObjects.Add(objectToHide);
    }

    private void resetHiddenObjects()
    {
        foreach(WorldObject hiddenObject in hiddenObjects)
        {
            hiddenObject.SetHidden(false);
        }
        hiddenObjects.Clear();
    }

    // Places a building piece including additional pieces
    public void PlaceFullBuildingPiece(BuildingPieceObject prefab, CellEdge edge, Direction direction, Room room)
    {
        if (!prefab || edge == null) return;

        // Place connected wall pieces first
        for (int c = 0; c < edge.buildingPieceObject.connectedPiecesLeft; c++)
        {
            CellEdge adjacentEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(edge, room, direction, false, c + 1);
            placeSingleBuildingPiece(wallPiecePrefab, adjacentEdge, mousedOverObject.DirectionFacing, room);
        }

        for (int c = 0; c < edge.buildingPieceObject.connectedPiecesRight; c++)
        {
            CellEdge adjacentEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(edge, room, direction, true, c + 1);
            placeSingleBuildingPiece(wallPiecePrefab, adjacentEdge, mousedOverObject.DirectionFacing, room);
        }

        // Place the main wall piece
        placeSingleBuildingPiece(prefab, edge, direction, room);

        // Place additional wall pieces. These could replace the just-placed connected pieces but that's fine
        for (int i = 0; i < prefab.additionalPiecesLeft.Length; i++)
        {
            CellEdge adjacentEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(edge, room, direction, false, i + 1);
            placeSingleBuildingPiece(prefab.additionalPiecesLeft[i], adjacentEdge, direction, room);
        }

        for (int i = 0; i < prefab.additionalPiecesRight.Length; i++)
        {
            CellEdge adjacentEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(edge, room, direction, true, i + 1);
            placeSingleBuildingPiece(prefab.additionalPiecesRight[i], adjacentEdge, direction, room);
        }
    }

    private void placeSingleBuildingPiece(BuildingPieceObject prefab, CellEdge edge, Direction direction, Room room)
    {
        if(edge == null) 
        {
            Debug.Log("Edge is null!");
            return;
        }
        BuildingPieceObject buildingPieceObject = Mall.CurrentFloor.AddBuildingPieceToRoom(edge, direction, prefab, room);
        buildingPieceObject.UpdateWallMaterials();
        if(edge.buildingPieceObject)
        {
            Destroy(edge.buildingPieceObject.gameObject);
        }
        edge.buildingPieceObject = buildingPieceObject;
    }

    private void placeTempBuildingPiece(BuildingPieceObject prefab, CellEdge edge, Direction direction, bool isValid)
    {
        BuildingPieceObject tempWallPiece = Mall.CreateBuildingPiece(edge, direction, prefab, tempBuildingObjectsParent);
        tempWallPiece.SetAsTempObject(true, isValid);
        tempObjects.Add(tempWallPiece);
    }

    private void updateTempRoomWallObjects()
    {
        clearTempObjects();

        foreach(Tuple<Cell, bool> cellValidPair in cellsToFill)
        {
            Cell cell = cellValidPair.Item1;
            bool isValid = cellValidPair.Item2;

            for(int i = 0; i < 4; i++)
            {
                Direction direction = (Direction) i;
                Vector2Int offset = Mall.GetCellDirectionOffset(direction);

                // If the neighbour cell in that direction is outside of the room we are about to place, put a wall there
                Vector2Int neighbourCell = cell.Coords + offset;
                if(cellsToFill.Where(o => o.Item1.Coords == neighbourCell).Count() == 0)
                {
                    CellEdge edge = cell.edges[(int) direction];
                    placeTempBuildingPiece(wallPiecePrefab, edge, direction, isValid);
                }
            }
        }
    }

    private void updateBuildingPiecePlacement(BuildingPieceObject buildingPiecePrefab, bool checkRoom, Action onPlaceBuilding)
    {
        WorldObject newMousedOverObject = PlayerController.Instance.MousedOverWorldObject;
        bool mousedOverObjectHasChanged = newMousedOverObject != mousedOverObject;
        mousedOverObject = newMousedOverObject;

        if(mousedOverObjectHasChanged)
        {
            resetHiddenObjects();
            clearTempObjects();
        }

        if(mousedOverObject == null || mousedOverObject is not BuildingPieceObject)
        {
            return;
        }

        Direction direction = mousedOverObject ? newMousedOverObject.DirectionFacing : default;
        BuildingPieceObject mousedOverBuildingPiece = mousedOverObject as BuildingPieceObject;
        CellEdge mousedOverCellEdge = mousedOverBuildingPiece.cellEdge;

        // The edge the wall is on must border a cell that is inside the room we're editing
        if(checkRoom && mousedOverCellEdge.neighbourCells.Where(o => o.Value.room == currentRoom).Count() == 0)
        {
            return;
        }

        Room room = currentRoom;

        // If there's no currentRoom being edited (e.g. for placing single objects) then the room is the first neighbour cell's room
        if(currentRoom == null)
        {
            Cell firstNeighbourWithRoom = mousedOverCellEdge.neighbourCells.Values.Where(o => o.room != null).FirstOrDefault();

            if(firstNeighbourWithRoom != null)
            {
                room = firstNeighbourWithRoom.room;
            }
        }

        if(mousedOverObjectHasChanged)
        {
            // When placing a wall piece, mousing over an existing wall piece will hide that piece and place a
            // transparent temporary wall piece in its place
            hideObject(mousedOverObject);

            bool isValid = true;

            // A wall piece can have additional pieces to place next to it (e.g. windows either side of a door)
            // If there is not enough space to place these additinonal pieces then the selection is invalid
            for(int i = 0; i < buildingPiecePrefab.additionalPiecesLeft.Length; i++)
            {
                CellEdge adjacentEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(
                    mousedOverCellEdge, room, direction, false, i + 1);

                if(adjacentEdge == null) isValid = false;
            }

            for(int i = 0; i < buildingPiecePrefab.additionalPiecesRight.Length; i++)
            {
                CellEdge adjacentEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(
                    mousedOverCellEdge, room, direction, true, i + 1);

                if(adjacentEdge == null) isValid = false;
            }

            // Stop players building an entrance is going to be adjanct to another room
            if(buildingPiecePrefab is BuildingPieceEntrance)
            {
                Direction oppositeDirection = Mall.GetOppositeDirection(direction);
                CellEdge adjacentEntranceEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(
                    mousedOverCellEdge, room, direction, true, 1);

                if (mousedOverCellEdge?.neighbourCells[oppositeDirection]?.room != null ||
                    adjacentEntranceEdge?.neighbourCells[oppositeDirection]?.room != null)
                {
                    isValid = false;
                }
            }

            selectionIsValid = isValid;

            placeTempBuildingPiece(buildingPiecePrefab, mousedOverCellEdge, direction, isValid);

            // A wall piece can have a number of connected pieces to its left and right. These will be replaced
            // by default wall pieces when the original wall piece is replaced (e.g. if you replace one half of a
            // 2 wide sign, the other half has to go as well).
            for(int c = 0; c < mousedOverCellEdge.buildingPieceObject.connectedPiecesLeft; c++)
            {
                CellEdge edge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(mousedOverCellEdge, room, direction, false, c + 1);
                hideObject(edge.buildingPieceObject);
                // if(c + 1 > buildingPiecePrefab.additionalPiecesLeft.Length)
                // {
                    placeTempBuildingPiece(wallPiecePrefab, edge, direction, isValid);
                // }
            }

            for(int c = 0; c < mousedOverCellEdge.buildingPieceObject.connectedPiecesRight; c++)
            {
                CellEdge edge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(mousedOverCellEdge, room, direction, true, c + 1);
                hideObject(edge.buildingPieceObject);
                // if(c + 1 > buildingPiecePrefab.additionalPiecesRight.Length)
                // {
                    placeTempBuildingPiece(wallPiecePrefab, edge, direction, isValid);
                // }
            }

            for(int i = 0; i < buildingPiecePrefab.additionalPiecesLeft.Length; i++)
            {
                CellEdge adjacentEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(
                    mousedOverCellEdge, room, direction, false, i + 1);

                if(adjacentEdge != null) 
                {
                    hideObject(adjacentEdge.buildingPieceObject);   
                    placeTempBuildingPiece(buildingPiecePrefab.additionalPiecesLeft[i], adjacentEdge, direction, isValid);
                }
            }

            for(int i = 0; i < buildingPiecePrefab.additionalPiecesRight.Length; i++)
            {
                CellEdge adjacentEdge = Mall.CurrentFloor.GetRoomEdgeAdjacentToCellEdge(
                    mousedOverCellEdge, room, direction, true, i + 1);

                if(adjacentEdge != null) 
                {
                    hideObject(adjacentEdge.buildingPieceObject);   
                    placeTempBuildingPiece(buildingPiecePrefab.additionalPiecesRight[i], adjacentEdge, direction, isValid);
                }
            }
        }

        if (InputHandler.Instance.WasPressedThisFrame("Select") &&
            selectionIsValid &&
            !EventSystem.current.IsPointerOverGameObject() &&
            PlayerController.Instance.RemoveMoney(buildingPiecePrefab.Cost))
        {
            CellEdge cellEdge = (mousedOverObject as BuildingPieceObject).cellEdge;

            if(cellEdge != null)
            {
                PlaceFullBuildingPiece(buildingPiecePrefab, cellEdge, mousedOverObject.DirectionFacing, room);
                Mall.CurrentFloor.RebuildNavMesh();
            }

            onPlaceBuilding();
            HUD.Instance.RoomBuildPanel.resetSelected();
            AkSoundEngine.PostEvent("placeWalls", gameObject);
        }
    }

    private void updateItemObjectPlacement(ItemObject itemObjectPrefab, bool currentCellHasChanged, bool checkRoom, bool checkCost, Action onPlaceItem)
    {
        if(currentCell == null) return;

        Cell[] occupiedCells = itemObjectPrefab.GetOccupiedCells(currentCell, objectDirection, out bool blockedByWall);
        bool allCellsAreValid = !blockedByWall;

        foreach (Cell cell in occupiedCells)
        {
            if(cell == null || cell.itemObject != null || !itemObjectPrefab.IsCellValid(currentCell, objectDirection) || 
                Mall.Instance.IsCellInEntranceArea(cell))
            {
                allCellsAreValid = false;
            }
            
            // Check each adjacent object and see if it requires the current cell to be empty
            for(int i = 0; i < 4; i++)
            {
                Cell neighbour = cell.GetNeighbour((Direction) i);

                if (neighbour != null && neighbour.itemObject != null)
                {
                    Cell[] cellsRequiringSpace = neighbour.itemObject.GetCellsRequiringSpace();

                    if (cellsRequiringSpace.Contains(cell))
                    {
                        allCellsAreValid = false;
                    }
                }
            }

            // Prevent items from blocking a doorway
            foreach(CellEdge edge in cell.edges)
            {
                if(edge.buildingPieceObject && edge.buildingPieceObject.type == BuildingPieceObject.PieceType.Doorway)
                {
                    allCellsAreValid = false;
                }
            }

            if (checkRoom && cell.room != currentRoom)
            {
                allCellsAreValid = false;
            }
        }

        if (itemTempObject)
        {
            itemTempObject.transform.position = Vector3.Lerp(itemTempObject.transform.position, currentCell.GetCentrePosition(), Time.unscaledDeltaTime * 15);
        }
        else if(currentCellHasChanged)
        {
            clearTempObjects();
            ItemObject itemObject = Mall.CreateItemObject(currentCell, objectDirection, itemObjectPrefab, tempBuildingObjectsParent);
            tempObjects.Add(itemObject);
            itemTempObject = itemObject;
        }

        // We do this every frame in case the direction changes
        if(itemTempObject)
        {
            itemTempObject.SetAsTempObject(true, allCellsAreValid);

            foreach(Cell cell in occupiedCells)
            {
                if(cell != null)
                {
                    cellsToFill.Add(new(cell, allCellsAreValid));
                }
            }
        }

        if(allCellsAreValid &&
            InputHandler.Instance.WasPressedThisFrame("Select") &&
            !EventSystem.current.IsPointerOverGameObject() &&
            (!checkCost || PlayerController.Instance.RemoveMoney(itemObjectPrefab.Cost)))
        {
            ItemObject itemObject;
            if (currentCell.room)
            {
                itemObject = Mall.CurrentFloor.AddItemInRoom(currentCell, objectDirection, itemObjectPrefab, currentCell.room);
            }
            else
            {
                itemObject = Mall.CurrentFloor.AddItem(currentCell, objectDirection, itemObjectPrefab);
            }
            
            HUD.Instance.RoomBuildPanel.resetSelected();
            itemObject.Init();
            itemObject.PlayPlacementSound();
            onPlaceItem();
        }

        if(InputHandler.Instance.WasPressedThisFrame("BuildIng_Rotate") && InputHandler.Instance.FindAction("BuildIng_Rotate").ReadValue<float>() == 1)
        {
            if(itemTempObject)
            {
                itemTempObject.RotateClockwise();
                objectDirection = itemTempObject.DirectionFacing;
            }
        }
        if (InputHandler.Instance.WasPressedThisFrame("BuildIng_Rotate") && InputHandler.Instance.FindAction("BuildIng_Rotate").ReadValue<float>() == -1)
        {
            if (itemTempObject)
            {
                itemTempObject.RotateAnticlockwise();
                objectDirection = itemTempObject.DirectionFacing;       
            }
        }
    }

    private void goToNextItem()
    {
        if(roomObjectToPlace < SelectedRoomParams.ObjectsToPlace.Length - 1)
        {
            resetHiddenObjects();
            clearTempObjects();
            roomObjectToPlace++;
        }
        else
        {
            currentRoom.OnBuildComplete();
            PlayerController.Instance.ExitBuildMode();
        }
    }

    private void onPlaceSingleItem()
    {
        resetHiddenObjects();
        clearTempObjects();
        //PlayerController.Instance.ExitBuildMode();
    }

#if UNITY_EDITOR

    private void OnGUI()
    {
        //if (stage != BuildingStage.None)
        //{
        //    List<Cell> cells = new List<Cell>();
        //    if(stage == BuildingStage.ChooseEndCell)
        //    {
        //     cells = Mall.CurrentFloor.GetCellsFromCellToMousePos(startCell);

        //    }

        //    GUI.Box(new Rect(50, 20, 180, 70), "Shop: " + SelectedRoomParams.Name + "\n" +
        //        "Cost: " + SelectedRoomParams.Cost + " + " + SelectedRoomParams.CostPerTile * cells.Count + " (" + (SelectedRoomParams.Cost + SelectedRoomParams.CostPerTile * cells.Count) + ")" + "\n" +
        //        "Stage: " + stage
        //        ) ;
        //}
    }

#endif

}
