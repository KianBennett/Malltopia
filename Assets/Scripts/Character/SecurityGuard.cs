using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SecurityGuard : Employee
{
    protected override void Awake()
    {
        base.Awake();

        ChangeState(new CharacterStatePatrol(this, 1.5f));
    }

    protected override void Update()
    {
        base.Update();

        if (IsInState<CharacterStateBreak>() && Energy >= 100)
        {
            ChangeState(new CharacterStatePatrol(this, 1.5f));
        }
    }
}
