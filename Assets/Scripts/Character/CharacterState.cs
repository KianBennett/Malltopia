using UnityEngine;

public abstract class CharacterState
{
    protected Character character;
    protected float timeSpentInState;

    public CharacterState(Character character)
    {
        this.character = character;
    }

    public virtual void OnEnterState()
    {
        timeSpentInState = 0;
    }

    public virtual void OnUpdateState()
    {
        timeSpentInState += Time.deltaTime;
    }

    public virtual void OnExitState()
    {
    }

    public virtual string GetCurrentStateText()
    {
        return string.Empty;
    }
}