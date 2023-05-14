using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmployeeContextMenu : ContextMenuRoot
{

    public Employee Target;

    public Slider HappynessSlider;
    public Slider EnergySlider;
    
    [SerializeField] TMP_Text Name;


    private void Start()
    {
        Target = (Employee)PlayerController.Instance.SelectedCharacter;
        Name.text = Target.CharacterName;
       
        Vector3 v = transform.position;
        transform.position = v;

        HappynessSlider.value = Target.Happiness;
        EnergySlider.value = Target.Energy;
    }


    void Update()
    {
        if(Target)
        {
            HappynessSlider.value = Target.Happiness;
            EnergySlider.value = Target.Energy;

            GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            GetComponent<VerticalLayoutGroup>().SetLayoutHorizontal();
            GetComponent<VerticalLayoutGroup>().SetLayoutVertical();

        }
    }


    public void LocateChar()
    {
        CameraController.Instance.SetPositionImmediate(Target.transform.position);
    }
}
