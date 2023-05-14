using TMPro;
using UnityEngine;

public class RoomContextMenu : ContextMenuRoot
{
    [SerializeField] TMP_Text Name;



    Room SelectedRoom;

    public Room SetRoom { set { SelectedRoom = value; }   }

    private void Start()
    {
        Name.text = SelectedRoom.name;
    }

    public void DestoryRoom()
    {
        if (PlayerController.Instance.SelectedRoom)
        {
            PlayerController.Instance.SelectedRoom.DestroyRoom();
            AkSoundEngine.PostEvent("destroyWalls", gameObject);
            CloseContextMenu();
        }
    }

    public void EditRoomButton()
    {
        if (PlayerController.Instance.SelectedRoom)
        {
            HUD.Instance.RoomBuildPanel.Open(PlayerController.Instance.SelectedRoom);
            CloseContextMenu();
        }
    }


    public void CamToDestnation()
    {
        CameraController.Instance.SetPositionImmediate(GetRoomPos(SelectedRoom));
    }

}