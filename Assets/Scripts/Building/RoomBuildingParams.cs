using UnityEngine;

/*
*   Defines the properties for each room
*/

[CreateAssetMenu(fileName = "RoomParams", menuName = "Data Assets/RoomParams", order = 0)]
public class RoomBuildingParams : ScriptableObject 
{
    public string Name;
    public int Cost;
    public int CostPerTile;
    public Vector2Int MinSize;
    public WorldObject[] ObjectsToPlace;
    public WorldObject[] ObjectsCanBuild;
    public TextureSet defaultFloorTextureSet;
    public WallMaterialSet defaultWallMaterials;
    public bool staffOnly;
}