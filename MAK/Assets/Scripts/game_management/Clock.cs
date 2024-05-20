using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoogalooGame;

//Keeps track of important time data
[System.Serializable]
public class TimeData
{
    public uint day;
    public uint hour;
    public uint minute;

    #region Constructors and Initializers

    public TimeData()
    {
        hour = 10;
        minute = 0;
        day = 1;
    }
    #endregion

    #region Operator Overloads
    public static bool operator == (TimeData first, TimeData second)
    {
        return first.minute == second.minute && first.hour == second.hour && first.day == second.day;
    }

    public static bool operator != (TimeData first, TimeData second)
    {
        return first.minute != second.minute || first.hour != second.hour || first.day != second.day;
    }

    //Tells whether the first time comes after the second time
    public static bool operator > (TimeData first, TimeData second)
    {
        if (first.day == second.day)
        {
            if (first.hour == second.hour)
                return first.minute > second.minute;
            else
                return first.hour > second.hour;
        }
        else
            return first.day > second.day;
    }

    //Tells whether the first time comes before the second time
    public static bool operator <(TimeData first, TimeData second)
    {
        if (first.day == second.day)
        {
            if (first.hour == second.hour)
                return first.minute < second.minute;
            else
                return first.hour < second.hour;
        }
        else
            return first.day < second.day;
    }

    //Returns a TimeData object representing the difference in Time between two TimeDatas. Assumes the first
    //TimeData parameter is > than the second
    public static TimeData operator -(TimeData first, TimeData second)
    {
        TimeData difference = new TimeData();
        difference.day = first.day - second.day;

        if (second.hour > first.hour) //If the hour of the second is greater, borrow one day before subtraction
        {
            difference.day--;
            difference.hour = Clock.hoursPerDay + first.hour - second.hour;
        }
        else
            difference.hour = first.hour - second.hour;

        if (second.minute > first.minute) //If the minute of the second is greater, borrow an hour
        {
            if (difference.hour == 0) //If hour is zero, must borrow from day
            {
                difference.day--;
                difference.hour = Clock.hoursPerDay;
            }
            difference.hour--;
            difference.minute = Clock.minutesPerHour + first.minute - second.minute;
        }
        else
            difference.minute = first.minute - second.minute;

        return difference;
    }
    #endregion

    #region Overrides
    public override bool Equals(object obj)
    {
        return this == (TimeData)obj;
    }

    public override int GetHashCode()
    {
        //Returns the total number of minutes since the initial time of 0 that this Time represents
        uint cummulativeValue = day;
        cummulativeValue *= Clock.hoursPerDay;
        cummulativeValue += hour;
        cummulativeValue *= Clock.minutesPerHour;
        cummulativeValue += minute;
        return (int)cummulativeValue;
    }

    public override string ToString()
    {
        return "Day " + day + "- " + hour + ":" + (minute < 10 ? "0" : "") + minute;
    }
    #endregion
}

public class Clock : MonoBehaviour, ISaveable
{
    public const uint minutesPerHour = 60;
    public const uint hoursPerDay = 24;
    public const uint minuteLength = 60; //Minute length in time units (frames by default)
    public const uint hourLength = minuteLength * minutesPerHour;
    public const uint dayLength = hourLength * hoursPerDay;
    public const uint hoursPerTimeSegment = hoursPerDay / 4;

    uint timePerFrame = 1;
    public uint currentTime { get; private set; }
    public TimeData time { get; private set; }
    uint nextHour, nextMinute; //Used to calculate the next hour and minute before updating

    bool frozen = false;

    //Weather and time related variables and enums
    public enum TimeOfDay { DAWN, MORNING, EVENING, NIGHT }
    public TimeOfDay timeOfDay;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (frozen)
            return;

        currentTime += timePerFrame;
        nextHour = currentTime / hourLength;
        nextMinute = (currentTime % hourLength) / minuteLength;

        if (nextHour != time.hour) //If an hour is about to pass
        {
            time.hour = nextHour;
            time.minute = nextMinute;
            OnHourChange();
        }
        else if (nextMinute != time.minute) //If the minute is changing, but not the hour, update UI
        {
            time.minute = nextMinute;
            GameplayManager.uiManager.UpdateMinute(time.minute);
        }

        //Otherwise, neither the minute nor hour needs updated
    }

    #region Events for time passing

    //Called when an hour passes
    void OnHourChange()
    {
        time.hour %= hoursPerDay; //Wrap around in the case of a new day

        //Check if the time of day has changed
        TimeOfDay oldTOD = timeOfDay;
        timeOfDay = (TimeOfDay)(time.hour / hoursPerTimeSegment); //Update the TimeOfDay
        if(oldTOD != timeOfDay) //If the time of day has changed
        {
            //If it is dawn, a new day has started
            if (timeOfDay == TimeOfDay.DAWN)
                OnNewDay();

            OnTimeOfDayChange();
        }

        //Update the hour UI
        GameplayManager.uiManager.UpdateHourAndMinute(time.hour, time.minute);

        //Signal the event that the hour has changed
        TimeAffectedObject.UpdateHourChanged();
    }

    //Called when the time of day changes
    void OnTimeOfDayChange()
    {
        TimeAffectedObject.UpdateTimeChanged();
        GameplayManager.gameplayManager.SaveGame(); //Save the game
        GameplayManager.uiManager.UpdateTimeOfDay(); //Let the UI update the time while saving
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

    #endregion

    #region Save Related Methods

    public void CopyToSave(Save save)
    {
        save.time = time;
        save.timeSpeed = timePerFrame;
    }

    public void LoadFromSave(Save save)
    {
        time = save.time;
        currentTime = time.hour * minutesPerHour * minuteLength + time.minute * minuteLength;
        timePerFrame = save.timeSpeed;
        //GameplayManager.uiManager.UpdateHourAndMinute(time.hour, time.minute); //Set the time on the UI
    }

    #endregion
}
