using UnityEngine;

/*
*   Define a list of strings that can be picked from randomly
*/

[CreateAssetMenu(fileName = "StringList", menuName = "Data Assets/StringList", order = 0)]
public class StringList : ScriptableObject 
{
    [SerializeField] private string[] Strings;

    public string GetRandomString()
    {
        return Strings[Random.Range(0, Strings.Length)];
    }
}