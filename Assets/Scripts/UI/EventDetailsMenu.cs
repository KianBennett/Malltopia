using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class EventDetailsMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textTitle;
    [SerializeField] private TextMeshProUGUI textTimeStarted;
    [SerializeField] private TextMeshProUGUI textContent;
    [SerializeField] private Animator animator;

    private UnityAction onClose;

    public void Open(MallEvent mallEvent, UnityAction onClose = null)
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Open");

        this.onClose = onClose;

        TimeManager.ConvertTimeTo24Hr(mallEvent.TimeStarted, out int hours, out int mins);

        textTitle.text = mallEvent.GetTitle();
        textTimeStarted.text = string.Format("Started at {0}:{1}", hours.ToString("D2"), mins.ToString("D2"));
        textContent.text = mallEvent.GetDescription();
    }

    public void Close()
    {
        gameObject.SetActive(false);

        if(onClose != null)
        {
            onClose();
        }
    }
}
