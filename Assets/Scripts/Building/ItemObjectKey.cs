using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObjectKey : ItemObject
{
    public enum ObjectTypeKey { Shoe, Clothes, Sweets, Comic, Electronics, Cookery, Arcade }

    [SerializeField] private ObjectTypeKey keyType;
}
