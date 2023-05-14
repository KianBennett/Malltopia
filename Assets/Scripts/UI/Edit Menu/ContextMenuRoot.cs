using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuRoot : MonoBehaviour
{

    public GameObject CloseButton;

    
   public Color PositiveColor, MidColor, NegativeColor;




    public void DestoryMenu()
    {
        Destroy(gameObject);
    }


    void asdas(Transform Pos)
    {
    }


   public Vector3 GetRoomPos(Room RoomInWorld)
    {
        Vector3 pos = new();

        for (int i = 0; i < RoomInWorld.cells.Count; i++)
        {
            pos += RoomInWorld.cells[i].GetCentrePosition();
        }

       return pos / RoomInWorld.cells.Count;


    }
    public void CloseContextMenu()
    {
        ContextMenuManager.Instance.CloseCurrentContextMenu();
    }
}