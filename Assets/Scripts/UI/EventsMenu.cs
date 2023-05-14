using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventsMenu : MonoBehaviour
{
    [SerializeField] private EventListButton eventButtonPrefab;
    [SerializeField] private RectTransform scrollViewContent;
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI textNoEvents;

    private List<EventListButton> buttonList;

    public void Open()
    {
        populateList();
        gameObject.SetActive(true);
        animator.SetTrigger("Open");
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void populateList()
    {
        buttonList ??= new();

        foreach(EventListButton button in buttonList)
        {
            Destroy(button.gameObject);
        }

        buttonList.Clear();

        textNoEvents.gameObject.SetActive(EventFramework.Instance.ActiveEvents.Length == 0);

        for(int i = 0; i < EventFramework.Instance.ActiveEvents.Length; i++)
        {
            MallEvent mallEvent = EventFramework.Instance.ActiveEvents[i];
            EventListButton button = Instantiate(eventButtonPrefab, scrollViewContent);
            button.Rect.anchoredPosition = new Vector2(button.Rect.anchoredPosition.x, -38 - 60 * i);
            button.Init(mallEvent.GetNotificationText(), 
                delegate 
                {
                    Close();
                    HUD.Instance.EventDetailsMenu.Open(mallEvent, Open);
                });
        }

        scrollViewContent.sizeDelta = new Vector2(scrollViewContent.sizeDelta.x, 60 + 60 * buttonList.Count);
    }
}
