using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : Selectable
{
    public class SerializedRoomData
    {
        public List<Vector2Int> cells;
        public int floorTextureSetIndex;
        public int wallMaterialSetIndex;

        public SerializedRoomData()
        {
            cells = new();
        }
    }

    public List<Cell> cells;

    private Floor floor;
    private RoomBuildingParams roomParams;

    private TextureSet floorTextureSet;
    private WallMaterialSet wallMaterialSet;

    public List<ItemObject> RoomObjects;

    private List<Character> fixedCharacters;

    public RoomBuildingParams RoomParams { get { return roomParams; } }
    public bool IsShop { get { return roomParams is ShopParams; } }
    public TextureSet FloorTextureSet { get { return floorTextureSet; } }
    public WallMaterialSet WallMaterialSet { get { return wallMaterialSet; } }

    public void Init(List<Cell> cells, Floor floor, RoomBuildingParams roomParams)
    {
        this.cells = cells;
        this.floor = floor;
        this.roomParams = roomParams;

        fixedCharacters = new();

        floorTextureSet = roomParams.defaultFloorTextureSet;
        wallMaterialSet = roomParams.defaultWallMaterials;
    }

    public T[] FindBuildingPieceObjects<T>() where T : BuildingPieceObject
    {
        List<T> buildingPieceObjects = new();

        foreach (Cell cell in cells)
        {
            foreach (CellEdge edge in cell.edges)
            {
                if (edge.buildingPieceObject && edge.buildingPieceObject is T)
                {
                    buildingPieceObjects.Add(edge.buildingPieceObject as T);
                }
            }
        }

        return buildingPieceObjects.ToArray();
    }

    public T[] FindItemObjects<T>() where T : ItemObject
    {
        List<T> itemObjects = new();

        foreach (Cell cell in cells)
        {
            if (cell.itemObject && cell.itemObject is T)
            {
                itemObjects.Add(cell.itemObject as T);
            }
        }

        return itemObjects.ToArray();
    }

    public T FindRandomItemObject<T>() where T : ItemObject
    {
        List<T> itemObjects = new();

        foreach (Cell cell in cells)
        {
            if (cell.itemObject && cell.itemObject is T)
            {
                itemObjects.Add(cell.itemObject as T);
            }
        }

        return itemObjects[Random.Range(0, itemObjects.Count)];
    }

    public BuildingPieceObject FindNearestEntrance(Vector3 point)
    {
        return FindBuildingPieceObjects<BuildingPieceObject>()
            .Where(o => o.type == BuildingPieceObject.PieceType.Doorway)
            .OrderBy(o => Vector3.Distance(o.cellEdge.GetCentrePosition(), point))
            .FirstOrDefault();
    }

    public ItemObjectSeat.SittingPoint FindNearestVacantSittingPoint(Vector3 origin)
    {
        return FindItemObjects<ItemObjectSeat>()
            .Select(o => o.GetNearestUnnocupiedSittingPoint(origin))
            .OrderBy(o => Vector3.Distance(origin, o.GetSeatCell().GetCentrePosition()))
            .FirstOrDefault();
    }

    public void OnBuildComplete()
    {
        Mall.CurrentFloor.RegisterRoom(this);
        /*
                if(roomParams is ShopParams)
                {
                    ShopParams shopParams = roomParams as ShopParams;

                    if (shopParams.ShopAssistantPrefab != null)
                    {
                        ItemObjectTill[] tills = FindItemObjects<ItemObjectTill>();

                        foreach(ItemObjectTill till in tills)
                        {
                            if(till.AssistantCell != null && till.CustomerCell != null)
                            {
                                Vector3 position = till.AssistantCell.GetCentrePosition();
                                Quaternion rotation = Quaternion.LookRotation(till.CustomerCell.GetCentrePosition() - till.AssistantCell.GetCentrePosition(), Vector3.up);
                                ShopAssistant shopAssistant = Instantiate(shopParams.ShopAssistantPrefab, position, rotation, Mall.CurrentFloor.FixedCharactersContainers);
                                shopAssistant.Movement.LookAt(till.CustomerCell.GetCentrePosition());
                                fixedCharacters.Add(shopAssistant);
                            }
                        }
                    }
                }*/
    }

    public override void SetHighlighted(bool highlighted)
    {
        base.SetHighlighted(highlighted);

        foreach (BuildingPieceObject buildingPieceObject in FindBuildingPieceObjects<BuildingPieceObject>())
        {
            buildingPieceObject.SetForcedOutline(highlighted);
        }

        foreach (ItemObject itemObject in FindItemObjects<ItemObject>())
        {
            itemObject.SetForcedOutline(highlighted);
        }
    }

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);

        foreach (BuildingPieceObject buildingPieceObject in FindBuildingPieceObjects<BuildingPieceObject>())
        {
            buildingPieceObject.SetForcedSelectedOutline(selected);

            // If we're deselecting we can deselect fully for good measure. When selecting we only want to force the outline
            if (!selected)
            {
                buildingPieceObject.SetSelected(false);
            }
        }

        foreach (ItemObject itemObject in FindItemObjects<ItemObject>())
        {
            itemObject.SetForcedSelectedOutline(selected);

            if (!selected)
            {
                itemObject.SetSelected(false);
            }
        }
    }

    public Cell[] GetRoomEmptyCells()
    {
        return cells.Where(o => o.itemObject == null).ToArray();
    }

    public void SetFloorTextureSet(TextureSet textureSet)
    {
        floorTextureSet = textureSet;
        floor.UpdateFloorMaterial();

        Debug.Log(textureSet.name);
    }

    public void SetWallMaterialSet(WallMaterialSet wallMaterialSet)
    {
        this.wallMaterialSet = wallMaterialSet;

        foreach (BuildingPieceObject wallObject in FindBuildingPieceObjects<BuildingPieceObject>())
        {
            wallObject.UpdateWallMaterials();
        }

        Debug.Log(wallMaterialSet.name);
    }

    public void OnCustomerEnterRoom()
    {
        foreach (BuildingPieceEntrance entrance in FindBuildingPieceObjects<BuildingPieceEntrance>())
        {
            entrance.PlayCustomerEnterAnim();
        }
    }

    public void OnCustomerLeaveRoom()
    {
        foreach (BuildingPieceEntrance entrance in FindBuildingPieceObjects<BuildingPieceEntrance>())
        {
            entrance.PlayCustomerEnterAnim();
        }
    }

    public void DestroyRoom()
    {
        SetSelected(false);
        floor.UnregisterRoom(this);
        Destroy(gameObject);

        foreach (Character character in fixedCharacters)
        {
            Destroy(character.gameObject);
        }
        fixedCharacters.Clear();

        foreach (Cell cell in cells)
        {
            if (cell.itemObject)
            {
                Destroy(cell.itemObject.gameObject);
            }

            List<BuildingPieceObject> wallsToUpdate = new();

            foreach (CellEdge edge in cell.edges)
            {
                if (edge.buildingPieceObject)
                {
                    if (edge.IsBorderingTwoRooms())
                    {
                        wallsToUpdate.Add(edge.buildingPieceObject);
                    }
                    else
                    {
                        Destroy(edge.buildingPieceObject.gameObject);
                    }
                }
            }

            cell.room = null;

            // Update the wall materials AFTER clearing the cell room value
            foreach (BuildingPieceObject wallPiece in wallsToUpdate)
            {
                wallPiece.UpdateWallMaterials();
            }
        }

        floor.RebuildNavMesh();
        floor.UpdateFloorMaterial();
    }

    public void RegFixedCharacter(Character Char)
    {
        fixedCharacters.Add(Char);
    }
}
