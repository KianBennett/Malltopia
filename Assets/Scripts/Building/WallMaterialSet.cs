using UnityEngine;

[CreateAssetMenu(fileName = "WallMaterialSet", menuName = "Data Assets/Wall Material Set", order = 0)] 
public class WallMaterialSet : ScriptableObject
{
    public Material WallMaterial;
    public Material TopMaterial;
    public Material TrimMaterial;
}
