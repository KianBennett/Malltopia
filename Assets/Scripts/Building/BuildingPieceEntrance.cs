using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPieceEntrance : BuildingPieceObject
{
    [SerializeField] private Animator signAnimator;

    private float timeSinceLastBob;

    protected override void Update()
    {
        base.Update();

        if (signAnimator)
        {
            timeSinceLastBob += Time.deltaTime;
        }
    }

    public void PlayCustomerEnterAnim()
    {
        if (!signAnimator) return;

        if (timeSinceLastBob > 0.5f)
        {
            
            signAnimator.SetTrigger("Bob");
            timeSinceLastBob = 0;
        }
    }
}