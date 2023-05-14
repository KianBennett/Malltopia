using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Janitor : Employee
{
    protected override void Awake()
    {
        base.Awake();

        ChangeState(new CharacterStateClean(this, 1.5f));
    }

    protected override void Update()
    {
        base.Update();

        if (IsInState<CharacterStateBreak>() && Energy >= 100)
        {
            ChangeState(new CharacterStateClean(this));
        }
    }
}