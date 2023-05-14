using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NotificationsManager : MonoBehaviour
{
    [SerializeField] private NotificationButton notificationButtonPrefab;
    [SerializeField] private int maxNotificationsOnScreen;

    private List<NotificationButton> notificationButtons;

    void Awake()
    {
        notificationButtons = new();
    }

    void Update()
    {
        for(int i = 0; i < notificationButtons.Count; i++)
        {
            float y = Mathf.MoveTowards(notificationButtons[i].Rect.anchoredPosition.y, 120 + 60 * i, Time.deltaTime * 500);
            notificationButtons[i].Rect.anchoredPosition = new Vector2(notificationButtons[i].Rect.anchoredPosition.x, y);
        }

        // Remove excess notifications
        if (notificationButtons.Count > maxNotificationsOnScreen)
        {
            for (int i = maxNotificationsOnScreen; i < notificationButtons.Count; i++)
            {
                Destroy(notificationButtons[i].gameObject);
            }
            notificationButtons.RemoveRange(maxNotificationsOnScreen, notificationButtons.Count - maxNotificationsOnScreen);
        }

        //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //    ShowNotification("This is a new notification!", null);
        //}
    }

    public void ShowNotification(string content, UnityAction onClick= null)
    {
        NotificationButton button = Instantiate(notificationButtonPrefab, transform);
        button.Init(content, 
            delegate
            {
                CloseNotification(button);
                onClick?.Invoke();
            }, 
            delegate { CloseNotification(button); }
        );
        button.Rect.anchoredPosition = new Vector2(-20, 120);
        notificationButtons.Insert(0, button);
    }

    public void CloseNotification(NotificationButton button)
    {
        notificationButtons.Remove(button);
        Destroy(button.gameObject);
    }
}
