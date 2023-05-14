using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HelpMenu : StandardMenu
{
    [SerializeField, TextArea(4, 20)] private string[] sectionContents;
    [SerializeField] private OptionsTabButton[] sectionButtons;
    [SerializeField] private TextMeshProUGUI contentsText;
    [SerializeField] private Button buttonSectionNext;
    [SerializeField] private Button buttonSectionPrev;

    private int currentSection;

    public override void Show(UnityAction onClose = null)
    {
        base.Show(onClose);

        SetCurrentSection(0);
    }

    public void SetCurrentSection(int section)
    {
        currentSection = section;
        buttonSectionNext.interactable = section < sectionContents.Length - 1;
        buttonSectionPrev.interactable = section > 0;

        contentsText.text = sectionContents[section];
        for(int i = 0; i < sectionButtons.Length; i++)
        {
            sectionButtons[i].SetActive(i == section);
        }
    }

    public void NextSection()
    {
        if(currentSection < sectionContents.Length - 1)
        {
            SetCurrentSection(currentSection + 1);
        }
    }

    public void PreviousSection()
    {
        if (currentSection > 0)
        {
            SetCurrentSection(currentSection - 1);
        }
    }
}
