using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrashManager : Singleton<TrashManager>
{
    [SerializeField] private bool spawnTrash;
    [SerializeField] private ItemObjectTrash trashObjectPrefab;

    private List<ItemObjectTrash> trashObjects;

    protected override void Awake()
    {
        base.Awake();
        trashObjects = new();
        StartCoroutine(spawnIEnum());
    }

    public void RemoveTrashObject(ItemObjectTrash trashObject)
    {
        if(trashObject)
        {
            trashObject.cell.trashObject = null;
            trashObjects.Remove(trashObject);
            AkSoundEngine.PostEvent("janitor", trashObject.gameObject);
            Destroy(trashObject.gameObject);
        }
    }

    public void RemoveTrashObjectFromCell(Cell cell)
    {
        RemoveTrashObject(cell.trashObject);
    }

    // Returns success
    private bool spawnTrashObject()
    {
        // Cell[] cells = Mall.Instance.GetEmptyCellsNotInRoom();
        // Cell cell = cells[Random.Range(0, cells.Length)];

        // This is probs very inefficient
        // Loop through each cell - if it's outside a room and empty then add it to the array x times
        //      Where x is the number of times a customer has entered that cell
        //      This means that the more a customer enters that cell, the higher chance it has of spawning trash

        List<Cell> cellsWithCustomerCount = new();

        foreach(Cell cell in Mall.CurrentFloor.Cells)
        {
            if (cell.room == null && cell.itemObject == null && cell.trashObject == null)
            {
                for (int c = 0; c < cell.customerCount; c++)
                {
                    cellsWithCustomerCount.Add(cell);
                }
            }
        }

        if(cellsWithCustomerCount.Count > 0)
        {
            Cell spawnCell = cellsWithCustomerCount[Random.Range(0, cellsWithCustomerCount.Count)];

            // If the cell is in range of a bin then have a random change of putting it in there instead of the floor
            ItemObjectBin nearestBin = Mall.CurrentFloor.FindNearestObjectOfType<ItemObjectBin>(spawnCell.GetCentrePosition());
            if(nearestBin != null)
            {
                float distToBin = Vector3.Distance(nearestBin.transform.position, spawnCell.GetCentrePosition());
                float binRange = 6;

                if (distToBin < binRange)
                {
                    // The chance of the trash going in the bin is 100% when dist == 0 and 50% when dist == binRange
                    float percentageChanceOfTrashGoingInBin = 1.0f - 0.5f * (distToBin / binRange);
                    bool getInTheBin = Random.value < percentageChanceOfTrashGoingInBin;

                    if (getInTheBin)
                    {
                        nearestBin.AddTrash();
                        return false;
                    }
                }
            }

            ItemObjectTrash trashObject = Mall.CurrentFloor.AddItem(spawnCell, (Direction) Random.Range(0, 4), trashObjectPrefab) as ItemObjectTrash;
            spawnCell.trashObject = trashObject;
            trashObjects.Add(trashObject);

            return true;
        }

        return false;
    }

    private IEnumerator spawnIEnum()
    {
        while(true)
        {
            if(spawnTrash && CustomerManager.Instance.CustomerCount > 0)
            {
                yield return new WaitForSeconds(Mathf.Max(20.0f - (0.025f * CustomerManager.Instance.CustomerCount), 5.0f));
                while(!spawnTrashObject())
                {
                    // If we can't spawn trash (i.e. there are customers in the mall but no tiles have been walked on yet
                    // Then keep trying every second (not every frame to avoid perf issues)
                    yield return new WaitForSeconds(1.0f);
                }
            }
            yield return null;
        }
    }

    public ItemObjectTrash GetClosestTrashObject(Vector3 position)
    {
        return trashObjects
            .Where(o => o.CanBeCleaned())
            .OrderBy(o => Vector3.Distance(position, o.cell.GetCentrePosition()))
            .FirstOrDefault();
    }
}
