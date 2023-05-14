using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Selectable : MonoBehaviour
{
    [Header("Outline")]
    [SerializeField] private bool showOutlineOnHighlighted = true;
    [SerializeField] protected Outline outline;
    [SerializeField] private SelectableParams selectableParams;

    private bool isHighlighted;
    private bool isSelected;
    private bool forceOutline; // Used if you want to show the outline without highlighting or selecting an object
    private bool forceSelectedOutline;

    public bool IsHighlighted { get { return isHighlighted; } }
    public bool IsSelected { get { return isSelected; } }

    protected virtual void Awake()
    {
        if (outline)
        {
            outline.enabled = false;
        }
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        if (outline)
        {
            bool showOutline = (showOutlineOnHighlighted && (isHighlighted || isSelected)) || forceOutline || forceSelectedOutline;
            if (showOutline != outline.enabled)
            {
                outline.enabled = showOutline;
            }
            if (outline.enabled && selectableParams)
            {
                outline.OutlineColor = (isSelected || forceSelectedOutline) ? 
                    selectableParams.outlineColourSelected : selectableParams.outlineColourHighlighted;
            }
        }
    }

    public virtual void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
    }

    public virtual void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    void OnMouseEnter()
    {
        if (PlayerController.Instance) SetHighlighted(true);
    }

    void OnMouseExit()
    {
        if (PlayerController.Instance) SetHighlighted(false);
    }

    public void SetForcedOutline(bool enabled)
    {
        forceOutline = enabled;
    }

    public void SetForcedSelectedOutline(bool enabled)
    {
        forceSelectedOutline = enabled;
    }
}