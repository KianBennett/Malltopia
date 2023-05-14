using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCameraView : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Show()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Open");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
