using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SleepingCot : InteractableObject
{
    [System.Serializable]
    public class TimeOption
    {
        public int hour, minute;
        public string name;
        public Material uiMaterial;
    }

    [SerializeField] GameObject sleepUI;
    [SerializeField] List<TimeOption> timeOptions;
    [SerializeField] GameObject optionsAnchor;
    [SerializeField] GameObject cursor;
    [SerializeField] Image[] bgBoxes;
    [SerializeField] UIAnimation sleepAnimation, sleepBGAnimation;
    Dictionary<Clock.TimeOfDay, TimeOption> options;
    int selectedOption;
    int optionCount;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        selectedOption = 0;

        //Make sure UI gets drawn to main camera
        Canvas sleepCanvas = sleepUI.GetComponent<Canvas>();
        sleepCanvas.worldCamera = GameplayManager.mainCamera;
        sleepCanvas.planeDistance = 2;

        //Put everything in a dictionary for optimization (Unity won't allow dictionaries to be serialized)
        uint i = 0;
        options = new Dictionary<Clock.TimeOfDay, TimeOption>();
        foreach (TimeOption option in timeOptions)
        {
            options[(Clock.TimeOfDay)i] = option;
            i++;

            //TODO: Generate the options within the UI and set the cursor
        }
        timeOptions.Clear();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        sleepAnimation.Update();
        sleepBGAnimation.Update();

        //TODO: Handle interacting with the UI
    }

    #region Methods for Changing UI

    void OpenSleepUI()
    {
        sleepUI.SetActive(true);
        ChangeBGBoxes();
    }

    void CloseSleepUI()
    {
        sleepUI.SetActive(false);
    }

    void ChangeBGBoxes()
    {
        //Update the material for all of the boxes
        foreach (Image image in bgBoxes)
            image.material = options[(Clock.TimeOfDay)selectedOption].uiMaterial;
    }


    #endregion

    #region Methods for Updating the Time of Day

    void UpdateClock()
    {
        
    }

    #endregion

    #region Interaction Methods

    protected override void OnPlayerInteract()
    {
        base.OnPlayerInteract();

        OpenSleepUI();
    }

    protected override void OnPlayerFinishInteract()
    {
        base.OnPlayerFinishInteract();

        CloseSleepUI();
    }

    #endregion
}
