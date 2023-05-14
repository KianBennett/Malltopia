using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Customer : Character
{
    public CustomerParams Params;

    private float happiness;    // min = 0, max = 100
    private float energy;       // min = 0, max = 100
    private float hunger;       // min = 0, max = 100
    private float thirst;       // min = 0, max = 100
    private int budget;
    private float thief;
    private int amountStolen;
    private Dictionary<RoomBuildingParams, float> shopInterest; // min = 0, max = 100
    private ShopParams startingShop;

    private bool isInQueue;
    private bool isBrowsing;
    private float totalEnergyRestored;
    private List<RoomBuildingParams> shopsInteractedWith;

    public float Happiness { get { return happiness; } }
    public float Energy { get { return energy; } }
    public float Hunger { get { return hunger; } }
    public float Thirst { get { return thirst; } }
    public int Budget { get { return budget; } }
    public float Thief { get { return thief; } }
    public int AmountStolen { get { return amountStolen; } }
    public List<RoomBuildingParams> ShopsInteractedWith { get { return shopsInteractedWith; } }
    public Dictionary<RoomBuildingParams, float> ShopInterest { get { return shopInterest; } }
    public ShopParams StartingShop { get { return startingShop; } set { startingShop = value; } }
    public bool IsBrowsing { get { return isBrowsing; } set { isBrowsing = value; } }

    protected override void Awake()
    {
        base.Awake();
        happiness = Random.Range(60.0f, 80.0f);
        energy = Random.Range(80.0f, 100.0f);
        budget = Random.Range(40, 70);
        thief = Random.Range(1, 100);
        shopInterest = new();
        shopsInteractedWith = new();

        StartCoroutine(seeTrashIEnum());
        StartCoroutine(seeDecorationIEnum());
        StartCoroutine(playCustomerSounds());
    }

    protected override void Start()
    {
        base.Start();

        // If a starting shop hasn't been defined, pick one randomly from the existing shops in the mall
        if (startingShop == null)
        {
            Room[] shops = Mall.CurrentFloor.FindShops();
            if (shops.Length > 0)
            {
                startingShop = shops[Random.Range(0, shops.Length)].RoomParams as ShopParams;
            }
        }

        if(startingShop != null)
        {
            shopInterest.Add(startingShop, Params.MaxInterest);
        }
        else
        {
            ChangeState(new CharacterStateLeaving(this));
            //HUD.Instance.Notifications.ShowNotification("A customer entered the mall but couldn't find a shop");
            //ChangeState(new CharacterStateSitting(this));
            energy = 20;
        }
    }

    protected override void Update()
    {
        base.Update();

        if(!IsLeaving)
        {
            // When a customer has maximum interest in a shop, go and buy something
            if(!IsInState<CharacterStatePurchase>() && !IsInState<CharacterStateStealing>())
            {
                RoomBuildingParams firstShopWithMaxInterest = 
                    shopInterest.Keys.Where(o => shopInterest[o] >= Params.MaxInterest).FirstOrDefault();

                if (firstShopWithMaxInterest != null && firstShopWithMaxInterest is ShopParams)
                {
                    if((firstShopWithMaxInterest as ShopParams).ProductCost <= budget)
                    {
                        ChangeState(new CharacterStatePurchase(this, firstShopWithMaxInterest));
                    }

                    //if (thief <= 20)
                    //{
                    //    ChangeState(new CharacterStateStealing(this, firstShopWithMaxInterest));
                    //}
                    //else
                    //{
                    //    if (happiness < 20)
                    //    {
                    //        ChangeState(new CharacterStateStealing(this, firstShopWithMaxInterest));
                    //    }
                    //    else
                    //    {
                    //        ChangeState(new CharacterStatePurchase(this, firstShopWithMaxInterest));
                    //    }
                    //}

                    shopInterest.Remove(firstShopWithMaxInterest);
                }

            }
            
            // Restore energy when sitting
            if(IsSitting)
            {
                float energyToRestore = Time.deltaTime * Params.EnergyRecoveryRateSitting;

                // Diminishing returns - the more energy that has been restored over time the less energy gets restored by sitting
                //if(totalEnergyRestored > 0)
                //{
                //    energyToRestore *= (1 - totalEnergyRestored / Params.MaxEnergyRestoredBySitting / 2);
                //    if(energyToRestore < 0) energyToRestore = 0;
                //}

                energy += energyToRestore;
                totalEnergyRestored += energyToRestore;

                if(energy > 80 || totalEnergyRestored >= Params.MaxEnergyRestoredBySitting)
                {
                    ChangeState(new CharacterStateWander(this));
                }
            }
            // Otherwise energy gets depleted over time
            else if(shopsInteractedWith.Count > 0)
            {
                energy = Mathf.Max(energy - Time.deltaTime * Params.EnergyDecreaseRate, 0);
            }    

            // If energy falls below a threshold then look for somewhere to sit down
            if(energy < 20 && !IsInState<CharacterStateSitting>())
            {
                ChangeState(new CharacterStateSitting(this));
            }

            if(IsInState<CharacterStateWander>())
            {
                // For each shop doorway or window in range, increase the interest in that shop
                BuildingPieceObject[] buildingObjectsInRange = Mall.CurrentFloor.FindBuildingObjectsInRange(transform.position, Params.RangeToIncreaseShopInterest, BuildingPieceObject.PieceType.Doorway, BuildingPieceObject.PieceType.Window);
                foreach(BuildingPieceObject buildingObject in buildingObjectsInRange)
                {
                    RoomBuildingParams roomBuildingParams = buildingObject.RoomFront.RoomParams;

                    // If the room is a shop and the customer hasn't bought from there before
                    if(roomBuildingParams is ShopParams && !shopsInteractedWith.Contains(roomBuildingParams))
                    {
                        increaseInterestInShop(roomBuildingParams, Params.ShopInterestIncreaseRatePerObject * Time.deltaTime);
                    }
                }
            }

            // Decrease happiness while standing in a queue
            if(isInQueue)
            {
                happiness = Mathf.Max(happiness - Time.deltaTime * Params.QueueHappinessDecreaseRate);
            }

            // Leave when running out of energy, happiness or budget
            if(happiness <= 0 || energy <= 0 || budget <= 0)
            {
                ChangeState(new CharacterStateLeaving(this));
            }
        }
    }

    public void EnterQueue()
    {
        isInQueue = true;
    }

    public void LeaveQueue()
    {
        isInQueue = false;
    }

    public void PurchaseItem(RoomBuildingParams shop, int cost)
    {
        budget -= cost;
        if(PlayerController.Instance) PlayerController.Instance.AddMoney(cost);
        shopsInteractedWith.Add(shop);
        AkSoundEngine.PostEvent("cashier", this.gameObject);
    }

    public void PurchaseItemFromObject(ItemObjectInteractable itemObject)
    {
        budget -= itemObject.InteractCost;
        if (PlayerController.Instance) PlayerController.Instance.AddMoney(itemObject.InteractCost);
    }

    public void StealItem(RoomBuildingParams shop, int cost)
    {
        amountStolen += cost;
        shopsInteractedWith.Add(shop);
        AkSoundEngine.PostEvent("stealing", this.gameObject);
    }

    public void CaughtStealing()
    {
        amountStolen = 0;
        if (!IsInState<CharacterStateLeaving>())
        {
            ChangeState(new CharacterStateLeaving(this));
        }
    }

    protected override void OnEnterNewCell(Cell cellLeaving, Cell cellEntering)
    {
        base.OnEnterNewCell(cellLeaving, cellEntering);
        cellEntering.customerCount++;

        if (cellEntering != null && cellLeaving != null && cellEntering.room != cellLeaving.room)
        {
            if (cellLeaving != null && cellLeaving.room != null)
            {
                cellLeaving.room.OnCustomerLeaveRoom();
            }

            if (cellEntering != null && cellEntering.room != null)
            {
                cellEntering.room.OnCustomerEnterRoom();
            }
        }
    }

    private void increaseInterestInShop(RoomBuildingParams room, float amount)
    {
        if (shopInterest.ContainsKey(room))
        {
            shopInterest[room] += amount;
        }
        else
        {
            shopInterest.Add(room, amount);
        }

        //Debug.LogFormat("Interest in {0}: {1}", room.name, shopInterest[room]);
    }

    protected override void onLeaveMall()
    {
        base.onLeaveMall();

        if (AmountStolen > 0)
        {
            PlayerController.Instance.RemoveMoney(amountStolen);
        }
    }

    void OnDestroy()
    {
        if(CustomerManager.Instance)
        {
            CustomerManager.Instance.RemoveCustomer(this);
        }
    }

    // Every X seconds if a bit of trash is in range and line of sight then decrease happiness
    private IEnumerator seeTrashIEnum()
    {
        while(true)
        {
            while(isInQueue) yield return null;

            ItemObject[] trashObjectsInRange = Mall.CurrentFloor.FindItemObjectsInRange<ItemObject>(transform.position, Params.SeeTrashRange, ItemObject.ObjectType.Trash);
            float totalHappinessDecrease = trashObjectsInRange.Length * Params.SeeTrashHappinessDecrease;
            if(totalHappinessDecrease > 20)
            {
                totalHappinessDecrease = 20;
            }

            happiness -= totalHappinessDecrease;

            yield return new WaitForSeconds(Params.SeeTrashInterval);
        }
    }

    // Every X seconds if a decoration is in range and line of sight then decrease happiness
    private IEnumerator seeDecorationIEnum()
    {
        while(true)
        {
            while(isInQueue) yield return null;

            ItemObject[] decorationObjectsInRange = Mall.CurrentFloor.FindItemObjectsInRange<ItemObject>(transform.position, Params.SeeDecorationRange, ItemObject.ObjectType.Decoration);
            float totalHappinessIncrease = decorationObjectsInRange.Length * Params.SeeDecorationHappinessIncrease;
            if(totalHappinessIncrease > 20)
            {
                totalHappinessIncrease = 20;
            }

            happiness += totalHappinessIncrease;

            yield return new WaitForSeconds(Params.SeeDecorationInterval);
        }
    }

    private IEnumerator playCustomerSounds()
    {
        float interval = 6.0f;

        while(true)
        {
            if(happiness < 20)
            {
                AkSoundEngine.PostEvent("angryCustomers", this.gameObject);
            }
            if(isBrowsing)
            {
                AkSoundEngine.PostEvent("ponderingCustomers", this.gameObject);
            }
            yield return new WaitForSeconds(interval);
        }
    }
}