using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerContextMenu : ContextMenuRoot
{
    [SerializeField]
    private TMP_Text Name, Goal, Budget;

    //Sliders
    [SerializeField]
    private GameObject HappynessSlider, FoodSlider, DrinkSlider, ToiletSlider, CleanlinessSlider, EnergySlider, EntertainmentSlider;

    [HideInInspector]
    public Customer CustomerScript;

    private Floor floorscript;

    [SerializeField]
    private GameObject GoalLocateButton;

    private void Start()
    {
        floorscript = ContextMenuManager.Instance.floorscript;
        Name.text = CustomerScript.CharacterName;
        GenerateVisitList();
    }

    public void CameraToPosButton()
    {
        CameraController.Instance.objectToFollow = CustomerScript.gameObject.transform;
    }

    public void SliderUpdate()
    {
        GetSliderComponent(HappynessSlider).value = CustomerScript.Happiness;
        GetSliderComponent(HappynessSlider).fillRect.GetComponent<Image>().color = SetSliderColor(CustomerScript.Happiness);
        HappynessSlider.transform.GetChild(1).GetComponent<Image>().color = SetSliderColor(CustomerScript.Happiness);
        GetSliderComponent(EnergySlider).value = CustomerScript.Energy;
        GetSliderComponent(EnergySlider).fillRect.GetComponent<Image>().color = SetSliderColor(CustomerScript.Energy);
        EnergySlider.transform.GetChild(1).GetComponent<Image>().color = SetSliderColor(CustomerScript.Energy);
    }

    private Slider GetSliderComponent(GameObject item)
    {
        return item.transform.GetChild(0).GetComponent<Slider>();
    }

    private void Update()
    {
        SliderUpdate();
        Goal.text = GetStatues();
        Budget.text ="Budget: $" + CustomerScript.Budget.ToString();
        //  print(CustomerScript.ShopInterest);

        if (floorscript.GetCellFromWorldPos(CustomerScript.Movement.Destination) == null||floorscript.GetCellFromWorldPos(CustomerScript.Movement.Destination).room == null)
        {
            GoalLocateButton.SetActive(false);
        }
        else
        {
            GoalLocateButton.SetActive(true);
        }
    }

    public Color SetSliderColor(float Value)
    {
        if (Value >= 75) return PositiveColor;
        else if (Value >= 50)
        {
            return Color.Lerp(MidColor, PositiveColor, ((Value / 100) - 0.50f) * 4);
        }
        else if (Value >= 25)
        {
            return Color.Lerp(NegativeColor, MidColor, ((Value / 100) - 0.25f) * 4);
        }
        else return NegativeColor;
    }

    private string GetStatues()
    {
        return CustomerScript.CurrentState.GetCurrentStateText();
        //Cell DestinationCell = floorscript.GetCellFromWorldPos(CustomerScript.Movement.Destination);
        //if (DestinationCell == null) return "leaving the mall";
        //if (floorscript.GetCellFromWorldPos(CustomerScript.transform.position) == null) return "Entering the mall";
        //else if (DestinationCell.room != null)
        //{
        //    if (DestinationCell.room != floorscript.GetCellFromWorldPos(CustomerScript.transform.position).room)
        //    {
        //        return "En route to " + DestinationCell.room.RoomParams.Name;
        //    }
        //    else
        //    {
        //        return "Shopping in " + DestinationCell.room.RoomParams.Name;
        //    }
        //    //Debug.LogError("Defaulted, Please Check");
        //    //return "Defaulted, Please Check";
        //}
        //else return "Wondering the mall";
    }

    public void CamToDestnation()
    {
        CameraController.Instance.SetPositionImmediate(GetRoomPos(floorscript.GetCellFromWorldPos(CustomerScript.Movement.Destination).room));
    }

    public void CamToCustomer()
    {
        CameraController.Instance.SetPositionImmediate(CustomerScript.transform.position);
    }

    public GameObject listPrafab;
    public GameObject ListLocation;

    private void GenerateVisitList()
    {
        if (CustomerScript.ShopsInteractedWith.Count == 0)
        {
            GameObject ddd = Instantiate(listPrafab);
            ddd.transform.GetChild(0).GetComponent<TMP_Text>().text = "No shops visited yet";
            ddd.transform.SetParent(ListLocation.transform);
        }
        else
        {
            foreach (var item in CustomerScript.ShopsInteractedWith)
            {
                GameObject ddd = Instantiate(listPrafab);
                ddd.transform.GetChild(0).GetComponent<TMP_Text>().text = item.Name;
                ddd.transform.SetParent(ListLocation.transform);
            }
        }
    }
}