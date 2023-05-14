using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotificationButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textContent;
    [SerializeField] private Button button;
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Animator animator;
    [SerializeField] private float lifetime;

    private float timeAlive;
    private UnityAction onClose;

    public RectTransform Rect { get { return rect; } }

    void Update()
    {
        timeAlive += Time.unscaledDeltaTime;
        if(timeAlive > lifetime && onClose != null)
        {
            onClose();
        }
    }

    public void Init(string content, UnityAction onClick, UnityAction onClose)
    {
        textContent.text = content;
        this.onClose = onClose;

        if(onClick != null)
        {
            button.onClick.AddListener(onClick);
        }
        if(onClose != null) 
        {
            closeButton.onClick.AddListener(onClose);
        }

        animator.SetTrigger("Open");
    }
}
