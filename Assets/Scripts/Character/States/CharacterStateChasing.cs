using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CharacterStateChasing : CharacterState
{
    private Coroutine chasingCoroutine;
    private Customer target;
    private bool hasCaughtThief;

    public CharacterStateChasing(SecurityGuard securityGuard, Customer customer) : base(securityGuard)
    {
        this.target = customer;
    }

public override void OnEnterState()
    {
        base.OnEnterState();

        chasingCoroutine = character.StartCoroutine(chaseIEnum());

        Debug.Log(character.CharacterName + " changed to chase state and my target is " + target.CharacterName);
    }

    public override void OnExitState()
    {
        base.OnExitState();

        if (chasingCoroutine != null) character.StopCoroutine(chasingCoroutine);
    }

    private IEnumerator chaseIEnum()
    {
        while(true)
        {
            Employee employee = character as Employee;

            if (target == null)
            {
                Debug.Log("Thief Left");
                employee.Movement.StopLookingAt();
                character.GetComponent<Employee>().SetEnergy(0);
                yield break;
            }

            employee.Movement.LookAt(target.transform.position);

            if (!CharacterInteract.CloseToCharacter(character, target, 1f))
            {
                if (hasCaughtThief)
                {
                    employee.Movement.SetDestination(target.transform.position, false);
                }
                else
                {
                    employee.Movement.SetDestination(target.transform.position, true);
                }
            }
            else
            {
                target.CaughtStealing();
                hasCaughtThief = true;
                Debug.Log("Caught Thief");
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public override string GetCurrentStateText()
    {
        return "Chasing a thief";
    }
}
