using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private ConfirmMenu confirmMenu;
    [SerializeField] private StandardMenu helpMenu;
    [SerializeField] private CreditsMenu creditsMenu;

    public OptionsMenu OptionsMenu { get { return optionsMenu; } }
    public ConfirmMenu ConfirmMenu { get { return confirmMenu; } }
    public StandardMenu HelpMenu { get { return helpMenu; } }
    public CreditsMenu CreditsMenu { get { return creditsMenu; } }

    public void SetCanvasScale(float scale)
    {
        canvasScaler.scaleFactor = scale;
    }

    public void PlayButtonSound()
    {
        AkSoundEngine.PostEvent("menuSelect", gameObject);
    }
}