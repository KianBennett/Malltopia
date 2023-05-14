using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuManager : Singleton<ContextMenuManager>
{
    [SerializeField]
    private GameObject CustomerMenuPrefab, EmployeeMenuPrefab, RoomMenuPrefab, ObjectMenuPrefab;

    public GameObject CurrentContextMenu;

    public Floor floorscript;

    private void Start()
    {
        if (!floorscript)
        {
            floorscript = FindObjectOfType<Floor>();
        }
    }
    public enum ContextMenuType
    {
        Customer,
        Employee,
        Room,
        Object,
        NullType
    }

    public void ToggleContextMenu(ContextMenuType Type)
    {
        switch (Type)
        {
            case ContextMenuType.NullType:
                {
                    if (CurrentContextMenu) CloseCurrentContextMenu();

                    break;
                }
            case ContextMenuType.Customer:
                {
                    InstantiateCustomerMenu();

                    break;
                }
            case ContextMenuType.Employee:
                {
                    InstantiateEmployeeMenu();

                    break;
                }
            case ContextMenuType.Object:
                {
                    InstantiateObjectMenu();
                    break;
                }
            case ContextMenuType.Room:
                {
                    InstantiateRoomMenu();
                    break;
                }
            default:
                {
                    Debug.LogError("ContextMenuManager, ToggleContextMenu has defaulted");
                    break;
                }
        }
    }

    public void CloseCurrentContextMenu()
    {
        if (CurrentContextMenu)
        {

            CurrentContextMenu.GetComponent<Animator>().SetTrigger("Exit");
            CurrentContextMenu = null;
        }
    }

    public void InstantiateCustomerMenu()
    {
        GameObject RootPanel = Instantiate(CustomerMenuPrefab);
        RootPanel.GetComponent<CustomerContextMenu>().CustomerScript = (Customer)PlayerController.Instance.SelectedCharacter;
        RootPanel.transform.SetParent(HUD.Instance.gameObject.transform);
        CurrentContextMenu = RootPanel;

        //Vector2 Pos = new(Screen.currentResolution.width / 2- 260, Screen.currentResolution.height / 2 - 270);
        //RootPanel.GetComponent<RectTransform>().localPosition = Pos;
        RootPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260, -285);
    }
    public void InstantiateRoomMenu()
    {
        GameObject RootPanel = Instantiate(RoomMenuPrefab);
        RootPanel.GetComponent<RoomContextMenu>().SetRoom = PlayerController.Instance.SelectedRoom;
        RootPanel.transform.SetParent(HUD.Instance.gameObject.transform);
        CurrentContextMenu = RootPanel;

        RootPanel.GetComponent<RectTransform>().position = CameraController.Instance.Camera.WorldToScreenPoint(GetRoomPos(PlayerController.Instance.SelectedRoom)) + Vector3.up * 100;

        /*        Vector2 Pos = new(Screen.currentResolution.width / 4, 0);
                RootPanel.GetComponent<RectTransform>().localPosition = Pos;*/
    }
    public void InstantiateObjectMenu()
    {
        GameObject RootPanel = Instantiate(ObjectMenuPrefab);
        RootPanel.transform.SetParent(HUD.Instance.gameObject.transform);
        CurrentContextMenu = RootPanel;

        //Vector2 Pos = new(Screen.currentResolution.width / 2 - 260, Screen.currentResolution.height / 2 - 70);
        //RootPanel.GetComponent<RectTransform>().localPosition = Pos;
        RootPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-260, -70);
    }
    public void InstantiateEmployeeMenu()
    {
        GameObject RootPanel = Instantiate(EmployeeMenuPrefab);
        RootPanel.transform.SetParent(HUD.Instance.gameObject.transform);
        CurrentContextMenu = RootPanel;

        //Vector2 Pos = new(Screen.currentResolution.width / 2 - 260, Screen.currentResolution.height / 2 - 100);
        //RootPanel.GetComponent<RectTransform>().localPosition = Pos;
        RootPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -20);
    }



    private Vector3 GetRoomPos(Room RoomInWorld)
    {
        Vector3 pos = new();

        for (int i = 0; i < RoomInWorld.cells.Count; i++)
        {
            pos += RoomInWorld.cells[i].GetCentrePosition();
        }

        return pos / RoomInWorld.cells.Count;


    }
}