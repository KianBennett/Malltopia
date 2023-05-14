using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectsPanel : ListPanel
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

        for (int i = 0; i < BuildingSystem.Instance.CommonAreaObjects.Length; i++)
        {
            WorldObject worldObject = BuildingSystem.Instance.CommonAreaObjects[i];
            PurchaseButton button = Instantiate(purchaseButtonPrefab, scrollViewContentRect);
            button.SetValues(worldObject.DisplayText, worldObject.Cost);
            button.Rect.anchoredPosition = new Vector3(0, -60 - 90 * i, 0);
            int index = i;
            button.Button.onClick.AddListener(delegate 
            {
                PlayerController.Instance.EnterSingleObjectBuildMode();
                BuildingSystem.Instance.ChangeSingleObject(worldObject);
                BuildingSystem.Instance.SetCurrentRoom(null);
            });
            buttons.Add(button);
        }

        scrollViewContentRect.sizeDelta = new Vector2(scrollViewContentRect.sizeDelta.x, buttons.Count * 90 + 30);
    }
}