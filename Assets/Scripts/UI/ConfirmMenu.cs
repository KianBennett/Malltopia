using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class ConfirmMenu : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private GameObject backgroundFade;

    private UnityAction onConfirm, onDeny;

    public void Show(string text, bool showBackgroundFade, UnityAction onConfirm, UnityAction onDeny)
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Open");
        confirmText.text = text;
        backgroundFade.SetActive(showBackgroundFade);
        this.onConfirm = onConfirm;
        this.onDeny = onDeny;
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Confirm()
    {
        onConfirm?.Invoke();
        Hide();
    }

    public void Deny()
    {
        onDeny?.Invoke();
        Hide();
    }
}
