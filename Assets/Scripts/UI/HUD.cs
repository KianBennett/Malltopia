using UnityEngine;
using TMPro;

public class HUD : Singleton<HUD>
{
    public TextMeshProUGUI TextCustomerInfo;

    [SerializeField] private bool showHelpMenuAtStart;
    [SerializeField] private GameObject hudContainer;
    [SerializeField] private BuildingPanel buildingPanel;
    [SerializeField] private RoomBuildPanel roomBuildPanel;
    [SerializeField] private RoomBuildStatusPanel roomBuildStatusPanel;
    [SerializeField] private ObjectsPanel objectsPanel;
    [SerializeField] private EmployeePanel employeePanel;
    [SerializeField] private ToolsPanel toolsPanel;
    [SerializeField] private StatsPanel statsPanel;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private EventsMenu eventsMenu;
    [SerializeField] private EventDetailsMenu eventDetailsMenu;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private NotificationsManager notifications;
    [SerializeField] private SecurityCameraView securityCameraView;

    public BuildingPanel BuildingPanel { get { return buildingPanel; } }
    public RoomBuildPanel RoomBuildPanel { get { return roomBuildPanel; } }
    public RoomBuildStatusPanel RoomBuildStatusPanel { get { return roomBuildStatusPanel; } }
    public ObjectsPanel ObjectsPanel { get { return objectsPanel; } }
    public EmployeePanel EmployeePanel { get { return employeePanel; } }
    public ToolsPanel ToolsPanel { get { return toolsPanel; } }
    public PauseMenu PauseMenu { get { return pauseMenu; } }
    public EventsMenu EventsMenu { get { return eventsMenu; } }
    public EventDetailsMenu EventDetailsMenu { get { return eventDetailsMenu; } }
    public NotificationsManager Notifications { get { return notifications; } }
    public SecurityCameraView SecurityCameraView { get { return securityCameraView; } }

    protected override void Awake()
    {
        base.Awake();

        if(showHelpMenuAtStart)
        {
            Invoke("openHelpMenu", 3.0f);
        }
    }

    void Update()
    {
        if(TextCustomerInfo != null)
        {
            Character selectedCharacter = PlayerController.Instance.SelectedCharacter;
            if(selectedCharacter)
            {
                if(selectedCharacter is Customer)
                {
                    Customer selectedCustomer = selectedCharacter as Customer;
                    TextCustomerInfo.text = string.Format("Name: {0}\nState: {1}\nHappiness: {2}\nEnergy: {3}\nBudget: {4}",
                        selectedCustomer.CharacterName,
                        selectedCustomer.CurrentState != null ? selectedCustomer.CurrentState.ToString().Replace("CustomerState", "") : "",
                        selectedCustomer.Happiness.ToString("0.0"),
                        selectedCustomer.Energy.ToString("0.0"),
                        selectedCustomer.Budget
                    );
                }
                else if(selectedCharacter is Employee)
                {
                    Employee selectedEmployee = selectedCharacter as Employee;
                    TextCustomerInfo.text = string.Format("Name: {0}\nState: {1}\nEnergy: {2}",
                        selectedEmployee.CharacterName,
                        selectedEmployee.CurrentState != null ? selectedEmployee.CurrentState.ToString().Replace("CustomerState", "") : "",
                        selectedEmployee.Energy.ToString("0.0")
                    );
                }
                else
                {
                    TextCustomerInfo.text = string.Format("Name: {0}\nState: {1}",
                        selectedCharacter.CharacterName,
                        selectedCharacter.CurrentState != null ? selectedCharacter.CurrentState.ToString().Replace("CustomerState", "") : ""
                    );
                }
            }
            else
            {
                TextCustomerInfo.text = "";
            }
        }
    }

    public void SetHudEnabled(bool enabled)
    {
        hudContainer.gameObject.SetActive(enabled);
    }

    public void ShowInfoPanel()
    {
        infoPanel.SetActive(true);
    }

    public void HideInfoPanel()
    {
        infoPanel.SetActive(false);
    }

    public void OpenEventsMenu()
    {
        eventsMenu.Open();
    }

    private void openHelpMenu()
    {
        TimeManager.Instance.SetTimeSpeed(0);
        UIManager.Instance.HelpMenu.Show(delegate { TimeManager.Instance.SetTimeSpeed(1); });
    }
}