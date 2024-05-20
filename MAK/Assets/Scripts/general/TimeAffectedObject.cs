using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoogalooGame;
using UnityEngine.SceneManagement;

public class TimeAffectedObject : Object
{
    static Dictionary<int, TimeAffectedObject> timedObjects = new Dictionary<int, TimeAffectedObject>();
    public TimeData lastUpdateTime { get; protected set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        timedObjects[GetInstanceID()] = this;

        if(GameplayManager.saveData == null)
            Debug.Log("There is no loaded save file...");

        if (ReferenceEquals(lastUpdateTime, null)) //If the last update time was never set, initialize it
            UpdateLastUpdateTime();
        else //Else call any necessary methods for time updating from the loaded time
        {
            //Check if the last update time was in the past far enough to call any methods
            if (GameplayManager.clock.time.day != lastUpdateTime.day ||
                GameplayManager.clock.time.hour != lastUpdateTime.hour)
                OnHourChange();

            if (GameplayManager.clock.time.day != lastUpdateTime.day)
                OnNewDay();
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    #region Time Update Events
    /// <summary> Called for all TimeAffectedObjects when an hour passes </summary>
    protected virtual void OnHourChange(){ UpdateLastUpdateTime(); }
    /// <summary> Called for all TimeAffectedObjects when the time of day changes (afternoon->evening, etc.) </summary>
    protected virtual void OnTimeChange() { UpdateLastUpdateTime(); }
    protected virtual void OnNewDay() { UpdateLastUpdateTime(); }

    public static void UpdateHourChanged()
    {
        foreach (KeyValuePair<int, TimeAffectedObject> TAO in timedObjects)
            TAO.Value.OnHourChange();
    }
    public static void UpdateTimeChanged()
    {
        foreach (KeyValuePair<int, TimeAffectedObject> TAO in timedObjects)
        {
            TAO.Value.OnTimeChange();
            TAO.Value.OnHourChange();
        }
    }

    public static void UpdateNewDayStarted()
    {
        foreach (KeyValuePair<int, TimeAffectedObject> TAO in timedObjects)
            TAO.Value.OnNewDay();
    }

    //Sets the latest update time to the current time in-game
    protected void UpdateLastUpdateTime() { lastUpdateTime = GameplayManager.clock.time; }

    #endregion

    #region Interface overrides for saving data
    public override void CopyToSave(Save save)
    {
        save.objectData[uniqueId] = new ObjectData(lastUpdateTime, transform.position, SceneManager.GetActiveScene().name);
    }

    public override void LoadFromSave(Save save)
    {
        base.LoadFromSave(save);

        ObjectData temp;

        //Try to read ObjectData from the save file
        if (save.objectData.TryGetValue(uniqueId, out temp))
        {
            this.transform.position = temp.position;
            this.lastUpdateTime = temp.lastUpdateTime;

            TimeData timePassed = lastUpdateTime - GameplayManager.clock.time; //Get time passed since last load

            //If more than one in-game hour has passed since this object was last loaded, signal new day
            if (timePassed.hour > 0)
                OnHourChange();
            //If more than one in-game day has passed since this object was last loaded, signal new day
            if (timePassed.day > 0)
                OnNewDay();
        }
        else //If we could not find an existing object data, set to default values
            OnFirstLoad();

    }
    #endregion

    #region Other Save Related Methods

    //Called when the TAO gets deloaded with a scene
    protected virtual void OnLeaveScene()
    {
        timedObjects.Remove(this.GetInstanceID());
    }

    protected virtual void OnDestroy()
    {
        timedObjects.Remove(this.GetInstanceID()); //Remove this object from the list of objects
    }

    //Goes through all time affected objects and saves them
    public static void SaveAllTimeAffectedObjects()
    {
        //Save every single time affected object that is relevant
        foreach (KeyValuePair<int, TimeAffectedObject> TAO in timedObjects)
        {
            TAO.Value.UpdateLastUpdateTime();
            TAO.Value.CopyToSave(GameplayManager.saveData);
        }
    }

    //Cleans up the list of Time Affected Objects so that any that are going to be unloaded 
    //will be removed. This should only be called right before leaving a scene
    public static void CleanTimeAffectedObjectList()
    {
        /*
        foreach (KeyValuePair<int, TimeAffectedObject> TAO in timedObjects)
        {
        }*/
    }

    //Removes this object from being saved in the save file
    public void RemoveObjectFromSaveFile()
    {
        //TODO
    }

    //Called the first time the item is ever loaded per save file
    protected virtual void OnFirstLoad()
    {
        UpdateLastUpdateTime();
    }
    #endregion

    #region Other Overrides

    protected override void OnLanding() { base.OnLanding(); }
    protected override void OnAerial() { base.OnAerial(); }

    #endregion
}
