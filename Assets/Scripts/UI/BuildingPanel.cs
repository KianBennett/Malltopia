using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPanel : ListPanel
{
    [SerializeField] private PurchaseButton purchaseButtonPrefab;
    [SerializeField] private RectTransform scrollViewContentRect;

    private List<PurchaseButton> buttons;

    protected override void onClose()
    {
        PlayerController.Instance.ExitBuildMode();
    }

    protected override void populateList()
    {
        buttons = new();

        for (int i = 0; i < BuildingSystem.Instance.Rooms.Length; i++)
        {
            RoomBuildingParams room = BuildingSystem.Instance.Rooms[i];
            PurchaseButton button = Instantiate(purchaseButtonPrefab, scrollViewContentRect);
            button.SetValuesRoom(room.Name,  room.Cost, room.CostPerTile );
            button.Rect.anchoredPosition = new Vector2(0, -60 - 90 * i);
            int index = i;
            button.Button.onClick.AddListener(delegate 
            {
                BuildingSystem.Instance.ChangeRoom(index);
                PlayerController.Instance.EnterBuildMode();
                Close(false);
            });
            buttons.Add(button);
        }

        scrollViewContentRect.sizeDelta = new Vector2(scrollViewContentRect.sizeDelta.x, buttons.Count * 90 + 30);
    }
}