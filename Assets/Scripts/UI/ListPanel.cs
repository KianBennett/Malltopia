using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ListPanel : MonoBehaviour
{
    [SerializeField] private Animator animator;

    protected bool isOpen;

    public bool isOpenCheck { get { return isOpen; } }

    protected virtual void Awake()
    {
        // Immediately skip to the end of the animation depending on initial state
        animator.CrossFade(isOpen ? "Open" : "Close", 0.0f, 0, 1.0f);

        populateList();
    }

    public void Open()
    {
        if(!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        if (!isOpen)
        {
            animator.SetTrigger("Open");
        }
        HUD.Instance.ToolsPanel.Close(false);
        isOpen = true;
    }

    public void Close(bool invokeOnClose)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        if (isOpen)
        {
            animator.SetTrigger("Close");
            HUD.Instance.ToolsPanel.Open();
        }
        isOpen = false;

        if(invokeOnClose)
        {
            onClose();
        }
    }

    protected virtual void onClose()
    {
    }

    protected virtual void populateList()
    {
    }
}
