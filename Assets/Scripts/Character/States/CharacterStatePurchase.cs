using System.Collections;
using System.Linq;
using UnityEngine;

public class CharacterStatePurchase : CharacterState
{
    private RoomBuildingParams roomBuildingParams;
    private Coroutine purchaseCoroutine;

    private ItemObjectTill till;
    private Room currentRoom;
    private bool hasStartedPurchaseSequence;

    public CharacterStatePurchase(Customer customer, RoomBuildingParams roomBuildingParams) : base(customer)
    {
        this.roomBuildingParams = roomBuildingParams;
        hasStartedPurchaseSequence = false;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        ItemObjectTill till = null;
        Room[] rooms = Mall.CurrentFloor.FindRoomsOfType(roomBuildingParams);

        if(rooms.Length > 0)
        {
            //IOrderedEnumerable<Room> orderedRooms = rooms.OrderBy(o =>
            //{
            //    BuildingPieceObject nearestEntrance = o.FindNearestEntrance(character.transform.position);
            //    if(nearestEntrance)
            //    {
            //        return Vector3.Distance(nearestEntrance.cellEdge.GetCentrePosition(), character.transform.position);
            //    }
            //    return int.MaxValue;
            //});

            //Room nearestRoom = orderedRooms.First();

            Room chosenRoom = MathUtil.RandomElementOfArray(rooms);
            currentRoom = chosenRoom;

            if (chosenRoom)
            {
                ItemObjectTill[] tills = chosenRoom.FindItemObjects<ItemObjectTill>();
                if(tills.Length > 0)
                {
                    till = tills[Random.Range(0, tills.Length)];
                }
            }
            else
            {
                Debug.LogWarning("Something funny's going on here! A null element is in the room list");
            }
        }

        if(till != null && till.CustomerCell != null)
        {
            purchaseCoroutine = character.StartCoroutine(purchaseIEnum(till));
        }
        // Leave if could not find till
        else
        {
            character.ChangeState(new CharacterStateLeaving(character));
        }
    }

    public override void OnUpdateState()
    {
        base.OnUpdateState();

        if (hasStartedPurchaseSequence && (till == null || currentRoom == null))
        {
            character.ChangeState(new CharacterStateWander(character));
        }

        // As a failsafe if we spend ages in this state then exit out of it to avoid getting stuck
        if(timeSpentInState > 120)
        {
            character.ChangeState(new CharacterStateWander(character));
        }
    }

    public override void OnExitState()
    {
        base.OnExitState();

        Customer customer = character as Customer;

        if(purchaseCoroutine != null)
        {
            character.StopCoroutine(purchaseCoroutine);
            purchaseCoroutine = null;
        }

        if(till)
        {
            till.RemoveCustomerFromQueue(customer);
        }

        character.Movement.StopLookingAt();
    }

    // Wait until the customer reaches the till, wait for 5 seconds then exit the mall
    private IEnumerator purchaseIEnum(ItemObjectTill till)
    {
        if(till == null || currentRoom == null)
        {
            character.ChangeState(new CharacterStateWander(character));
            yield break;
        }

        this.till = till;

        Customer customer = character as Customer;

        hasStartedPurchaseSequence = true;

        //Go look at item before purchase
        ItemObject[] keyObjects = currentRoom.FindItemObjects<ItemObjectKey>();

        Cell targetCell = null;
        Cell[] availableCells = null;

        foreach (ItemObjectKey keyObject in keyObjects)
        {
            availableCells = keyObject.GetSurroundingCells().Where(o => o.room == currentRoom && o.itemObject == null).ToArray();
        }
        // Move the character to the available cell
        if (availableCells == null)
        {
            character.ChangeState(new CharacterStateWander(character));
            yield break;
        }
        else
        {
            targetCell = availableCells[Random.Range(0, availableCells.Length)];
        }

        if (targetCell != null)
        {
            customer.Movement.SetDestination(targetCell.GetRandomPosition());
        }

        // When the customer is in range of the available cell, look at the object and wait for X seconds
        float timeSpentInRange = 0;
        float waitTime = Random.Range(10, 30);

        while (timeSpentInRange < waitTime)
        {
            bool inRange = Vector3.Distance(customer.transform.position, targetCell.GetCentrePosition()) < 0.5f;

            if (inRange)
            {
                customer.Movement.LookAt(currentRoom.GetComponentInParent<Floor>().FindNearestObjectOfType<ItemObjectKey>(customer.transform.position).cell.GetCentrePosition());
                timeSpentInRange += Time.deltaTime;
            }
            else
            {
                customer.Movement.StopLookingAt();
            }
            yield return null;
        }

        customer.ModelAnimator.SetTrigger("GrabFromShelf");
        yield return new WaitForSeconds(1.5f);

        //Decide whether to steal the item or not
        if (customer.Thief <= 20 || customer.Happiness <= 20)
        {
            customer.ChangeState(new CharacterStateStealing(customer, this.roomBuildingParams));
            yield break;
        }

        customer.Movement.StopLookingAt();

        // Move the character to the end of the queue
        targetCell = till.GetNextEmptyCellForQueue();
        if(targetCell != null)
        {
            customer.Movement.SetDestination(targetCell.GetCentrePosition());
        }

        // If this queue changes, change destination to the new end cell
        while (targetCell == null || !customer.Movement.HasPath || customer.Movement.GetRemainingDistance() > 0.5f)
        {
            Cell newTargetCell = till.GetNextEmptyCellForQueue();
            if(newTargetCell != targetCell)
            {
                targetCell = newTargetCell;
                customer.Movement.SetDestination(targetCell.GetCentrePosition());
            }

            // If the till has been destroyed then cancel and go back to wandering
            if(!till)
            {
                character.ChangeState(new CharacterStateWander(character));
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        // When in position at the end of the queue, join the queue
        till.AddCustomerToQueue(customer);
            
        // When the customer is in range of the till, look at the assistant and wait for X seconds
        // If they get pushed away from the till, don't loose all progress
        timeSpentInRange = 0;
        while(timeSpentInRange < 4)
        {
            // If the till has been destroyed then cancel and go back to wandering
            if (!till)
            {
                character.ChangeState(new CharacterStateWander(character));
                yield break;
            }

            bool inRange = Vector3.Distance(customer.transform.position, till.CustomerCell.GetCentrePosition()) < 0.5f;
            bool inRangeOfQueuePosition = customer.Movement.GetRemainingDistance() < 0.5f;

            if (inRange)
            {
                customer.Movement.LookAt(till.AssistantCell.GetCentrePosition());
                timeSpentInRange += Time.deltaTime;
            }
            else if (inRangeOfQueuePosition)
            {
                Cell previousCellInQueue = till.GetPreviousCellInQueueForCustomer(customer);
                if (previousCellInQueue != null)
                {
                    customer.Movement.LookAt(previousCellInQueue.GetCentrePosition());
                }
            }
            else
            {
                customer.Movement.StopLookingAt();
            }
            yield return null;
        }

        customer.PurchaseItem(till.cell.room.RoomParams, till.GetProductCost());
        customer.ChangeState(new CharacterStateWander(customer));

        this.till = null;
    }

    public override string GetCurrentStateText()
    {
        if(currentRoom != null)
        {
            if (character.CurrentCell != null && character.CurrentCell.room == currentRoom)
            {
                return "Shopping in the " + currentRoom.RoomParams.Name;
            }
            else
            {
                return "Looking for the " + currentRoom.RoomParams.Name;
            }
        }
        
        return "Looking for something to buy";
    }
}