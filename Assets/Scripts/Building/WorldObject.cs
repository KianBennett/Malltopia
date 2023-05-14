using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Linq;
using UnityEngine.UIElements;

/*
*   A base class for each world object (wall pieces, items etc)
*/

public class WorldObject : Selectable
{
    public enum PlacementSound
    {
        Brick, Wood, Metal, Leather, Ceramic
    }

    [SerializeField] private Material tempMaterialValid;
    [SerializeField] private Material tempMaterialInvalid;
    [SerializeField] private Renderer[] tempRenderersToIgnore;
    [SerializeField] private string displayText;
    [SerializeField] private int cost;
    [SerializeField] private Sprite icon;
    [SerializeField] private PlacementSound placementSound;

    protected Dictionary<Renderer, Material[]> renderers; // Value here is renderer's original material
    protected Collider[] colliders;
    protected bool isTempObject;
    protected bool isValidTempObject;

    protected Direction directionFacing;

    public Direction DirectionFacing { get { return directionFacing; } }
    public int Cost { get { return cost; } }
    public Sprite Icon { get { return icon; } }
    public string DisplayText { get { return displayText; } }
    public bool IsTempObject { get { return isTempObject; } }

    protected override void Awake()
    {
        base.Awake();
        renderers = new();
        colliders = GetComponents<Collider>();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (!tempRenderersToIgnore.Contains(renderer))
            {
                renderers.Add(renderer, renderer.sharedMaterials);
            }
        }
    }

    // A function to call on the same frame as initalisation, but after other stuff has been called (stuff that can't be done on awake but needs to be done before start)
    public virtual void Init()
    {
    }

    // Swaps out the materials for each child renderer to the transparent temp material
    public virtual void SetAsTempObject(bool temp, bool valid = true)
    {
        // If there's nothing to change then don't bother
        if(isTempObject == temp && valid == isValidTempObject)
        {
            return;
        }

        isTempObject = temp;
        isValidTempObject = valid;
        Material tempMaterial = valid ? tempMaterialValid : tempMaterialInvalid;

        if(renderers == null)
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                if(!tempRenderersToIgnore.Contains(renderer))
                {
                    renderers.Add(renderer, renderer.sharedMaterials);
                }
            }
        }

        foreach (Renderer renderer in renderers.Keys)
        {
            if(temp)
            {
                Material[] materials = new Material[renderers[renderer].Length];
                Array.Fill(materials, tempMaterial);
                renderer.materials = materials;
            }
            else
            {
                renderer.materials = renderers[renderer];
            }
        }

        // Disable all colliders
        foreach (Collider collider in colliders)
        {
            collider.enabled = !temp;
        }

        // Show an outline with the same colour as the temp material but slightly more opaque
        if(outline)
        {
            if (temp)
            {
                outline.AppendMaterials();
                //outline.OutlineColor = tempMaterial.color;
                outline.SetColour(new Color(tempMaterial.color.r, tempMaterial.color.g, tempMaterial.color.b, 0.8f));
            }
            outline.enabled = temp;
        }
    }

    public void SetHidden(bool hidden)
    {
        foreach(Renderer renderer in renderers.Keys)
        {
            if(renderer) renderer.enabled = !hidden;
        }
    }

    public void SetDirection(Direction direction)
    {
        directionFacing = direction;
    }

    public void RotateClockwise()
    {
        directionFacing = (Direction) MathUtil.Mod((int) directionFacing + 1, 4);
    }

    public void RotateAnticlockwise()
    {
        directionFacing = (Direction) MathUtil.Mod((int) directionFacing - 1, 4);
    }

    public override void SetHighlighted(bool highlighted)
    {
        // Don't highlight temp objects
        if (isTempObject) return;

        base.SetHighlighted(highlighted);

        if (highlighted)
        {
            PlayerController.Instance.AddMousedOverObject(this);
        }
        else
        {
            PlayerController.Instance.RemoveMousedOverObject(this);
        }
    }

    public void PlayPlacementSound()
    {
        // TODO: Add different sounds for different material types
        switch (placementSound)
        {
            case PlacementSound.Brick:
                // Play brick sound
                AkSoundEngine.SetSwitch("itemType", "generic", gameObject);
                break;
            case PlacementSound.Wood:
                // Play wood sound
                AkSoundEngine.SetSwitch("itemType", "wood", gameObject);
                break;
            case PlacementSound.Metal:
                // Play metal sound
                AkSoundEngine.SetSwitch("itemType", "metal", gameObject);
                break;
            case PlacementSound.Leather:
                // Play leather sound
                AkSoundEngine.SetSwitch("itemType", "leather", gameObject);
                break;
            case PlacementSound.Ceramic:
                // Play ceramic sound
                AkSoundEngine.SetSwitch("itemType", "ceramic", gameObject);
                break;
        }

        AkSoundEngine.PostEvent("placeItems", gameObject);
    }

    public void PlayRemovalSound()
    {
        // TODO: Add different sounds for different material types
        switch (placementSound)
        {
            case PlacementSound.Brick:
                // Play brick removal sound
                AkSoundEngine.SetSwitch("itemType", "generic", gameObject);
                break;
            case PlacementSound.Wood:
                // Play wood removal sound
                AkSoundEngine.SetSwitch("itemType", "wood", gameObject);
                break;
            case PlacementSound.Metal:
                // Play metal removal sound
                AkSoundEngine.SetSwitch("itemType", "metal", gameObject);
                break;
            case PlacementSound.Leather:
                // Play leather removal sound
                AkSoundEngine.SetSwitch("itemType", "leather", gameObject);
                break;
            case PlacementSound.Ceramic:
                // Play ceramic removal sound
                AkSoundEngine.SetSwitch("itemType", "ceramic", gameObject);
                break;
        }

        AkSoundEngine.PostEvent("destroyItems", gameObject);
    }

    protected virtual void OnDestroy() 
    {
        PlayerController.Instance?.RemoveMousedOverObject(this);
    }
}