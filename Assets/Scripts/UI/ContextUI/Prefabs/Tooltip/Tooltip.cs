using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public void DestoryTooltip()
    {
        Destroy(transform.parent.gameObject);
    }
}
