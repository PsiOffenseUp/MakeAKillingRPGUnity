using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAffectedObject : Object
{
    static List<TimeAffectedObject> timedObjects = new List<TimeAffectedObject>();

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        timedObjects.Add(this);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected virtual void OnDestroy()
    {
        timedObjects.Remove(this); //Remove this object from the list of objects
    }

    #region Time Update Events
    /// <summary> Called for all TimeAffectedObjects when an hour passes </summary>
    protected virtual void OnHourChange(){ }
    /// <summary> Called for all TimeAffectedObjects when the time of day changes (afternoon->evening, etc.) </summary>
    protected virtual void OnTimeChange() { }
    protected virtual void OnNewDay() { }

    public static void UpdateHourChanged()
    {
        foreach (TimeAffectedObject TAO in timedObjects)
            TAO.OnHourChange();
    }
    public static void UpdateTimeChanged()
    {
        foreach (TimeAffectedObject TAO in timedObjects)
        {
            TAO.OnTimeChange();
            TAO.OnHourChange();
        }
    }

    public static void UpdateNewDayStarted()
    {
        foreach (TimeAffectedObject TAO in timedObjects)
            TAO.OnNewDay();
    }

    #endregion

    protected override void OnLanding() { }
}
