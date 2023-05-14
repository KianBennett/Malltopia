using UnityEngine;
using UnityEngine.InputSystem;
using System.Reflection;
using System.Diagnostics;

public class InputHandler : Singleton<InputHandler>
{
    PlayerInput InputSystem;
    [Tooltip ("While true, this will print the Action, Class and Method to the console for debug purposes")]
    [SerializeField]
    private bool PrintFunctionCallers;

    private void Start()
    {
        InputSystem = GetComponent<PlayerInput>();
    }

    public bool IsPressed(string ActionName)
    {
        if (!NameErrorCatch(ActionName)) return false;
        if (InputSystem.actions.FindAction(ActionName).IsPressed()) Debug_FunctionCaller(ActionName, new StackFrame(1).GetMethod());
        return InputSystem.actions.FindAction(ActionName).IsPressed();
    }
    public bool WasPressedThisFrame(string ActionName)
    {
        if (!NameErrorCatch(ActionName)) return false;
        if (InputSystem.actions.FindAction(ActionName).WasPressedThisFrame()) Debug_FunctionCaller(ActionName, new StackFrame(1).GetMethod());
        return InputSystem.actions.FindAction(ActionName).WasPressedThisFrame();
    }
    public bool WasReleasedThisFrame(string ActionName)
    {
        if (!NameErrorCatch(ActionName)) return false;
        if (InputSystem.actions.FindAction(ActionName).WasReleasedThisFrame()) Debug_FunctionCaller(ActionName, new StackFrame(1).GetMethod());
        return InputSystem.actions.FindAction(ActionName).WasReleasedThisFrame();
    }
    public InputAction FindAction(string ActionName)
    {
        if (!NameErrorCatch(ActionName)) return null;
        return InputSystem.actions.FindAction(ActionName);
    }
    public bool NameErrorCatch(string ActionName)
    {
        if (InputSystem.actions.FindAction(ActionName) != null) return true;
        else
        {
            UnityEngine.Debug.LogError("'" + ActionName + "'" + " is not a valid action name");
            return false;
        }
    }
    void Debug_FunctionCaller(string ActionName, MethodBase CallerFunction)
    {
        if (PrintFunctionCallers)
        { 
            string Text =
                "Action: '" + "<color=green>" + ActionName + "</color>" + "' " + new StackFrame(1).GetMethod().Name
                + "      Class: '" + "<color=green>" + CallerFunction.DeclaringType.Name.ToString() + "</color>"
                + "'  Method: '" + "<color=green>" + CallerFunction.Name.ToString() + "</color>" + "'";

            UnityEngine.Debug.Log(Text);
        }
    }

}
