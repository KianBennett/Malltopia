using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ItemObjectTill : ItemObject
{
    private Cell assistantCell;
    private Cell customerCell;

    public Cell AssistantCell { get { return assistantCell; } }
    public Cell CustomerCell { get { return customerCell; } }

    private List<Cell> cellsToNearestEntrance;
    //private Dictionary<Cell, Customer> customersInQueue;
    //private List<Tuple<Cell, Customer>> customersInQueue;

    private List<Tuple<Customer, Cell>> customersInQueue;

    private uint crowdSoundID;

    protected override void Awake()
    {
        base.Awake();
        customersInQueue = new();
    }

    protected override void Update()
    {
        base.Update();

        // Antti sound - Crowd rumbling
        // I thought for this we could start playing the sound of the crowd rumbling when a queues starts forming and stop it when the queue ends
        // A queue has formed when customersInQueue.Count > 0

        // Ive done > 1 here  so that the sound does not start playing with just one person in the queue.
        // Also, my queue sounds incicates that quite a few customers are in the queue.
        // However, feel free to test and alter it if you find it nessecary 
        if (customersInQueue.Count > 1)
        {
            crowdSoundID = AkSoundEngine.PostEvent("crowdRumbling", this.gameObject);
        }
        else
        {
            AkSoundEngine.StopPlayingID(crowdSoundID, 1000, AkCurveInterpolation.AkCurveInterpolation_Linear);
        }
    }

    public override void Init()
    {
        base.Init();

        if (!isTempObject)
        {
            Direction assistantDirection = directionFacing;
            Direction customerDirection = (Direction)MathUtil.Mod((int)directionFacing + 2, 4);

            // Make sure we're not being blocked by a wall
            if (!cell.GetEdge(assistantDirection).buildingPieceObject)
            {
                assistantCell = Mall.CurrentFloor.GetCell(cell.Coords + Mall.GetCellDirectionOffset(assistantDirection));
            }

            if (!cell.GetEdge(customerDirection).buildingPieceObject)
            {
                customerCell = Mall.CurrentFloor.GetCell(cell.Coords + Mall.GetCellDirectionOffset(customerDirection));
            }

            if (customerCell != null)
            {
                // Find the cells to the nearest entrance
                if (cell.room)
                {
                    BuildingPieceObject entrance = cell.room.FindNearestEntrance(cell.GetCentrePosition());
                    if (entrance)
                    {
                        Cell entranceCell = entrance.cellEdge.neighbourCells.Values.Where(o => o.room == cell.room).FirstOrDefault();
                        if (entranceCell != null)
                        {
                            StartCoroutine(Pathfinder.Instance.FindPath(cell.room.cells.ToArray(), customerCell, entranceCell, true, path =>
                            {
                                cellsToNearestEntrance = path;
                            }));
                        }
                    }
                }
            }

            if (AssistantCell != null && CustomerCell != null && cell.room && cell.room.RoomParams is ShopParams)
            {
                ShopParams shopParams = cell.room.RoomParams as ShopParams;

                Vector3 position = AssistantCell.GetCentrePosition();
                Quaternion rotation = Quaternion.LookRotation(CustomerCell.GetCentrePosition() - AssistantCell.GetCentrePosition(), Vector3.up);
                ShopAssistant shopAssistant = Instantiate(shopParams.ShopAssistantPrefab, position, rotation, Mall.CurrentFloor.FixedCharactersContainers);
                shopAssistant.Movement.LookAt(CustomerCell.GetCentrePosition());
                cell.room.RegFixedCharacter(shopAssistant);
            }
        }
    }

    private void updateCustomerQueueCells()
    {
        Cell currentCell = customerCell;
        for (int i = 0; i < customersInQueue.Count; i++)
        {
            // The first cell will always be the customer cell
            if (i > 0)
            {
                // First use the precalculate path to the shop entrance
                if (i < cellsToNearestEntrance.Count)
                {
                    currentCell = cellsToNearestEntrance[i];
                }
                // Then calculate new cells that wrap around the room clockwise
                else
                {
                    currentCell = getNextCellForQueue(currentCell, customersInQueue[i - 2].Item2);
                }
            }

            if (currentCell != null)
            {
                Customer customer = customersInQueue[i].Item1;
                customersInQueue[i] = new(customer, currentCell);
            }
        }

        StartCoroutine(notifyCustomersOfQueueUpdateIEnum());
    }

    private IEnumerator notifyCustomersOfQueueUpdateIEnum()
    {
        for (int i = 0; i < customersInQueue.Count; i++)
        {
            Customer customer = customersInQueue[i].Item1;
            Cell cell = customersInQueue[i].Item2;
            customer.Movement.SetDestination(cell.GetCentrePosition());

            yield return new WaitForSeconds(0.15f);
        }
    }

    // Gets the first empty cell in the direction that the queue is going
    // We need the cell before it to determine which direction to start in
    private Cell getNextCellForQueue(Cell currentCell, Cell prevCell)
    {
        if (currentCell == null) return null;

        Direction directionFromPrevCell = Direction.North;
        if (prevCell != null)
        {
            directionFromPrevCell = Mall.GetDirectionFromOffset(currentCell.Coords - prevCell.Coords);
        }

        for (int i = 0; i < 4; i++)
        {
            // Start in the direction from the previous cell
            Direction direction = (Direction)MathUtil.Mod((int)directionFromPrevCell + i, 4);

            Cell neighbour = currentCell.GetNeighbour(direction);
            if (neighbour == null) continue;

            bool neighbourIsAlreadyInQueue = customersInQueue.Where(o => o.Item2 == neighbour).Count() != 0;
            bool isBlockedByWall = currentCell.GetEdge(direction).buildingPieceObject != null && currentCell.GetEdge(direction).buildingPieceObject.type != BuildingPieceObject.PieceType.Doorway;
            bool isBlockedByObject = neighbour.itemObject;
            bool isGoingBackInShop = neighbour.room == cell.room;

            if (!neighbourIsAlreadyInQueue &&
                !isBlockedByWall &&
                !isBlockedByObject &&
                !isGoingBackInShop)
            {
                return neighbour;
            }
        }

        return null;
    }

    public Cell GetNextEmptyCellForQueue()
    {
        if (customersInQueue.Count < cellsToNearestEntrance.Count)
        {
            return cellsToNearestEntrance[customersInQueue.Count];
        }
        else
        {
            Cell currentCell = customersInQueue.Last().Item2;
            // ^2 here is an index operator (^2 is second-to-last item)
            Cell prevCell = customersInQueue.Count > 1 ? customersInQueue[^2].Item2 : null;
            return getNextCellForQueue(currentCell, prevCell);
        }
    }

    public void AddCustomerToQueue(Customer customer)
    {
        Cell cell = GetNextEmptyCellForQueue();

        if (cell != null)
        {
            customersInQueue.Add(new(customer, cell));
        }

        customer.EnterQueue();
    }

    public void RemoveCustomerFromQueue(Customer customer)
    {
        customersInQueue.RemoveAll(o => o.Item1 == customer);
        updateCustomerQueueCells();

        customer.LeaveQueue();
    }

    public Cell GetPreviousCellInQueueForCustomer(Customer customer)
    {
        int positionInQueue = customersInQueue.FindIndex(o => o.Item1 == customer);

        // If we're the first customer in the queue then we don't have a cell before us
        if (positionInQueue > 0)
        {
            return customersInQueue[positionInQueue - 1].Item2;
        }

        return null;
    }

    public int GetProductCost()
    {
        if (cell.room.RoomParams is ShopParams)
        {
            return (cell.room.RoomParams as ShopParams).ProductCost;
        }
        return 0;
    }
}