using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TannoyManager : Singleton<TannoyManager>
{
    [SerializeField] private float tannoyInterval;

    private uint tannoyEvent;

    void Start()
    {
        if(PlayerController.Instance)
        {
            StartCoroutine(tannoyIEnum(PlayerController.Instance.IsPaused));
        }
    }

    void Update()
    {
        if (PlayerController.Instance)
        {
            if(PlayerController.Instance.IsPaused)
            {
                AkSoundEngine.StopPlayingID(tannoyEvent, 1000, AkCurveInterpolation.AkCurveInterpolation_Linear);
            }
        }
    }

    // Play a random tannoy announcement at a regular interval
    private IEnumerator tannoyIEnum(bool ispaused)
    {
        while (true)
        {
            yield return new WaitForSeconds(tannoyInterval);
            if (!ispaused && TimeManager.Instance.TimeSpeed < 3 && PlayerController.Instance)
            {
                tannoyEvent = AkSoundEngine.PostEvent("tannoy", gameObject);
            }
        }
    }
}
