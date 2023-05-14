using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SelectableParams", menuName = "Data Assets/SelectableParams", order = 0)]
public class SelectableParams : ScriptableObject
{
    public Color outlineColourHighlighted = Color.white;
    public Color outlineColourSelected = Color.white;
}
