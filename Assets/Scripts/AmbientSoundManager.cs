using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundManager : Singleton<AmbientSoundManager>
{
    private uint ambianceEvent;
    private bool ambiancePlaying;
    private bool isPaused;

    protected override void Init()
    {
        base.Init();
        Persist = true;
    }

    protected override void Awake()
    {
        base.Awake();
        // Antti sound - Mall Music

        //isPaused = false;

        //AkSoundEngine.SetState("playAmbiance", "yes");
        //ambiancePlaying = false;
        //AkSoundEngine.PostEvent("playMusic", gameObject);

        // Not sure how this implementation will work exactly but I guess play the music here then adjust it with the values in Update?
    }

    void Start()
    {
        isPaused = false;

        AkSoundEngine.SetState("playAmbiance", "yes");
        ambiancePlaying = false;
        AkSoundEngine.PostEvent("playMusic", gameObject);
    }

    void Update()
    {
        int customersInMall = CustomerManager.Instance.CustomerCount;
        int maxCapacity = 100;
        float mallPercentageFull = (float) customersInMall / maxCapacity;

        AkSoundEngine.SetRTPCValue("PercentageMallFull", mallPercentageFull);
        // Change music based on how busy the mall is, I'd say for now ~100 customers counts as full capacity

        if(PlayerController.Instance)
        {
            CameraController.Instance.GetMousePointOnGround(out Vector3 mousePoint);
            Vector2Int coords = Mall.Instance.CellCoordsFromWorldPos(mousePoint);
            bool cameraIsOutsideMall = Mall.CurrentFloor.GetCell(coords) == null;

            isPaused = PlayerController.Instance.IsPaused;
            // Annti sound - Menu music
            // I think when the game is paused we should stop the mall music and ambient sounds and play the menu music
            if (isPaused)
            {
                AkSoundEngine.SetState("IsGamePaused", "Yes");
                //AkSoundEngine.StopPlayingID(tannoyEvent, 1000, AkCurveInterpolation.AkCurveInterpolation_Linear);
                if (ambiancePlaying)
                {
                    AkSoundEngine.StopPlayingID(ambianceEvent, 1000, AkCurveInterpolation.AkCurveInterpolation_Linear);
                    ambiancePlaying = false;
                }
            }
            else
            {
                AkSoundEngine.SetState("IsGamePaused", "No");

                // Is it possible to fade in the outside ambient sounds when the camera is outside the mall?
                if (cameraIsOutsideMall && !ambiancePlaying)
                {
                    ambianceEvent = AkSoundEngine.PostEvent("startAmbiance", gameObject);
                    ambiancePlaying = true;
                }
                else if (!cameraIsOutsideMall && ambiancePlaying)
                {
                    AkSoundEngine.StopPlayingID(ambianceEvent, 1000, AkCurveInterpolation.AkCurveInterpolation_Linear);
                    ambiancePlaying = false;
                }
            }
        }
    }
}
