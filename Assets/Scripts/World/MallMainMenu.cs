using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MallMainMenu : Mall
{
    [SerializeField] private int initialCustomerCount;

    void Start()
    {
        Floor.NavMeshGenerationEnabled = false;

        // Place rooms

        // Shoe shop
        Room shoeShop = CurrentFloor.BuildRoom(
            BuildingSystem.Instance.Rooms[0],
            CurrentFloor.GetCellsFromCorners(new Vector2Int(20, 18), new Vector2Int(28, 26))
        );
        BuildingSystem.Instance.PlaceFullBuildingPiece(
            BuildingSystem.Instance.WorldObjectList.buildingPieceSignShoe, 
            CurrentFloor.GetCell(new Vector2Int(28, 22)).GetEdge(Direction.East), 
            Direction.East, 
            shoeShop
        );
        CurrentFloor.AddItemInRoom(
            CurrentFloor.GetCell(new Vector2Int(22, 24)), 
            Direction.North, 
            BuildingSystem.Instance.WorldObjectList.objectCounterTill, 
            shoeShop
        ).Init();
        shoeShop.OnBuildComplete();

        // Sweet shop
        Room sweetShop = CurrentFloor.BuildRoom(
            BuildingSystem.Instance.Rooms[1],
            CurrentFloor.GetCellsFromCorners(new Vector2Int(22, 27), new Vector2Int(28, 32))
        );
        BuildingSystem.Instance.PlaceFullBuildingPiece(
            BuildingSystem.Instance.WorldObjectList.buildingPieceSignSweet,
            CurrentFloor.GetCell(new Vector2Int(28, 29)).GetEdge(Direction.East),
            Direction.East,
            sweetShop
        );
        CurrentFloor.AddItemInRoom(
            CurrentFloor.GetCell(new Vector2Int(23, 30)),
            Direction.North,
            BuildingSystem.Instance.WorldObjectList.objectCounterTill,
            sweetShop
        ).Init();
        sweetShop.OnBuildComplete();

        // Clothing shop
        Room clothingShop = CurrentFloor.BuildRoom(
            BuildingSystem.Instance.Rooms[2],
            CurrentFloor.GetCellsFromCorners(new Vector2Int(29, 33), new Vector2Int(38, 40))
        );
        BuildingSystem.Instance.PlaceFullBuildingPiece(
            BuildingSystem.Instance.WorldObjectList.buildingPieceSignClothing,
            CurrentFloor.GetCell(new Vector2Int(35, 33)).GetEdge(Direction.South),
            Direction.South,
            clothingShop
        );
        CurrentFloor.AddItemInRoom(
            CurrentFloor.GetCell(new Vector2Int(31, 38)),
            Direction.North,
            BuildingSystem.Instance.WorldObjectList.objectCounterTill,
            clothingShop
        ).Init();
        clothingShop.OnBuildComplete();

        // Comic shop
        Room comicShop = CurrentFloor.BuildRoom(
            BuildingSystem.Instance.Rooms[3],
            CurrentFloor.GetCellsFromCorners(new Vector2Int(39, 27), new Vector2Int(43, 40))
        );
        BuildingSystem.Instance.PlaceFullBuildingPiece(
            BuildingSystem.Instance.WorldObjectList.buildingPieceSignComic,
            CurrentFloor.GetCell(new Vector2Int(40, 27)).GetEdge(Direction.South),
            Direction.South,
            comicShop
        );
        CurrentFloor.AddItemInRoom(
            CurrentFloor.GetCell(new Vector2Int(41, 38)),
            Direction.North,
            BuildingSystem.Instance.WorldObjectList.objectCounterTill,
            comicShop
        ).Init();
        comicShop.OnBuildComplete();

        // Toilets
        Room toilets = CurrentFloor.BuildRoom(
            BuildingSystem.Instance.Rooms[8],
            CurrentFloor.GetCellsFromCorners(new Vector2Int(44, 16), new Vector2Int(48, 22))
        );
        BuildingSystem.Instance.PlaceFullBuildingPiece(
            BuildingSystem.Instance.WorldObjectList.buildingPieceSignToilet,
            CurrentFloor.GetCell(new Vector2Int(44, 18)).GetEdge(Direction.West),
            Direction.West,
            toilets
        );
        CurrentFloor.AddItemInRoom(
            CurrentFloor.GetCell(new Vector2Int(46, 20)),
            Direction.West,
            BuildingSystem.Instance.WorldObjectList.objectToiletCubicle,
            toilets
        ).Init();
        toilets.OnBuildComplete();

        Floor.NavMeshGenerationEnabled = true;

        // Common objects

        CurrentFloor.AddItem(
            CurrentFloor.GetCell(new Vector2Int(39, 19)),
            Direction.North,
            BuildingSystem.Instance.WorldObjectList.objectFountain
        ).Init();

        StartCoroutine(rebuildNavMesh());

        // Spawn customers
        for(int i = 0; i < initialCustomerCount; i++)
        {
            int cellX = Random.Range(10, 60);
            int cellZ = Random.Range(16, 50);
            Cell cell = CurrentFloor.GetCell(cellX, cellZ);

            if(cell != null && cell.itemObject == null )
            {
                CustomerManager.Instance.SpawnCustomer(cell.GetCentrePosition());
            }
        }
    }

    // We want to yield to the next frame before generating the navmesh instead of doing it straight after building the mall
    // This is because Destroy is called on some objects that will affect the navmesh and these objects aren't truly gone until the next frame
    private IEnumerator rebuildNavMesh()
    {
        yield return new WaitForSeconds(Time.deltaTime);
        CurrentFloor.RebuildNavMesh();
    }
}
