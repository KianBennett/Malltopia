using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class MallEvent
{
    private Coroutine eventCoroutine;
    private bool hasStopped;
    private UnityAction onEventStopped;
    private float timeStarted;

    public bool HasStopped { get { return hasStopped; } }
    public float TimeStarted { get { return timeStarted; } }

    public virtual void StartEvent(UnityAction onEventStopped)
    {
        if(eventCoroutine != null)
        {
            EventFramework.Instance.StopCoroutine(eventCoroutine);
        }
        eventCoroutine = EventFramework.Instance.StartCoroutine(eventIEnum());

        timeStarted = TimeManager.Instance.DayTime;
    }

    public virtual void EndEvent()
    {
        if (eventCoroutine != null)
        {
            EventFramework.Instance.StopCoroutine(eventCoroutine);
        }

        if (onEventStopped != null)
        {
            onEventStopped();
        }

        hasStopped = true;
    }

    public virtual bool CanStartEvent()
    {
        return true;
    }

    public abstract string GetTitle();
    public abstract string GetNotificationText();
    public abstract string GetDescription();

    protected abstract IEnumerator eventIEnum();
}