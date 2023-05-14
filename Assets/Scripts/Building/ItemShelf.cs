using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShelf : MonoBehaviour
{
    [SerializeField] ItemObject itemObject;
    [SerializeField] private ShelfObject[] stockPrefabs;
    [SerializeField] private Transform[] stockPositions;

    private List<ShelfObject> objects;

    void Start()
    {
        if(itemObject && !itemObject.IsTempObject)
        {
            objects = new();

            float fillPercentage = 0.75f;

            foreach (Transform stockPosition in stockPositions)
            {
                bool hasStock = Random.value <= fillPercentage;

                if (hasStock)
                {
                    ShelfObject prefab = stockPrefabs[Random.Range(0, stockPrefabs.Length)];
                    ShelfObject shelfObject = Instantiate(prefab, stockPosition);
                    shelfObject.transform.localPosition = Vector3.zero;
                    shelfObject.transform.localRotation = Quaternion.identity;
                    objects.Add(shelfObject);
                }
            }
        }
    }
}
