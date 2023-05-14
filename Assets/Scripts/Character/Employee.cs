using UnityEngine;

public class Employee : Character
{
    [SerializeField] private RoomBuildingParams staffRoomBuilding;
    [SerializeField] private string jobTitle;
    [SerializeField] private int hireCost;
    [SerializeField] private int wage;
    [SerializeField] private int energyDecreaseRate;
    [SerializeField] private int energyRecoveryRateSitting;

    private float happiness; // min = 0, max = 100
    private float energy; // min = 0, max = 100

    public string JobTitle { get { return jobTitle; } }
    public float Happiness { get { return happiness; } }
    public float Energy { get { return energy; } }
    public int HireCost { get { return hireCost; } }
    public int Wage { get { return wage; } }
    public RoomBuildingParams StaffRoomBuilding { get { return staffRoomBuilding; } }

    protected override void Awake()
    {
        base.Awake();

        energy = 100;
        happiness = 100;
    }

    protected override void Update()
    {
        base.Update();

        if (energy <= 0 && (!IsInState<CharacterStateEmployeeLeaving>() && !IsInState<CharacterStateBreak>()))
        {
            ChangeState(new CharacterStateBreak(this));
        }

        if(IsSitting)
        {
            energy += energyRecoveryRateSitting * Time.deltaTime;
        }

        bool IsStationaryInStaffRoom = !IsSitting && !Movement.HasPath && CurrentCell?.room && CurrentCell?.room.RoomParams == StaffRoomBuilding;

        if(IsStationaryInStaffRoom)
        {
            energy += energyRecoveryRateSitting * Time.deltaTime * 0.5f;
        }

        // For now, just set the happiness to their energy level. Change later.
        happiness = energy;
    }

    public void SetEnergy(float setAmount)
    {
        energy = setAmount;
    }
    public void ReduceEnergy (float reductionAmount)
    {
        energy -= reductionAmount;
    }

    void OnDestroy()
    {
        EmployeeManager.Instance?.UnregisterEmployee(this);
    }
}