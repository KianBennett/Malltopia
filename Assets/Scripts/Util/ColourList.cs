using UnityEngine;

/*
*   Define a list of colours that can be picked from randomly
*/

[CreateAssetMenu(fileName = "ColourList", menuName = "Data Assets/ColourList", order = 0)]
public class ColourList : ScriptableObject
{
    [SerializeField] private Color[] Colours;

    public Color GetRandomColour()
    {
        if(Colours.Length > 0)
        {
            return Colours[Random.Range(0, Colours.Length)];
        }

        return default;
    }
}