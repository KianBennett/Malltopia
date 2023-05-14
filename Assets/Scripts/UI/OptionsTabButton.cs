using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsTabButton : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Graphic icon;
    [SerializeField] private Color buttonColourActive, buttonColourInactive;

    private bool active;
    private float iconScale;

    void Awake()
    {
        iconScale = 1.0f;
    }

    void Update()
    {
        if (icon)
        {
            float targetScale = active ? 1.0f : 0.8f;
            iconScale = Mathf.MoveTowards(iconScale, targetScale, Time.unscaledDeltaTime * 4.0f);
            icon.transform.localScale = Vector3.one * iconScale;
        }
    }

    public void SetActive(bool active)
    {
        this.active = active;
        buttonImage.color = active ? buttonColourActive : buttonColourInactive;
        icon.color = new Color(1, 1, 1, active ? 1 : 0.6f);
        if(active)
        {
            animator.SetTrigger("Pulse");
        }
    }
}
