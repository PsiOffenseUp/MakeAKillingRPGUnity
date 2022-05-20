using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[SerializeField] Canvas uiCanvas;
    [SerializeField] TMPro.TextMeshProUGUI hourText, minuteText;
    [SerializeField] Color[] timeColors;

    uint hour, minute;

    // Start is called before the first frame update
    void Start()
    {
        UpdateHourColor();
	}

    // Update is called once per frame
    void Update()
    {
	}

    #region Helper methods
    public void SetRenderCamera(Camera camera)
    {
		uiCanvas.worldCamera = camera;
    }

    public void UpdateHourColor()
    {
        hourText.color = Color.Lerp(timeColors[(int)GameplayManager.clock.timeOfDay],
            timeColors[((int)GameplayManager.clock.timeOfDay + 1) % 4],
            (float)(hour % 4) / (Clock.hoursPerDay / 4));
    }
    #endregion

    #region Time of Day UI Methods
    public void UpdateMinute(uint minute)
    {
        //Update the minute text
        this.minute = minute;
        if (minute < 10)
            minuteText.text = "0" + minute.ToString();
        else
            minuteText.text = minute.ToString();

        //TODO
        //Update the position of the marker
    }

    public void UpdateHourAndMinute(uint hour, uint minute)
    {
        //Update the hour
        this.hour = hour;
        if (hour < 10)
            hourText.text = "0" + hour.ToString();
        else
            hourText.text = hour.ToString();

        //Update the color of the hours
        UpdateHourColor();

        //Update the minute using the existing method
        UpdateMinute(minute);
    }

    public void UpdateTimeOfDay()
    {
        switch(GameplayManager.clock.timeOfDay)
        {
            case Clock.TimeOfDay.DAWN:
                break;
            case Clock.TimeOfDay.MORNING:
                break;
            case Clock.TimeOfDay.EVENING:
                break;
            case Clock.TimeOfDay.NIGHT:
                break;
        }
    }
    #endregion
}
