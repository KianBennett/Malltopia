using System.Collections.Generic;
using UnityEngine;

public class BuildingPieceDoor : BuildingPieceObject
{
    [SerializeField] private Transform doorPivot;

    private List<Character> charactersInRange;

    protected override void Awake()
    {
        base.Awake();

        charactersInRange = new();
    }

    protected override void Update()
    {
        base.Update();

        if(!isTempObject)
        {
            float doorRot = charactersInRange.Count > 0 ? 85 : 0;
            doorPivot.localRotation = Quaternion.Lerp(doorPivot.localRotation, Quaternion.Euler(Vector3.up * doorRot), Time.deltaTime * 5f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Character character = other.GetComponent<Character>();

        if (character)
        {
            charactersInRange.Add(character);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponent<Character>();

        if (character)
        {
            charactersInRange.Remove(character);
        }
    }
}
