using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class EventListButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textContent;
    [SerializeField] private Button button;
    [SerializeField] private RectTransform rect;

    public RectTransform Rect { get { return rect; } }

    public void Init(string text, UnityAction onClick)
    {
        textContent.text = text;

        if(onClick != null)
        {
            button.onClick.AddListener(onClick);
        }
    }
}
