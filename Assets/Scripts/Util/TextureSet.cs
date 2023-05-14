using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TextureSet", menuName = "Data Assets/Texture Set", order = 0)]
public class TextureSet : ScriptableObject
{
    public Texture2D mainTexture;
    public Texture2D normals;
    public Texture2D roughness;
    public Texture2D specular;
}
