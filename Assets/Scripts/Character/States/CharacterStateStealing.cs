using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterStateStealing : CharacterState
{
    private RoomBuildingParams roomBuildingParams;
    private Coroutine stealCoroutine;

    private ItemObjectTill till;
    private Room currentRoom;

    public CharacterStateStealing(Customer customer, RoomBuildingParams roomBuildingParams) : base(customer)
    {
        this.roomBuildingParams = roomBuildingParams;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        ItemObjectTill till = null;
        Room[] rooms = Mall.CurrentFloor.FindRoomsOfType(roomBuildingParams);

        //if (rooms.Length > 0)
        //{
        //    //IOrderedEnumerable<Room> orderedRooms = rooms.OrderBy(o =>
        //    //{
        //    //    BuildingPieceObject nearestEntrance = o.FindNearestEntrance(character.transform.position);
        //    //    if (nearestEntrance)
        //    //    {
        //    //        return Vector3.Distance(nearestEntrance.cellEdge.GetCentrePosition(), character.transform.position);
        //    //    }
        //    //    return int.MaxValue;
        //    //});

        Room chosenRoom = character.CurrentCell.room;

        if (chosenRoom)
        {
            ItemObjectTill[] tills = chosenRoom.FindItemObjects<ItemObjectTill>();
            if (tills.Length > 0)
            {
                till = tills[Random.Range(0, tills.Length)];
            }
        }
        else
        {
            Debug.LogWarning("Something funny's going on here! A null element is in the room list");
        }


        if (till != null && till.CustomerCell != null)
        {
            stealCoroutine = character.StartCoroutine(stealIEnum(till));
        }
        // Leave if could not find till
        else
        {
            character.ChangeState(new CharacterStateLeaving(character));
        }
    }

    public override void OnExitState()
    {
        base.OnExitState();

        Customer customer = character as Customer;

        if (stealCoroutine != null)
        {
            character.StopCoroutine(stealCoroutine);
            stealCoroutine = null;
        }

        if (till)
        {
            till.RemoveCustomerFromQueue(customer);
            character.Movement.StopLookingAt();
        }
    }

    // Wait until the customer reaches the till, wait for 5 seconds then exit the mall
    private IEnumerator stealIEnum(ItemObjectTill till)
    {
        if (till == null)
        {
            yield break;
        }

        this.till = till;

        Customer customer = character as Customer;

        //ItemObject[] keyObjects = currentRoom.FindItemObjects<ItemObjectKey>();

        //Cell[] availableCells = null;

        //foreach (ItemObjectKey keyObject in keyObjects)
        //{
        //    availableCells = keyObject.GetSurroundingCells().Where(o => o.room == currentRoom && o.itemObject == null).ToArray();
        //}
        //Cell targetCell = null;
        //// Move the character to the available cell
        //if(availableCells == null)
        //{
        //    yield break;
        //}
        //else
        //{
        //    targetCell = availableCells[Random.Range(0, availableCells.Length)];
        //}


        //if (targetCell != null)
        //{
        //    customer.Movement.SetDestination(targetCell.GetCentrePosition());
        //}

        //// When the customer is in range of the available cell, look at the assistant and wait for X seconds
        //// If they get pushed away from the till, don't loose all progress
        //float timeSpentInRange = 0;
        //while (timeSpentInRange < 4)
        //{
        //    bool inRange = Vector3.Distance(customer.transform.position, targetCell.GetCentrePosition()) < 0.5f;

        //    if (inRange)
        //    {
        //        customer.Movement.LookAt(currentRoom.GetComponentInParent<Floor>().FindNearestObjectOfType<ItemObjectKey>(customer.transform.position).cell.GetCentrePosition());
        //        timeSpentInRange += Time.deltaTime;
        //    }
        //    else
        //    {
        //        customer.Movement.StopLookingAt();
        //    }
        //    yield return null;
        //}

        SecurityGuard[] alertedGuards = EmployeeManager.Instance.GetAllEmployeesOfType<SecurityGuard>();

        for (int i = 0; i < alertedGuards.Length; i++)
        {
            if (CharacterInteract.CloseToCharacter(character, alertedGuards[i], 10f) && alertedGuards[i].IsInState<CharacterStatePatrol>())
            {
                alertedGuards[0].ChangeState(new CharacterStateChasing(alertedGuards[0], customer));
                break;
            }
        }

        customer.StealItem(till.cell.room.RoomParams, till.GetProductCost());
        customer.ChangeState(new CharacterStateLeaving(customer));

        this.till = null;

    }

    public override string GetCurrentStateText()
    {
        return "Acting suspicious";
    }
}