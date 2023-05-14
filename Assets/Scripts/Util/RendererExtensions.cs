using UnityEngine;

public static class RendererExtensions
{
    public static void SetMaterial(this Renderer renderer, Material material, int index)
    {
        Material[] materials = renderer.materials;
        materials[index] = material;
        renderer.materials = materials;
    }
}