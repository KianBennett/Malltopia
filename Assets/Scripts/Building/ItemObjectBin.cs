using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class ItemObjectBin : ItemObjectInteractable
{
    [SerializeField] private Material materialStandard;
    [SerializeField] private Material materialDirty;
    [SerializeField] private Renderer binRenderer;

    private bool isDirty;
    private int trashPieces;

    public bool IsDirty => isDirty;

    public void AddTrash()
    {
        trashPieces++;
        if(trashPieces == 10)
        {
            setDirty(true);
        }
    }

    public void Clean()
    {
        setDirty(false);
        trashPieces = 0;
    }

    private void setDirty(bool dirty)
    {
        isDirty = dirty;

        if (binRenderer)
        {
            binRenderer.material = isDirty ? materialDirty : materialStandard;
        }
    }
}
