using UnityEngine;

/*
*   Additional properties for each shop
*/

[CreateAssetMenu(fileName = "ShopParams", menuName = "Data Assets/ShopParams", order = 0)]
public class ShopParams : RoomBuildingParams
{
    public ShopAssistant ShopAssistantPrefab;
    public int ProductCost;
    public ItemObject ItemObjectKey;
}