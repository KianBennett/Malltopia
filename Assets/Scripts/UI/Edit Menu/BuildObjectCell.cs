using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildObjectCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Key Item Dot")]

    [SerializeField] private Image IconRef;

    [SerializeField] private Image RequiredItemDot;

    [SerializeField] private Sprite CrossImage, TickImage;
    [SerializeField] private Color GreenColor = Color.green, RedColor = Color.red;

    [Header("Outline")]
    [SerializeField] private UnityEngine.UI.Outline OutlineObject;

    [SerializeField] private Color HoverColor, SelectedColor;

    [Header("Out Objects")]
    [SerializeField] private TMP_Text DisplayName;

    [SerializeField] private TMP_Text DisplayCost;

    [Header("Refs")]
    [HideInInspector] public Room _CurrentRoom;

    [SerializeField] private WorldObject _Item;

    [SerializeField] private bool RequiredItem;
    bool Selected;

    public bool SetSelected {
        set { Selected = value; } 
        get { return Selected; } 
    }

    private int Cost;


    private void Start()
    {
        transform.localScale = new(1, 1, 1);
    }

    public void ButtonStart(WorldObject Item, Room TargetRoom)
    {
        _Item = Item;
        _CurrentRoom = TargetRoom;
        DisplayName.text = Item.DisplayText;
        DisplayCost.text = "$" + Item.Cost.ToString();

        if (Item.GetComponent<ItemObjectKey>() != null)
        {
            RequiredItem = true;
        }
        else if (Item.GetComponent<ItemObjectTill>() != null)
        {
            RequiredItem = true;
        }
        else
        {
            RequiredItemDot.gameObject.SetActive(false);
        }
    }

    public void SetIcon(Sprite Icon)
    {
        IconRef.sprite = Icon;
    }

    private void Update()
    {
        if (RequiredItem) SetTick(ItemObjectKey_Check(_CurrentRoom));

        
    }

    public bool ItemObjectKey_Check(Room RoomObject)
    {
        return false;

        for (int i = 0; i < RoomObject.transform.GetChild(1).childCount; i++)
        {
            if (RoomObject.transform.GetChild(1).GetChild(i).GetComponent<WorldObject>().DisplayText == _Item.DisplayText)
            {
                return true;
            }
        }
        return false;
    }

    private bool ItemPlacedCheck(ItemObject[] Item)
    {
        foreach (ItemObject key in Item)
        {
            if (_Item == key)
            {
                return true;
            }
        }
        return false;
    }

    public void SetTick(bool IsPlaced)
    {
        if (IsPlaced)
        {
            RequiredItemDot.color = GreenColor;
            RequiredItemDot.transform.GetChild(0).GetComponent<Image>().sprite = TickImage;
        }
        else
        {
            RequiredItemDot.color = RedColor;
            RequiredItemDot.transform.GetChild(0).GetComponent<Image>().sprite = CrossImage;
        }
    }
    public void SetSelectedOutline(bool Set)
    {
        if (Set)
        {
            OutlineObject.enabled = true;
            OutlineObject.effectColor = SelectedColor;
        }
        else
        {
          //  OutlineObject.effectColor = Color.clear;
            OutlineObject.enabled = false;
        }

    }


    public void OnPointerEnter(PointerEventData Data)
    {
        if (!Selected)
        {
            OutlineObject.enabled = true;
            OutlineObject.effectColor = HoverColor;
        }
    }

    public void OnPointerExit(PointerEventData Data)
    {
        if (!Selected)
        {
            OutlineObject.enabled = false;
        }
    }
}