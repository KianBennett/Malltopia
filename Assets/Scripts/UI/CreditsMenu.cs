using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CreditsMenu : StandardMenu
{
    public void OpenURL(string url)
    {
        if(url != null && url != "")
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}
