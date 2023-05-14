#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.InputSystem;

public class DebugInputs : MonoBehaviour
{

    private void Update()
    {
        //Debug Functions
        if (InputHandler.Instance.WasPressedThisFrame("Test - Spawn Customer")) CustomerManager.Instance.SpawnCustomerAtMousePoint();
        if (InputHandler.Instance.WasPressedThisFrame("Test - Spawn Janitor")) EmployeeManager.Instance.SpawnEmployeeAtMousePoint(EmployeeManager.ChooseEmployee.Janitor);
        if (InputHandler.Instance.WasPressedThisFrame("Test - Spawn SecurityGuard")) EmployeeManager.Instance.SpawnEmployeeAtMousePoint(EmployeeManager.ChooseEmployee.SecurityGuard);
        if (InputHandler.Instance.WasPressedThisFrame("Test - ToggleCellCustomerCountVisMode")) MallGridVisualiser.Instance.ToggleCellCustomerCountVisMode();

        //Functions still requiring a home script

    }

}
#endif