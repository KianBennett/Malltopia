using UnityEngine;

public class ItemObjectTrash : ItemObject
{
    [SerializeField] private GameObject[] trashModels;
    [SerializeField] private Transform modelContainer;

    private Character characterToClean;

    public Vector3 TrashPosition { get { return modelContainer.transform.position; } }

    protected override void Awake()
    {
        base.Awake();

        int model = Random.Range(0, trashModels.Length);
        for(int i = 0; i < trashModels.Length; i++)
        {
            trashModels[i].SetActive(i == model);
        }

        modelContainer.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        modelContainer.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        modelContainer.localScale = Vector3.one * Random.Range(0.9f, 1.1f);
    }

    public void SetCharacterToClean(Character character)
    {
        characterToClean = character;
    }

    public bool CanBeCleaned()
    {
        // Check a janitor isn't already moving to clean it
        return characterToClean == null;
    }
}