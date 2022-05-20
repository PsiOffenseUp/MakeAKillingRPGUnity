using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public const uint minutesPerHour = 60;
    public const uint hoursPerDay = 24;
    public const uint minuteLength = 60; //Minute length in time units (frames by default)
    public const uint hourLength = minuteLength * minutesPerHour;
    public const uint dayLength = hourLength * hoursPerDay;
    public const uint hoursPerTimeSegment = hoursPerDay / 4;

    uint timePerFrame = 20;
    public uint currentHour { get; private set; }
    public uint currentMinute { get; private set; }
    public uint currentTime { get; private set; }
    uint nextHour, nextMinute; //Used to calculate the next hour and minute before updating

    bool frozen = false;

    public enum TimeOfDay { DAWN, MORNING, EVENING, NIGHT }
    public TimeOfDay timeOfDay { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        currentHour = 0;
        currentMinute = 0;
        timeOfDay = TimeOfDay.DAWN;
    }

    // Update is called once per frame
    void Update()
    {
        if (frozen)
            return;

        currentTime += timePerFrame;
        nextHour = currentTime / hourLength;
        nextMinute = (currentTime % hourLength) / minuteLength;

        if (nextHour != currentHour) //If an hour is about to pass
        {
            currentHour = nextHour;
            currentMinute = nextMinute;
            OnHourChange();
        }
        else if (nextMinute != currentMinute) //If the minute is changing, but not the hour, update UI
        {
            currentMinute = nextMinute;
            GameplayManager.uiManager.UpdateMinute(currentMinute);
        }

        //Otherwise, neither the minute nor hour needs updated
    }

    #region Events for time passing

    //Called when an hour passes
    void OnHourChange()
    {
        currentHour %= hoursPerDay; //Wrap around in the case of a new day

        //Check if the time of day has changed
        switch(currentHour / hoursPerTimeSegment) //If we're on a time of day boundary
        {
            case 0: //If this is a new day
                timeOfDay = TimeOfDay.DAWN;
                StartNewDay();
                break;
            case 1: //Dawn -> Morning
                timeOfDay = TimeOfDay.MORNING;
                OnTimeOfDayChange();
                break;
            case 2: //Morning -> Evening
                timeOfDay = TimeOfDay.EVENING;
                OnTimeOfDayChange();
                break;
            case 3: //Evening -> Night
                timeOfDay = TimeOfDay.NIGHT;
                OnTimeOfDayChange();
                break;
        }

        //Update the hour UI
        GameplayManager.uiManager.UpdateHourAndMinute(currentHour, currentMinute);

        //Signal the event that the hour has changed
        TimeAffectedObject.UpdateHourChanged();
    }

    //Called when the time of day changes
    void OnTimeOfDayChange()
    {
        TimeAffectedObject.UpdateTimeChanged();
        GameplayManager.uiManager.UpdateTimeOfDay();
    }

    void OnNewDay()
    {
        OnTimeOfDayChange(); //Signal that the time of day changed
    }
    #endregion

    #region Methods to change the passage of time
    public void FreezeClock() { frozen = true; }
    public void UnfreezeClock() { frozen = false; }

    /// <summary> Changes how many ticks of the clock occur per frame. Default is 1 </summary>
    /// <param name="new_speed"></param>
    public void ChangeTimePassageSpeed(uint new_speed = 1)
    {
        timePerFrame = new_speed;
    }

    /// <summary> Starts the day over and signals related events </summary>
    public void StartNewDay()
    {
        OnNewDay();
        TimeAffectedObject.UpdateNewDayStarted(); //Let all objects affected by time know that a new day started
    }

    public void ReadTimeOfDayFromSave(BoogalooGame.Save save)
    {
        currentHour = save.hour;
        currentMinute = save.minute;
        timePerFrame = save.timeSpeed;
        timeOfDay = (TimeOfDay)(currentHour / 4);

        GameplayManager.uiManager.UpdateHourAndMinute(currentHour, currentMinute);
        GameplayManager.uiManager.UpdateTimeOfDay();
    }
    #endregion
}
