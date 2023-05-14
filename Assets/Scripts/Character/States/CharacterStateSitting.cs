using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterStateSitting : CharacterState
{
    public ItemObjectSeat.SittingPoint sittingPoint;
    public ItemObjectSeat.SeatEntrance seatEntrance;
    private bool hasStartedSitting;
    private float timeSinceLastBlockedNotification;

    private Coroutine goToSeatCoroutine;

    public CharacterStateSitting(Character character) : base(character)
    {
        hasStartedSitting = false;
        timeSinceLastBlockedNotification = 0;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (goToSeatCoroutine != null) character.StopCoroutine(goToSeatCoroutine);
        goToSeatCoroutine = character.StartCoroutine(findNearestSeatIEnum());
    }

    public override void OnUpdateState()
    {
        base.OnUpdateState();

        // As a failsafe if a character has spent more than 100 seconds sitting (should never happen) then exit the state
        if (timeSpentInState > 100)
        {
            character.ChangeState(new CharacterStateWander(character));
        }

        // Every 20 seconds send a notification if the seat is blocked
        timeSinceLastBlockedNotification += Time.deltaTime;
        if (sittingPoint != null && sittingPoint.AreAllEntrancesBlocked() && timeSinceLastBlockedNotification > 20)
        {
            HUD.Instance.Notifications.ShowNotification("An object is blocking a seat!", delegate { CameraController.Instance.SetPositionImmediate(sittingPoint.seat.transform.position); });
        }

        UpdateSitting(character, sittingPoint, seatEntrance, hasStartedSitting);
    }

    public override void OnExitState()
    {
        base.OnExitState();

        if (goToSeatCoroutine != null) character.StopCoroutine(goToSeatCoroutine);

        if (sittingPoint != null && seatEntrance != null && hasStartedSitting)
        {
            character.StartCoroutine(LeaveSeat(character, sittingPoint, seatEntrance));
        }
    }

    private IEnumerator findNearestSeatIEnum()
    {
        character.Movement.StopMoving();

        // Wait until we find the nearest vacant seat
        while (sittingPoint == null || seatEntrance == null)
        {
            // If a seat hasn't already been defined then wait until one becomes free
            sittingPoint = Mall.CurrentFloor.FindNearestVacantSittingPoint(character.transform.position);
            seatEntrance = sittingPoint?.GetNearestUnblockedEntrance(character.transform.position);
            yield return null;
        }

        yield return UseSeatIEnum(character, sittingPoint, seatEntrance, delegate { hasStartedSitting = true; });
    }

    public static IEnumerator UseSeatIEnum(Character character, ItemObjectSeat.SittingPoint sittingPoint, ItemObjectSeat.SeatEntrance seatEntrance, UnityAction onEnterSeat)
    {
        // Move to the seat entrance
        character.Movement.SetDestination(seatEntrance.entrancePoint.position);

        // Wait until the character is close enough
        yield return new WaitUntil(() =>
        {
            return character.DistToPoint(seatEntrance.entrancePoint.position) < 0.2f;
        });

        // Start sitting down and occupy the seat
        onEnterSeat?.Invoke();
        sittingPoint.OnCharacterSit(character);

        // Look away from the seat
        character.Movement.LookAt(seatEntrance.entrancePoint.position + (seatEntrance.entrancePoint.position - sittingPoint.seatPosition.position).normalized);

        yield return new WaitForSeconds(0.5f);

        // Play the sitting animation
        character.ModelAnimator.SetTrigger("Sit");
        character.SetSitting(true);

        // Move the character (non-physics) towards the seat position
        while (character.DistToPoint(sittingPoint.seatPosition.position) > 0.1f)
        {
            character.transform.position = Vector3.MoveTowards(character.transform.position, sittingPoint.seatPosition.position, Time.deltaTime * 0.8f);
            yield return null;
        }
    }

    public static void UpdateSitting(Character character, ItemObjectSeat.SittingPoint sittingPoint, ItemObjectSeat.SeatEntrance seatEntrance, bool hasStartedSitting)
    {
        // If this seat is destroyed while the character is sitting on it
        if (hasStartedSitting && (sittingPoint == null || sittingPoint.seat == null))
        {
            character.ChangeState(new CharacterStateWander(character));
        }

        // If the seat becomes taken or blocked then reset the state
        if (!hasStartedSitting && sittingPoint != null && seatEntrance != null)
        {
            bool sittingPointHasBeenTaken = !sittingPoint.IsVacant();
            bool entranceIsBlocked = seatEntrance.IsBlocked(sittingPoint.GetSeatCell());
            if (sittingPointHasBeenTaken || entranceIsBlocked)
            {
                character.ChangeState(new CharacterStateSitting(character));
            }
        }
    }

    public static IEnumerator LeaveSeat(Character character, ItemObjectSeat.SittingPoint sittingPoint, ItemObjectSeat.SeatEntrance seatEntrance)
    {
        if (sittingPoint == null || seatEntrance == null)
        {
            Debug.LogWarning("Trying to leave a null sitting point!");
            yield break;
        }

        // Play standing (reverse sitting) animation
        character.ModelAnimator.SetTrigger("Stand");

        yield return new WaitForSeconds(0.3f);

        // Move towards the entrance position
        float tick = 0.0f;
        while (tick < 1.8f)
        {
            character.transform.position = Vector3.MoveTowards(character.transform.position, seatEntrance.entrancePoint.position, Time.deltaTime * 0.5f);
            tick += Time.deltaTime;
            yield return null;
        }

        // Remove character from the occupying the seat and restore ability to move/look
        character.SetSitting(false);
        character.Movement.StopLookingAt();
        sittingPoint.OnCharacterStand();
    }

    public override string GetCurrentStateText()
    {
        return "Sitting";
    }
}