using UnityEngine;

public class CharacterStateIdle : CharacterState
{
    public CharacterStateIdle(Character character) : base(character)
    {
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        character.Movement.StopMoving();
    }

    public override string GetCurrentStateText()
    {
        return "Loitering";
    }
}
