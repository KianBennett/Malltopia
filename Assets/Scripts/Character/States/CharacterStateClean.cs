using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterStateClean : CharacterState
{
    private Coroutine lookForTrashCoroutine;
    private float initDelay;

    public CharacterStateClean(Janitor janitor, float initDelay = .0f) : base(janitor)
    {
        this.initDelay = initDelay;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (lookForTrashCoroutine != null) character.StopCoroutine(lookForTrashCoroutine);
        lookForTrashCoroutine = character.StartCoroutine(cleanIEnum());
    }

    public override void OnExitState()
    {
        base.OnExitState();

        if (lookForTrashCoroutine != null) character.StopCoroutine(lookForTrashCoroutine);

        character.Movement.StopLookingAt();
    }

    private IEnumerator cleanIEnum()
    {
        yield return new WaitForSeconds(initDelay);

        while (true)
        {
            ItemObjectBin nearestDirtyBin = Mall.CurrentFloor.FindItemObjectsOfType<ItemObjectBin>()
                .Where(o => o.IsDirty)
                .OrderBy(o => Vector3.Distance(character.transform.position, o.transform.position))
                .FirstOrDefault();

            ItemObjectTrash nearestTrashObject = TrashManager.Instance.GetClosestTrashObject(character.transform.position);

            // If there's a bin and no trash
            if(nearestDirtyBin != null && nearestTrashObject == null)
            {
                yield return cleanBinIEnum(nearestDirtyBin);
            }
            // If there's trash and no bin
            else if(nearestTrashObject != null && nearestDirtyBin == null)
            {
                yield return pickUpTrashIEnum(nearestTrashObject);
            }
            // If there's both then pick the closest one
            else if(nearestTrashObject != null && nearestDirtyBin != null)
            {
                float distToBin = character.DistToPoint(nearestDirtyBin.transform.position);
                float distToTrashObject = character.DistToPoint(nearestTrashObject.transform.position);

                if(distToBin < distToTrashObject)
                {
                    yield return cleanBinIEnum(nearestDirtyBin);
                }
                else
                {
                    yield return pickUpTrashIEnum(nearestTrashObject);
                }
            }
            // If there is nothing to clean then walk around randomly for a few seconds before checking again
            else
            {
                Vector3 pos = MathUtil.GetRandomPointWithAngleDistance(character.transform.position, 4.0f, 20.0f);
                character.Movement.SetDestination(pos);

                yield return new WaitForSeconds(6.0f);
            }
        }
    }

    private IEnumerator pickUpTrashIEnum(ItemObjectTrash trashObject)
    {
        character.Movement.SetDestination(trashObject.TrashPosition);
        trashObject.SetCharacterToClean(character);

        yield return new WaitUntil(() =>
        {
            return trashObject == null || character.DistToPoint(trashObject.TrashPosition) < 0.75f;
        });

        // If the trash has been removed while the janitor is on the way skip it and look for another one
        if (trashObject == null)
        {
            yield break;
        }

        character.Movement.StopMoving();
        character.Movement.LookAt(trashObject.TrashPosition);

        yield return new WaitForSeconds(1.2f);

        character.ModelAnimator.SetTrigger("PickUpTrash");

        yield return new WaitForSeconds(0.7f);

        character.GetComponent<Employee>().ReduceEnergy(10);
        TrashManager.Instance.RemoveTrashObject(trashObject);

        yield return new WaitForSeconds(2.0f);
        character.Movement.StopLookingAt();
    }
    
    private IEnumerator cleanBinIEnum(ItemObjectBin binObject)
    {
        Cell nearestEmptySurroundingCell = binObject.GetSurroundingCells(false)
                                                .Where(o => o.itemObject == null)
                                                .OrderBy(o => Vector3.Distance(character.transform.position, o.GetCentrePosition()))
                                                .FirstOrDefault();

        if (nearestEmptySurroundingCell == null)
        {
            yield break;
        }

        character.Movement.SetDestination(nearestEmptySurroundingCell.GetCentrePosition());

        yield return new WaitUntil(() =>
        {
            return binObject == null 
                || nearestEmptySurroundingCell.itemObject 
                || character.Movement.GetRemainingDistance() < 0.2f;
        });

        // If the bin gets destroyed or target cell gets filled while moving towards it
        if (binObject == null || nearestEmptySurroundingCell.itemObject)
        {
            yield break;
        }

        character.Movement.LookAt(binObject.transform.position);

        yield return new WaitForSeconds(2.0f);

        if(binObject)
        {
            binObject.Clean();
        }

        yield return new WaitForSeconds(0.5f);

        character.Movement.StopLookingAt();
    }

    public override string GetCurrentStateText()
    {
        return "Cleaning";
    }
}
