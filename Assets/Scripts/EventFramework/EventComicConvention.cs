using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EventComicConvention : MallEvent
{
    [System.Serializable]
    public class EventParams
    {
        public ShopParams comicBookShop;
        public float delayInHours = 72;
        public float durationInHours = 8;
        public float customerSpawnIntervalMin = 6;
        public float customerSpawnIntervalMax = 12;

        public string title;
        public string notificationText;
        [TextArea]
        public string description;
    }

    private EventParams Params { get { return EventFramework.Instance.ComicConventionParams; } }

    public override bool CanStartEvent()
    {
        // The event can start if there's at least one shop in the mall
        return Mall.CurrentFloor.FindShops().Length > 0
            && !EventFramework.Instance.HasActiveEventOfType<EventComicConvention>();
    }

    public override string GetTitle()
    {
        return Params.title;
    }

    public override string GetNotificationText()
    {
        return Params.notificationText;
    }

    public override string GetDescription()
    {
        return Params.description;
    }

    protected override IEnumerator eventIEnum()
    {
        HUD.Instance.Notifications.ShowNotification(Params.notificationText, 
            delegate { HUD.Instance.EventDetailsMenu.Open(this); });

        yield return new WaitForSeconds(Params.delayInHours * TimeManager.Instance.HourDuration);

        // If the comic book shop has been removed by the time the event begins, notify the player
        if (Mall.CurrentFloor.FindRoomsOfType(Params.comicBookShop).Length == 0)
        {
            HUD.Instance.Notifications.ShowNotification("The comic convention has begun but you don't have a comic shop!");
        }
        else
        {
            HUD.Instance.Notifications.ShowNotification("The comic convention has begun!");
        }

        float duration = Params.durationInHours * TimeManager.Instance.HourDuration;

        while(duration > 0)
        {
            float interval = Random.Range(Params.customerSpawnIntervalMin, Params.customerSpawnIntervalMax);

            yield return new WaitForSeconds(interval);
            duration -= interval;

            Customer customer = CustomerManager.Instance.SpawnCustomerAtEntrance();
            customer.StartingShop = Params.comicBookShop;
        }

        HUD.Instance.Notifications.ShowNotification("The comic convention has ended");
    }
}
