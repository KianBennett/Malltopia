using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfObject : MonoBehaviour
{
    [SerializeField] private ColourList colours;
    [SerializeField] private Material material;
    [SerializeField] private Renderer[] renderers;

    private static Dictionary<Color, Material> colourMaterialDictionary;

    void Awake()
    {
        if(colours && renderers.Length > 0)
        {
            colourMaterialDictionary ??= new();

            Color colour = colours.GetRandomColour();
            if(colourMaterialDictionary.ContainsKey(colour))
            {
                foreach(Renderer renderer in renderers)
                {
                    renderer.material = colourMaterialDictionary[colour];
                }
            }
            else
            {
                Material mat = new(material);
                mat.color = colour;
                colourMaterialDictionary.Add(colour, mat);
                foreach (Renderer renderer in renderers)
                {
                    renderer.material = mat;
                }
            }
        }

        transform.localRotation = Quaternion.Euler(Vector3.forward * Random.Range(-15.0f, 15.0f));
        transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
    }
}
