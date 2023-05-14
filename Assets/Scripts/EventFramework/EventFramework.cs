using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventFramework : Singleton<EventFramework>
{
    [SerializeField] private EventComicConvention.EventParams comicConventionParams;

    private List<MallEvent> activeEvents;

    public EventComicConvention.EventParams ComicConventionParams { get { return comicConventionParams; } }

    public MallEvent[] ActiveEvents { get { return activeEvents.ToArray(); } }

    protected override void Awake()
    {
        activeEvents = new();
        StartCoroutine(checkForNewEvents());
    }

    public void StartEvent(MallEvent mallEvent)
    {
        activeEvents.Add(mallEvent);
        mallEvent.StartEvent(delegate { activeEvents.Remove(mallEvent); });

        // Antti sound - Celebrating customers
        // We'll probably want to move this later but for now play the cheering sound when an event is started
    }

    public bool HasActiveEventOfType<T>() where T : MallEvent
    {
        return activeEvents.Exists(o => o is T);
    }

    private IEnumerator checkForNewEvents()
    {
        while(true)
        {
            // Every x seconds do an RNG check to start a new event
            yield return new WaitForSeconds(120.0f);

            float r = Random.value;

            if(r <= 0.2f)
            {
                EventComicConvention comicConventionEvent = new();
                if(comicConventionEvent.CanStartEvent())
                {
                    StartEvent(comicConventionEvent);
                }
            }
        }
    }
}
