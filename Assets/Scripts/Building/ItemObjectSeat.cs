using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ItemObjectSeat : ItemObject
{
    [Serializable]
    public class SeatEntrance
    {
        public Transform entrancePoint;
        public Direction directionFromSeat;

        public bool IsBlocked(Cell seatCell)
        {
            return seatCell.GetNeighbour(directionFromSeat).itemObject != null || 
                seatCell.GetEdge(directionFromSeat).buildingPieceObject != null;
        }
    }

    [Serializable]
    public class SittingPoint
    {
        public ItemObjectSeat seat;
        public Transform seatPosition;
        public Vector2Int seatCellLocal;
        public SeatEntrance[] entrances;

        [HideInInspector] public Character seatedCharacter;

        public Cell GetSeatCell()
        {
            return Mall.CurrentFloor.GetCell(seat.cell.Coords + seatCellLocal);
        }

        //public Cell GetNearestUnblockedEntranceCell(Vector3 pos)
        //{
        //    Cell seatCell = GetSeatCell();
        //    return entranceCellDirections
        //        .Where(o => seatCell.GetEdge(o).buildingPieceObject == null)
        //        .Select(o => seatCell.GetNeighbour(Mall.GetDirectionRelativeToDirection(o, seat.directionFacing)))
        //        .Where(o => o.itemObject == null)
        //        .OrderBy(o => Vector3.Distance(pos, o.GetCentrePosition()))
        //        .FirstOrDefault();
        //}

        //public bool IsEntranceBlocked(Direction direction)
        //{
        //    Cell seatCell = GetSeatCell();
        //    return seatCell.GetNeighbour(direction).itemObject == null && seatCell.GetEdge(direction).buildingPieceObject == null;
        //}

        //public Vector3 GetPositionForEntrance(Direction entranceCellDirection)
        //{
        //    int index = Array.IndexOf(entranceCellDirections, entranceCellDirection);
        //    if(index < 0 || index > entrancePositions.Length - 1)
        //    {
        //        Debug.LogWarning("This sitting position doesn't have an entrance from that direction");
        //        return default;
        //    }

        //    return entrancePositions[index].position;
        //}

        public SeatEntrance GetNearestUnblockedEntrance(Vector3 pos)
        {
            Cell seatCell = GetSeatCell();
            return entrances
                .Where(o => !o.IsBlocked(seatCell))
                .OrderBy(o => Vector3.Distance(pos, o.entrancePoint.position))
                .FirstOrDefault();
        }

        public void OnCharacterSit(Character character)
        {
            seatedCharacter = character;
        }

        public void OnCharacterStand()
        {
            seatedCharacter = null;
        }

        public bool IsVacant()
        {
            return seatedCharacter == null;
        }

        public bool AreAllEntrancesBlocked()
        {
            Cell seatCell = GetSeatCell();
            return entrances.Where(o => !o.IsBlocked(seatCell)).Count() == 0;
        }
    }

    [SerializeField] private SittingPoint[] sittingPoints;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // If a character is sitting on the seat when it's destroyed, kick them out of the sitting state
        foreach(SittingPoint sittingPoint in sittingPoints)
        {
            if(sittingPoint.seatedCharacter != null)
            {
                sittingPoint.seatedCharacter.ChangeState(new CharacterStateWander(sittingPoint.seatedCharacter));
            }
        }
    }

    public bool HasVacantSittingPoint()
    {
        return sittingPoints.Where(o => o.IsVacant() && !o.AreAllEntrancesBlocked()).Any();
    }

    public SittingPoint GetNearestUnnocupiedSittingPoint(Vector3 position)
    {
        return sittingPoints
            .Where(o => o.IsVacant())
            .OrderBy(o => Vector3.Distance(o.GetSeatCell().GetCentrePosition(), position))
            .FirstOrDefault();
    }
}
