using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class CharacterStatePatrol : CharacterState
{
    private Coroutine patrolCoroutine;
    private float initDelay;

    List<Room> rooms;

    public CharacterStatePatrol(SecurityGuard securityGuard, float initDelay = .0f) : base(securityGuard)
    {
        this.initDelay = initDelay;
    }
    public override void OnEnterState()
    {
        base.OnEnterState();

        CheckForRooms();

        if (patrolCoroutine != null) character.StopCoroutine(patrolCoroutine);

        patrolCoroutine = character.StartCoroutine(patrolIEnum());
    }

    public override void OnExitState()
    {
        base.OnExitState();

        if (patrolCoroutine != null) character.StopCoroutine(patrolCoroutine);
    }

    private IEnumerator patrolIEnum()
    {
        yield return new WaitForSeconds(initDelay);

        Employee employee = character as Employee;

        while (true)
        {
            if(rooms.Count > 0)
            {
                Cell targetCell = null;

                List<Cell> emptyCells = rooms[0].GetRoomEmptyCells().ToList();
                targetCell = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];

                if (targetCell != null)
                {
                    employee.Movement.SetDestination(targetCell.GetCentrePosition());
                }

                float timeSpentInRange = 0;
                while (timeSpentInRange < 10)
                {
                    bool inRange = Vector3.Distance(employee.transform.position, targetCell.GetCentrePosition()) < 0.5f;

                    if (inRange)
                    {
                        employee.Movement.LookAt(rooms[0].FindRandomItemObject<ItemObjectTill>().transform.position);
                        timeSpentInRange += Time.deltaTime;
                    }
                    else
                    {
                        employee.Movement.StopLookingAt();
                    }
                    yield return null;
                }
                character.GetComponent<Employee>().ReduceEnergy(10);
                rooms.RemoveAt(0);

            }
            else
            {
                CheckForRooms();
                yield return null;
            }

            yield return new WaitForSeconds(10f);
        }
    }

    private void CheckForRooms()
    {
        rooms = Mall.CurrentFloor.FindShops().ToList();

        if (rooms.Count > 0)
        {
            IOrderedEnumerable<Room> orderedRooms = rooms.OrderBy(o =>
            {
                BuildingPieceObject nearestEntrance = o.FindNearestEntrance(character.transform.position);
                if (nearestEntrance)
                {
                    return Vector3.Distance(nearestEntrance.cellEdge.GetCentrePosition(), character.transform.position);
                }
                return int.MaxValue;
            });
        }
    }

    public override string GetCurrentStateText()
    {
        return "Patrolling";
    }
}
