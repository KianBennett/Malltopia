using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterStateBreak : CharacterState
{
    private Coroutine breakCoroutine;

    private Room staffRoom;
    public ItemObjectSeat.SittingPoint sittingPoint;
    public ItemObjectSeat.SeatEntrance seatEntrance;
    private bool hasStartedSitting;

    public CharacterStateBreak(Employee employee) : base(employee)
    {
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (breakCoroutine != null) character.StopCoroutine(breakCoroutine);
        character.StartCoroutine(breakIEnum());
    }

    public override void OnUpdateState()
    {
        base.OnUpdateState();

        // If the staff room is destroyed then reset the state
        if(staffRoom == null)
        {
            character.ChangeState(new CharacterStateBreak(character as Employee));
        }

        CharacterStateSitting.UpdateSitting(character, sittingPoint, seatEntrance, hasStartedSitting);
    }

    public override void OnExitState()
    {
        base.OnExitState();

        if (breakCoroutine != null) character.StopCoroutine(breakCoroutine);

        if (sittingPoint != null && seatEntrance != null && hasStartedSitting)
        {
            character.StartCoroutine(CharacterStateSitting.LeaveSeat(character, sittingPoint, seatEntrance));
        }
    }

    private IEnumerator breakIEnum()
    {
        staffRoom = Mall.CurrentFloor.FindNearestRoomOfType((character as Employee).StaffRoomBuilding, character.transform.position);

        // If there is no staff room available then leave the mall
        if(staffRoom == null )
        {
            character.ChangeState(new CharacterStateEmployeeLeaving(character));
            yield break;
        }

        // Move to a random cell in the staff room
        Cell[] emptyCells = staffRoom.cells.Where(o => o.itemObject == null).ToArray();
        if (emptyCells.Length > 0)
        {
            Cell cell = emptyCells[Random.Range(0, emptyCells.Length)];
            character.Movement.SetDestination(cell.GetCentrePosition());
        }
        else
        {
            // If there are no empty cells in the room then leave
            character.ChangeState(new CharacterStateEmployeeLeaving(character));
            yield break;
        }

        // Wait until the character enters the room
        yield return new WaitUntil(() =>
        {
            return character.CurrentCell.room == staffRoom;
        });

        while(sittingPoint == null || seatEntrance == null)
        {
            sittingPoint = staffRoom.FindNearestVacantSittingPoint(character.transform.position);
            seatEntrance = sittingPoint?.GetNearestUnblockedEntrance(character.transform.position);
            yield return null;
        }

        yield return CharacterStateSitting.UseSeatIEnum(character, sittingPoint, seatEntrance, delegate { hasStartedSitting = true; });
    }

    public override string GetCurrentStateText()
    {
        return "On their break";
    }
}