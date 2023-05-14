using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmployeePanel : ListPanel
{
    [SerializeField] private TextMeshProUGUI textEmployeeCount;
    [SerializeField] private RectTransform happinessBarContainer, happinesBarFill;
    [SerializeField] private PurchaseButton purchaseButtonPrefab;

    void Update()
    {
        if(isOpen)
        {
            textEmployeeCount.text = EmployeeManager.Instance.EmployeeCount.ToString();
            float averageHappinessPercentage = EmployeeManager.Instance.GetAverageEmployeeHappiness();
            happinesBarFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, happinessBarContainer.sizeDelta.x * averageHappinessPercentage);
        }
    }

    protected override void populateList()
    {
        int employeeTypeCount = System.Enum.GetValues(typeof(EmployeeManager.ChooseEmployee)).Length;

        for (int i = 0; i < employeeTypeCount; i++)
        {
            Employee employee = EmployeeManager.Instance.EmployeeChecker((EmployeeManager.ChooseEmployee) i);
            PurchaseButton button = Instantiate(purchaseButtonPrefab, transform);
            button.SetValuesWithWage(employee.JobTitle, employee.HireCost, employee.Wage);
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200 - 90 * i);
            int index = i;
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                EmployeeManager.Instance.SpawnEmployeeAtCameraCentre((EmployeeManager.ChooseEmployee) index);
            });
        }
    }
}