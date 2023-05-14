using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExteriorObject : MonoBehaviour
{
    void Awake()
    {
        foreach(Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            foreach(Material mat in renderer.materials)
            {
                TimeManager.Instance.RegisterMaterial(mat);
            }
        }
    }
}
