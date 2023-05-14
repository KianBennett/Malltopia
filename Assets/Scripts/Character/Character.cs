using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Character : Selectable
{
    //[SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private CharacterMovement movement;
    [SerializeField] private Animator maleAnimator;
    [SerializeField] private Animator femaleAnimator;
    [SerializeField] private GameObject highlightRing;
    [SerializeField] private Camera cameraEyes;
    [SerializeField] private Transform eyesMaleTransform;
    [SerializeField] private Transform eyesFemaleTransform;

    [Header("Rendering")]
    [SerializeField] private GameObject maleBody;
    [SerializeField] private GameObject femaleBody;
    [SerializeField] private bool canBeFemale;
    [SerializeField] private Renderer[] skinRenderers;
    [SerializeField] private Renderer[] shirtRenderers;
    [SerializeField] private Renderer[] maleHairRenderers;
    [SerializeField] private Renderer[] femaleHairRenderers;
    [SerializeField] private Renderer[] trouserRenderers;
    [SerializeField] private Renderer[] shoeRenderers;

    [Header("Customisation")]
    [SerializeField] private StringList namesList;
    [SerializeField] private ColourList skinColours;
    [SerializeField] private ColourList hairColours;
    [SerializeField] private ColourList shirtColours;
    [SerializeField] private ColourList trouserColours;
    [SerializeField] private ColourList shoeColours;
    [SerializeField] private Material skinMaterial;
    [SerializeField] private Material hairMaterial;
    [SerializeField] private Material clothingMaterial;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.2f;

    private bool isMale;
    private int hairType;
    private CharacterState state;
    private string characterName;
    private Cell currentCell;
    private bool isSitting;

    //public NavMeshAgent Agent { get { return navMeshAgent; } }
    public CharacterMovement Movement { get { return movement; } }
    public Animator ModelAnimator { get { return isMale ? maleAnimator : femaleAnimator; } }
    public CharacterState CurrentState { get { return state; } }
    public Cell CurrentCell { get { return currentCell; } }
    public string CharacterName { get { return characterName; } }
    public bool IsSitting { get { return isSitting; } }
    public bool IsLeaving { get { return IsInState<CharacterStateLeaving>(); } }

    // Instead of creating new instance of a material for each renderer, store them in a cached static dictionary
    private static Dictionary<Color, Material> cachedSkinMaterials;
    private static Dictionary<Color, Material> cachedHairMaterials;
    private static Dictionary<Color, Material> cachedClothingMaterials;

    protected override void Awake()
    {
        base.Awake();

        cachedSkinMaterials ??= new();
        cachedHairMaterials ??= new();
        cachedClothingMaterials ??= new();

        isMale = canBeFemale ? Random.value > 0.5f : true;
        if (maleBody) maleBody.SetActive(isMale);
        if (femaleBody) femaleBody.SetActive(!isMale);

        int hairTypeCount = (isMale ? maleHairRenderers : femaleHairRenderers).Length;
        hairType = Random.Range(0, hairTypeCount);
        for(int i = 0; i < hairTypeCount; i++)
        {
            Renderer renderer = (isMale ? maleHairRenderers : femaleHairRenderers)[i];
            renderer.gameObject.SetActive(i == hairType);

            if(i == hairType)
            {
                renderer.material = getCachedMaterial(hairMaterial, hairColours.GetRandomColour(), cachedHairMaterials);
            }
        }

        Color skinColour = skinColours.GetRandomColour();
        foreach (Renderer renderer in skinRenderers)
        {
            renderer.material = getCachedMaterial(skinMaterial, skinColour, cachedSkinMaterials);
        }
        foreach (Renderer renderer in shirtRenderers)
        {
            renderer.material = getCachedMaterial(clothingMaterial, shirtColours.GetRandomColour(), cachedClothingMaterials);
        }
        foreach (Renderer renderer in trouserRenderers)
        {
            renderer.material = getCachedMaterial(clothingMaterial, trouserColours.GetRandomColour(), cachedClothingMaterials);
        }
        foreach (Renderer renderer in shoeRenderers)
        {
            renderer.material = getCachedMaterial(clothingMaterial, shoeColours.GetRandomColour(), cachedClothingMaterials);
        }

        characterName = namesList.GetRandomString();

        // Pick a random size and adjust the model scale and agent radius accordingly
        float size = Random.Range(minScale, maxScale);
        transform.localScale = Vector3.one * size;

        ModelAnimator.SetTrigger("Appear");

        Cell cell = Mall.CurrentFloor.GetCellFromWorldPos(transform.position);

        if(cell != null)
        {
            cell.floor.RegisterCharacter(this);
        }
        else
        {
            Mall.GroundFloor.RegisterCharacter(this);
        }
    }

    protected override void Start()
    {
        base.Start();

        CameraController.Instance.RegisterAdditionalCamera(cameraEyes);
    }

    protected override void Update()
    {
        base.Update();

        state?.OnUpdateState();

        // Listen for when the character moves to a new cell
        Cell newCurrentCell = Mall.CurrentFloor.GetCellFromWorldPos(transform.position);
        if (newCurrentCell != currentCell && newCurrentCell != null)
        {
            OnEnterNewCell(currentCell, newCurrentCell);
        }
        currentCell = newCurrentCell;

        if(cameraEyes)
        {
            cameraEyes.transform.position = (isMale ? eyesMaleTransform : eyesFemaleTransform).position;

            if (Input.GetKeyDown(KeyCode.M))
            {
                CameraController.Instance.SwitchToCamera(cameraEyes);
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                CameraController.Instance.SwitchToDefaultCamera();
            }
        }
    }

    public void ChangeState(CharacterState newState)
    {
        state?.OnExitState();

        //if (state != null) Debug.Log("left state: " + state);

        state = newState;
        state.OnEnterState();

        //Debug.Log("started new state: " + newState);
    }

    public bool IsInState<T>() where T : CharacterState
    {
        return state != null && state is T;
    }

    public float DistToPoint(Vector3 point)
    {
        return Vector3.Distance(transform.position, point);
    }

    public void SetSitting(bool sitting)
    {
        isSitting = sitting;
    }

    public override void SetHighlighted(bool highlighted)
    {
        base.SetHighlighted(highlighted);

        if (highlighted)
        {
            PlayerController.Instance.AddMousedOverCharacter(this);

            foreach(Renderer renderer in skinRenderers) renderer.material.EnableKeyword("_EMISSION");
            foreach (Renderer renderer in shirtRenderers) renderer.material.EnableKeyword("_EMISSION");
            foreach (Renderer renderer in trouserRenderers) renderer.material.EnableKeyword("_EMISSION");
            foreach (Renderer renderer in shoeRenderers) renderer.material.EnableKeyword("_EMISSION");
            getHairRenderer().material.EnableKeyword("_EMISSION");
        }
        else
        {
            PlayerController.Instance.RemoveMousedOverCharacter(this);

            foreach (Renderer renderer in skinRenderers) renderer.material.DisableKeyword("_EMISSION");
            foreach (Renderer renderer in shirtRenderers) renderer.material.DisableKeyword("_EMISSION");
            foreach (Renderer renderer in trouserRenderers) renderer.material.DisableKeyword("_EMISSION");
            foreach (Renderer renderer in shoeRenderers) renderer.material.DisableKeyword("_EMISSION");
            getHairRenderer().material.DisableKeyword("_EMISSION");
        }
    }

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);

        if (highlightRing)
        {
            highlightRing.SetActive(selected);
        }
    }

    protected virtual void OnEnterNewCell(Cell cellLeaving, Cell cellEntering)
    {
    }

    void OnTriggerStay(Collider other)
    {
        if(other == Mall.Instance.CustomerSpawnArea && IsLeaving)
        {
            onLeaveMall();
            Destroy(gameObject);
        }    
    }

    protected virtual void onLeaveMall()
    {
    }

    private Material getCachedMaterial(Material materialBase, Color color, Dictionary<Color, Material> cache)
    {
        if (cache.ContainsKey(color))
        {
            return cache[color];
        }

        Material material = new(materialBase)
        {
            color = color
        };

        cache.Add(color, material);
        return material;
    }

    private Renderer getHairRenderer()
    {
        return (isMale ? maleHairRenderers : femaleHairRenderers)[hairType];
    }

    void OnDestroy()
    {
        PlayerController.Instance?.RemoveMousedOverCharacter(this);
        currentCell?.floor.UnregisterCharacter(this);
    }
}