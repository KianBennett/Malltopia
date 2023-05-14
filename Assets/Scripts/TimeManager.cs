using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    [SerializeField] private Color moonlightColour;
    [SerializeField] private float dayDuration;
    [SerializeField] private float startTime;
    [SerializeField] private float sunriseTime;
    [SerializeField] private float sunriseDuration;
    [SerializeField] private float sunsetTime;
    [SerializeField] private float sunsetDuration;

    private int timeSpeed;
    private float sunlightIntensity;
    private List<Material> exteriorMaterials;

    // The number of seconds passed since the day has started at midnight
    // E.g. at midday with a dayDuration of 180s, dayTimeInSeconds = 90s
    private float dayTimeInSeconds;

    public int TimeSpeed { get { return timeSpeed; } }
    public float DayTime { get { return dayTimeInSeconds; } }
    public float DayDuration { get { return dayDuration; } }
    public float HourDuration { get { return DayDuration / 24.0f; } }

    protected override void Awake()
    {
        exteriorMaterials ??= new();
        timeSpeed = 1;
        dayTimeInSeconds = startTime;
    }

    void Update()
    {
        dayTimeInSeconds += Time.deltaTime;
        if(dayTimeInSeconds > dayDuration)
        {
            dayTimeInSeconds = 0;
        }

        if(dayTimeInSeconds < sunriseTime || dayTimeInSeconds >= sunsetTime + sunsetDuration)
        {
            sunlightIntensity = 0;
        }
        else if(dayTimeInSeconds >= sunriseTime && dayTimeInSeconds < sunriseTime + sunriseDuration) 
        {
            sunlightIntensity = (dayTimeInSeconds - sunriseTime) / sunriseDuration;
        }
        else if(dayTimeInSeconds >= sunsetTime && dayTimeInSeconds < sunsetTime + sunsetDuration)
        {
            sunlightIntensity = 1 - (dayTimeInSeconds - sunsetTime) / sunsetDuration;
        }
        else
        {
            sunlightIntensity = 1;
        }

        foreach(Material material in exteriorMaterials)
        {
            material.SetFloat("_SunlightIntensity", sunlightIntensity);
        }
    }

    public void RegisterMaterial(Material material)
    {
        exteriorMaterials ??= new();

        if(!exteriorMaterials.Contains(material))
        {
            exteriorMaterials.Add(material);
            material.SetColor("_MoonlightColor", moonlightColour);
            material.SetFloat("_SunlightIntensity", sunlightIntensity);
        }
    }

    public void IncreaseTimeSpeed()
    {
        if (timeSpeed < 3)
        {
            timeSpeed++;
        }
        UpdateTimeScaleFromSpeed();
    }

    public void DecreaseTimeSpeed()
    {
        if (timeSpeed > 0)
        {
            timeSpeed--;
        }
        UpdateTimeScaleFromSpeed();
    }

    public void SetTimeSpeed(int timeSpeed)
    {
        if (timeSpeed < 0 || timeSpeed > 3)
        {
            Debug.LogFormat("Trying to set an invalid timeSpeed: ({0})", timeSpeed);
        }
        this.timeSpeed = timeSpeed;
        UpdateTimeScaleFromSpeed();
    }

    public void UpdateTimeScaleFromSpeed()
    {
        switch (timeSpeed)
        {
            case 0:
                Time.timeScale = 0;
                break;
            case 1:
                Time.timeScale = 1;
                break;
            case 2:
                Time.timeScale = 4;
                break;
            case 3:
                Time.timeScale = 16;
                break;
        }
    }

    public void ConvertCurrentTimeTo24Hr(out int hours, out int mins)
    {
        ConvertTimeTo24Hr(dayTimeInSeconds, out hours, out mins);
    }

    public static void ConvertTimeTo24Hr(float dayTime, out int hours, out int mins)
    {
        float percentage = dayTime / Instance.DayDuration;

        hours = Mathf.FloorToInt(percentage * 24);
        mins = Mathf.FloorToInt(percentage * 1440) - hours * 60;
    }
}
