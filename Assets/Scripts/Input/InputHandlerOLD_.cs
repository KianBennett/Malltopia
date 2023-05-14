using UnityEngine;
using UnityEngine.EventSystems;

/*
*   Centralised location for all player input
*/

public class InputHandlerOLD_ : Singleton<InputHandlerOLD_> 
{
    // public delegate void RightClickDownDelegate();
    // public delegate void RightClickReleaseDelegate();
    // public delegate void RightClickHoldDelegate();

    // public RightClickDownDelegate OnRightClickDown;
    // public RightClickReleaseDelegate OnRightClickRelease;
    // public RightClickHoldDelegate OnRightClickHold;

    private Vector2 mouseDelta;
    private Vector2 lastMousePos;
    private Vector2 leftClickInit, rightClickInit;
    private Vector2 leftClickMoveDelta, rightClickMoveDelta;
    private bool leftClickHeld, rightClickHeld;
    private const int minDistForDrag = 5;
/*
    void Update() 
    {
        handleKeys();

        // Update distance mouse has moved since last frame
        mouseDelta = (Vector2) Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        if(EventSystem.current)
        {
            if (!EventSystem.current.IsPointerOverGameObject()) 
            {
                if (Input.GetMouseButtonDown(0)) 
                {
                    leftClickHeld = true;
                    leftClickInit = Input.mousePosition;
                    onLeftClickDown();
                }
                if (Input.GetMouseButtonDown(1)) 
                {
                    rightClickHeld = true;
                    rightClickInit = Input.mousePosition;
                    onRightClickDown();
                }
            }
        }
        else
        {
            Debug.LogError("Could not find an event system! Do you need to add one to the scene?");
        }

        if (leftClickHeld) 
        {
            leftClickMoveDelta = (Vector2) Input.mousePosition - leftClickInit;
            onLeftClickHold();
        }

        if (rightClickHeld) 
        {
            rightClickMoveDelta = (Vector2) Input.mousePosition - rightClickInit;
            onRightClickHold();
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            leftClickHeld = false;
            onLeftClickRelease();
        }

        if (Input.GetMouseButtonUp(1)) 
        {
            rightClickHeld = false;
            onRightClickRelease();
        }
    }
*/
    private void handleKeys() 
    {
/*        if (Input.GetKey(KeyCode.LeftArrow)) CameraController.Instance.PanLeft();
        if (Input.GetKey(KeyCode.RightArrow)) CameraController.Instance.PanRight();
        if (Input.GetKey(KeyCode.UpArrow)) CameraController.Instance.PanForward();
        if (Input.GetKey(KeyCode.DownArrow)) CameraController.Instance.PanBackward();
*/
      //  if (Input.GetKey(KeyCode.Comma)) CameraController.Instance.RotateLeft();
      //  if (Input.GetKey(KeyCode.Period)) CameraController.Instance.RotateRight();

      //  if (Input.GetKeyDown(KeyCode.LeftBracket)) PlayerController.Instance.DecreaseTimeSpeed();
      //  if (Input.GetKeyDown(KeyCode.RightBracket)) PlayerController.Instance.IncreaseTimeSpeed();

       // if (Input.GetKeyDown(KeyCode.B)) PlayerController.Instance.EnterBuildMode();
       // if (Input.GetKeyDown(KeyCode.O)) PlayerController.Instance.EnterSingleObjectBuildMode();
       // if (Input.GetKeyDown(KeyCode.Escape)) PlayerController.Instance.ExitBuildMode();

       // if (Input.GetKeyDown(KeyCode.C)) CustomerManager.Instance.SpawnCustomerAtMousePoint();
       // if (Input.GetKeyDown(KeyCode.J)) EmployeeManager.Instance.SpawnEmployeeAtMousePoint(EmployeeManager.ChooseEmployee.Janitor);
       // if (Input.GetKeyDown(KeyCode.G)) EmployeeManager.Instance.SpawnEmployeeAtMousePoint(EmployeeManager.ChooseEmployee.SecurityGuard);

       // if (Input.GetKeyDown(KeyCode.H)) MallGridVisualiser.Instance.ToggleCellCustomerCountVisMode();
    }   

    // TODO: Make these callbacks delegates
/*    private void onLeftClickDown() 
    {
    }

    private void onLeftClickHold() 
    {
    }

    private void onLeftClickRelease() 
    {
    }

    private void onRightClickDown() 
    {
        CameraController.Instance.Grab();
    }

    private void onRightClickHold() 
    {
        if (rightClickMoveDelta.magnitude >= minDistForDrag && mouseDelta.magnitude > 0)
        {
            CameraController.Instance.Pan();
        }
    }

    private void onRightClickRelease() 
    {
        CameraController.Instance.Release();
    }*/
}