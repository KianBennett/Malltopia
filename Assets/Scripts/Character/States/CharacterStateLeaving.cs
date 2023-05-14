using UnityEngine;

public class CharacterStateLeaving : CharacterState
{
    public CharacterStateLeaving(Character character) : base(character)
    {
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        character.Movement.SetDestination(Mall.Instance.CustomerSpawnArea.transform.position);
    }

    public override string GetCurrentStateText()
    {
        return "Leaving the mall";
    }
}