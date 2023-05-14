using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerManager : Singleton<CustomerManager>
{
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform customerObjectParent;
    
    [SerializeField] private bool spawnCustomersAutomatically;
    [SerializeField] private float spawnCustomersInterval;
    [SerializeField] private int maxPossibleCustomers;

    private List<Customer> customers;

    public int CustomerCount { get { return customers != null ? customers.Count : 0; } }

    protected override void Awake()
    {
        base.Awake();

        customers = new();
    }

    void Start()
    {
        StartCoroutine(spawnCustomersContinuouslyIEnum());
    }

    public Customer SpawnCustomer(Vector3 point)
    {
        Customer customer = Instantiate(customerPrefab, point, Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)), customerObjectParent);
        customers.Add(customer);

        // If we're over the customer limit (this shouldn't happen) then remove the older customer
        if(customers.Count > maxPossibleCustomers)
        {
            Destroy(customers.First().gameObject);
        }

        return customer;
    }

    public void RemoveCustomer(Customer customer)
    {
        customers?.Remove(customer);
    }

    public Customer SpawnCustomerAtEntrance()
    {
        return SpawnCustomer(Mall.Instance.GetRandomCustomerSpawnPoint());
    }

    public Customer SpawnCustomerAtMousePoint()
    {
        if (CameraController.Instance.GetMousePointOnGround(out Vector3 mousePoint))
        {
            Vector2Int coords = Mall.Instance.CellCoordsFromWorldPos(mousePoint);
            if (Mall.CurrentFloor.GetCell(coords) != null)
            {
                return SpawnCustomer(mousePoint);       
            }
            else
            {
                Debug.LogWarning("Tried to spawn customer outside the mall!");
            }
        }

        return null;
    }

    public float GetAverageCustomerHappiness()
    {
        if(customers.Count == 0)
        {
            return 0;
        }

        float totalHappiness = 0;

        foreach(Customer customer in customers)
        {
            totalHappiness += customer.Happiness / customer.Params.MaxHappiness;
        }

        return totalHappiness / customers.Count;
    }

    private IEnumerator spawnCustomersContinuouslyIEnum()
    {
        while(true)
        {
            yield return new WaitUntil(() => 
            { 
                return spawnCustomersAutomatically && Mall.CurrentFloor.FindShops().Length > 0 && customers.Count < maxPossibleCustomers;
            });

            SpawnCustomerAtEntrance();

            // The more shops you have, the faster customers will spawn
            yield return new WaitForSeconds(spawnCustomersInterval - (Mall.CurrentFloor.FindShops().Length * 1.5f));
        }
    }
}