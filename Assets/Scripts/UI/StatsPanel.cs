using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMoney;
    [SerializeField] private Animator animatorMoney;
    [SerializeField] private RectTransform crimeBarFill;
    [SerializeField] private RectTransform happinessBarFill;

    void Start()
    {
        PlayerController.Instance.OnMoneyChanged += delegate
        {
            textMoney.text = PlayerController.Instance.Money.ToString();
            animatorMoney.SetTrigger("Pulse");
        };

        textMoney.text = PlayerController.Instance.Money.ToString();
    }

    void Update()
    {
        crimeBarFill.localScale = new Vector3(0, 1, 1);
        happinessBarFill.localScale = new Vector3(CustomerManager.Instance.GetAverageCustomerHappiness(), 1, 1);
    }
}