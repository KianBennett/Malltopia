using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObjectInteractable : ItemObject
{
    [SerializeField] private int interactCost;

    public int InteractCost { get { return interactCost; } }
}
