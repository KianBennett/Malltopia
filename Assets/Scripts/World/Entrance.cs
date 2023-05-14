using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entrance : ExteriorObject
{
    [SerializeField] private Animator animator;
    [SerializeField] private float animInterval;

    private float tick;

    void Update()
    {
        tick += Time.deltaTime;

        if(tick > animInterval)
        {
            animator.SetTrigger("PlayAnim");
            tick = 0;
        }
    }
}
