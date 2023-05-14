using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EmployeeManager;

public class EmployeeManager : Singleton<EmployeeManager>
{
    [SerializeField] private Employee janitorPrefab;
    [SerializeField] private Employee securityGuardPrefab;
    [SerializeField] private Transform employeeObjectParent;

    private List<Employee> employees;

    public int EmployeeCount { get { return employees.Count; } }

    public enum ChooseEmployee
    {
        Janitor,
        SecurityGuard
    }

    protected override void Awake()
    {
        base.Awake();
        employees = new();
    }

    public void SpawnEmployeeAtMousePoint(ChooseEmployee chosenEmployee)
    {
        if(CameraController.Instance.GetMousePointOnGround(out Vector3 mousePoint))
        {
            spawnEmployeeAtWorldPos(chosenEmployee, mousePoint);
        }
    }

    public void SpawnEmployeeAtCameraCentre(ChooseEmployee chosenEmployee)
    {
        spawnEmployeeAtWorldPos(chosenEmployee, CameraController.Instance.transform.position);
    }

    private void spawnEmployeeAtWorldPos(ChooseEmployee chosenEmployee, Vector3 worldPos)
    {
        Vector2Int coords = Mall.Instance.CellCoordsFromWorldPos(worldPos);
        if (Mall.CurrentFloor.GetCell(coords) != null)
        {
            if(PlayerController.Instance.RemoveMoney(EmployeeChecker(chosenEmployee).HireCost))
            {
                Employee employee = Instantiate(EmployeeChecker(chosenEmployee), worldPos, Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)), employeeObjectParent);
                employees.Add(employee);
            }
        }
        else
        {
            Debug.LogWarning("Tried to spawn customer outside the mall!");
        }
    }
    
    public void UnregisterEmployee(Employee employee)
    {
        if(employees.Contains(employee))
        {
            employees.Remove(employee);
        }
    }

    public Employee EmployeeChecker(ChooseEmployee chosenEmployee)
    {
        switch (chosenEmployee)
        {
            case ChooseEmployee.Janitor:
                return janitorPrefab;
            case ChooseEmployee.SecurityGuard:
                return securityGuardPrefab;
            default:
                Debug.Log("No employee chosen");
                return null;
        }
    }

    public float GetAverageEmployeeHappiness()
    {
        if (employees.Count == 0)
        {
            return 0;
        }

        float totalHappiness = 0;

        foreach (Employee employee in employees)
        {
            totalHappiness += employee.Happiness / 100.0f;
        }

        return totalHappiness / employees.Count;
    }

    public T[] GetAllEmployeesOfType<T>() where T : Employee
    {
        return employees.Where(o => o is T).Select(o => o as T).ToArray();
    }
}
