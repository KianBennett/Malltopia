using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomBuildStatusPanel : StandardMenu
{
    [SerializeField] private TextMeshProUGUI textRoomName;
    [SerializeField] private TextMeshProUGUI textStatus;

    public void Open(string roomName)
    {
        Show();
        textRoomName.text = roomName;
    }

    public void UpdateStatus(int currentCost, bool tooSmall)
    {
        if(tooSmall)
        {
            textStatus.text = "Too small!";
        }
        else
        {
            textStatus.text = "<alpha=#AA>Current Cost:\n<alpha=#FF>$" + currentCost;
        }
    }

    public void Cancel()
    {
        BuildingSystem.Instance.CancelBuilding();
    }
}
