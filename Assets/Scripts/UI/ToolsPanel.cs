using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsPanel : MonoBehaviour
{
    [SerializeField] private Animator expandButton;
    [SerializeField] private Animator buttonPanel;

    void Awake()
    {
        // Immediately skip to the end of the animation
        buttonPanel.CrossFade("Close", 0.0f, 0, 1.0f);
    }

    public void Open()
    {
        expandButton.SetTrigger("Close");
        buttonPanel.SetTrigger("Open");
    }

    public void Close(bool showExpandButton)
    {
        if(showExpandButton)
        {
            expandButton.SetTrigger("Open");
        }
        buttonPanel.SetTrigger("Close");
    }

    public void OpenBuildPanel()
    {
        HUD.Instance.BuildingPanel.Open();
    }

    public void OpenObjectsPanel()
    {
        HUD.Instance.ObjectsPanel.Open();
    }

    public void OpenEmployeesPanel()
    {
        HUD.Instance.EmployeePanel.Open();
    }
}
