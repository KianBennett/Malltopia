using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static public class CharacterInteract
{
    public static Cell targetCell = null;
    public static Cell[] targetCells = null;
    public enum MoveToPointEnum
    {
        CenterOfCell,
        RandomSurroundingCell,
        SpecificSurroundingCell,
        NearestSurroundingCell
    }
    public static void MoveToObject(Character character, ItemObjectInteractable interactable, MoveToPointEnum moveType, bool isRunning = false)
    {
        targetCell = null;
        targetCells = null;

        switch (moveType)
        {
            case MoveToPointEnum.CenterOfCell:
                targetCell = interactable.cell;
                character.Movement.SetDestination(targetCell.GetCentrePosition(), isRunning);
                break;

            case MoveToPointEnum.RandomSurroundingCell:
                targetCells = interactable.GetSurroundingCells();
                targetCell = targetCells[Random.Range(0, targetCells.Length)];
                character.Movement.SetDestination(targetCell.GetCentrePosition(), isRunning);
                break;

            case MoveToPointEnum.SpecificSurroundingCell:
                Direction customerDirection = (Direction)MathUtil.Mod((int)interactable.DirectionFacing + 2, 4);
                targetCell = Mall.CurrentFloor.GetCell(interactable.cell.Coords + Mall.GetCellDirectionOffset(customerDirection));
                character.Movement.SetDestination(targetCell.GetCentrePosition(), isRunning);
                break;

            case MoveToPointEnum.NearestSurroundingCell:
                targetCells = interactable.GetSurroundingCells();
                targetCell = targetCells.OrderBy(o => Vector3.Distance(character.transform.position, o.GetCentrePosition())).FirstOrDefault();
                if(targetCell != null)
                {
                    character.Movement.SetDestination(targetCell.GetCentrePosition(), isRunning);
                }
                break;

            default:
                Debug.Log("MoveToObject somehow had no moveType associated with it");
                break;
        }
    }

    public static bool CloseToObject(Character character, ItemObjectInteractable interactable, float distanceRequired)
    {
        float distToTarget = Vector3.Distance(character.transform.position, interactable.cell.GetCentrePosition());

        if (distToTarget <= distanceRequired)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool CloseToCharacter(Character character, Character target, float distanceRequired)
    {
        if (character == null || target == null)
        {
            return false;
        }

        float distToTarget = Vector3.Distance(character.transform.position, target.transform.position);

        if (distToTarget <= distanceRequired)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
