using UnityEngine;
using TMPro;

public class TimeControlsPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textTime;
    [SerializeField] private GameObject[] speedIcons;

    void Update()
    {
        if (textTime != null)
        {
            TimeManager.Instance.ConvertCurrentTimeTo24Hr(out int hours, out int mins);
            textTime.text = string.Format("{0}:{1}", hours.ToString("D2"), mins.ToString("D2"));
        }

        for(int i = 0; i < speedIcons.Length; i++)
        {
            speedIcons[i].SetActive(i == TimeManager.Instance.TimeSpeed);
        }
    }

    public void pause()
    {
        TimeManager.Instance.SetTimeSpeed(0);
    }

    public void oneSpeed()
    {
        TimeManager.Instance.SetTimeSpeed(1);
    }

    public void twoSpeed()
    {
        TimeManager.Instance.SetTimeSpeed(2);
    }

    public void threeSpeed()
    {
        TimeManager.Instance.SetTimeSpeed(3);
    }
}
