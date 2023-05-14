using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CharacterStateWander : CharacterState
{
    private Coroutine wanderCoroutine;

    public CharacterStateWander(Character character) : base(character)
    {
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        wanderCoroutine = character.StartCoroutine(wanderIEnum());
    }

    public override void OnExitState()
    {
        base.OnExitState();

        character.StopCoroutine(wanderCoroutine);
        wanderCoroutine = null;
    }

    // Every 5 seconds pick a random point to walk to
    private IEnumerator wanderIEnum()
    {
        while (true)
        {
            Vector3 pos;
            do
            {
                pos = MathUtil.GetRandomPointWithAngleDistance(character.transform.position, 2.0f, 10.0f);
                yield return null;
            }
            while (Mall.CurrentFloor.GetCellFromWorldPos(pos) == null);

            character.Movement.SetDestination(pos);

            yield return new WaitForSeconds(4.0f);
        }
    }

    public override string GetCurrentStateText()
    {
        return "Wandering";
    }
}