using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PurchaseButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI textCost;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Button button;

    public RectTransform Rect { get { return rect; } }
    public Button Button { get { return button; } }

    public void SetValues(string name, int cost)
    {
        textName.text = name;
        textCost.text = "$" + cost;
    }

    public void SetValuesRoom(string name, int cost, int tileCost)
    {
        textName.text = name;
        textCost.text = "$" + cost + "\n<size=22><alpha=#88> ($" + tileCost + "/Sp)";
    }

    public void SetValuesWithWage(string name, int cost, int wage)
    {
        textName.text = name;
        textCost.text = "$" + cost + "\n<size=22><alpha=#88> ($" + wage + "/h)";
    }
}