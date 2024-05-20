using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIAnimation
{
    public Sprite[] displayImages;
    public int framesPerAnimFrame;
    public Image image;
    int frame;
    int framesOfAnimation;
    int animationLength;

    public void Initialize(Image image_to_animate_over, int frames_per_animation_frame)
    {
        this.image = image_to_animate_over;
        framesPerAnimFrame = frames_per_animation_frame;
    }

    //Resets animation to its first frame
    public void ResetAnimation()
    {
        if (displayImages == null)
            Debug.Log("displayImages i null!");
        frame = 0;
        framesOfAnimation = displayImages.Length;
        animationLength = framesOfAnimation * framesPerAnimFrame;
    }

    //Updates the animation and sets the sprite for the image
    public void Update()
    {
        //Go to the next frame of animation
        frame++;
        frame %= animationLength;

        //Update image to current frame
        image.sprite = displayImages[frame / framesPerAnimFrame];
    }

    //Sets the material to render the animation with
    public void SetMaterial(Material material)
    {
        image.material = material;
    }
}

public class UIManager : MonoBehaviour
{
    #region Classes for UI Info and Animations
    [System.Serializable]
    class UIInfo
    {
        public UIAnimation animation;
        public Material uiMaterial;
    }

    /*
    [System.Serializable]
    class ClockDigit
    {
        const float minScale = 0.75f;
        const float invMinScale = 1.0f - minScale;
        const float animationSpeed = 1.15f;
        const float PI = 3.1415926f;

        public TMPro.TextMeshProUGUI text;
        public Image animationImage;
        float timer;
        bool playingAnimation = true; //Used to lock animation across coroutines
        Material animationMaterial;

        #region Methods
        public IEnumerator PlayTickAnimation(uint targetValue, float delay = 0.0f) { 
            //Only play the animation if it is not already playing
            if(!playingAnimation)
            {
                yield return new WaitForSeconds(delay);

                timer = 0.0f; //Reset timer
                playingAnimation = true; //Lock the animation code for other coroutines
                while (timer < 0.5f) //If the animation is not finished, animate
                {
                    //Set the scale for the animation
                    animationMaterial.SetFloat("_Scale", 1.0f - invMinScale * Mathf.Sin(PI * timer));
                    timer += animationSpeed * Time.deltaTime; //Increase timer by time passed                
                     
                    yield return new WaitForEndOfFrame();
                }
                text.text = targetValue.ToString();
                while (timer < 1.0f) //If the animation is not finished, animate
                {
                    //Set the scale for the animation
                    animationMaterial.SetFloat("_Scale", 1.0f - invMinScale * Mathf.Sin(PI * timer));
                    timer += animationSpeed * Time.deltaTime; //Increase timer by time passed                

                    yield return new WaitForEndOfFrame();
                }
                animationMaterial.SetFloat("_Scale", 1.0f); //Set the scale back to normal
                playingAnimation = false; //Unlock animation
            }

            yield return null;
        }

        public void Initialize(Material material_base)
        {
           
            timer = 0.0f; //Set animation timer to 0
            
            animationMaterial = new Material(material_base); //Create a material so we can use it to animate
            animationImage.material = animationMaterial; //Apply the material to the image
            animationMaterial.SetFloat("Scale", 1.0f); //Initialize scale in animation
            playingAnimation = false;
        }

        #endregion
    }
    */
    #endregion

    [SerializeField] Canvas uiCanvas;
    [SerializeField] TMPro.TextMeshProUGUI moneyText, hourText, tensText, onesText;
    [SerializeField] List<UIInfo> timeInfo;
    [SerializeField] Image actionButton;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] RawImage loadingScreenImage;
    [SerializeField] GameObject roomInfoParent;
    [SerializeField] TMPro.TextMeshProUGUI roomName;
    [SerializeField] AnimationCurve roomAnimationCurve;
    [SerializeField] GameObject savingIcon;
    [SerializeField] Image timeOfDayIcon;
    [SerializeField] TMPro.TextMeshProUGUI actionText;
    //[SerializeField] ClockDigit hourDigit, tensPlaceDigit, onesPlaceDigit;
    //[SerializeField] Material clockDigitMaterial;

    Dictionary<Clock.TimeOfDay, UIInfo> timeUiInfo = new Dictionary<Clock.TimeOfDay, UIInfo>();
    public bool loadingScreenPlaying { get; private set; }

    int moneyInterp = 0; //Money that's being interpolated from
    uint moneySpeed; //Speed to animate getting the money
    uint moneyAccel, moneyTimer;

    UIInfo currentUISettings;
    UIAnimation currentTimeAnimation;
    const int FRAMES_PER_WEATHER_ANIM_FRAME = 15;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize clock parts and set them to animate independently
        /*hourDigit.Initialize(clockDigitMaterial);
        tensPlaceDigit.Initialize(clockDigitMaterial);
        onesPlaceDigit.Initialize(clockDigitMaterial);*/
        UpdateTimeOfDay();

        currentTimeAnimation = timeUiInfo[0].animation;
    }

    // Update is called once per frame
    void Update()
    {
        //Update the button UI //TODO: Start a coroutine with this on it?
        HandleMoneyAnimation();
        currentTimeAnimation.Update(); //Update the weather animation
    }

    public void Initialize()
    {
        loadingScreenPlaying = false;
        ResetMoneySpeed();

        uiCanvas.worldCamera = GameplayManager.mainCamera;
        uiCanvas.planeDistance = 3;

        //Populate dictionaries for later efficiency
        int i = 0;
        /*
        foreach (UIInfo info in weatherInfo)
        {
            weatherUiInfo[(Clock.Weather)i] = info;
            i++;
        }
        weatherInfo.Clear();*/

        i = 0;
        foreach (UIInfo info in timeInfo)
        {
            timeUiInfo[(Clock.TimeOfDay)i] = info;
            i++;
        }
        timeInfo.Clear();

        currentUISettings = timeUiInfo[Clock.TimeOfDay.DAWN];
        currentTimeAnimation = currentUISettings.animation;
        currentTimeAnimation.Initialize(timeOfDayIcon, FRAMES_PER_WEATHER_ANIM_FRAME); //Set frame rate for anim
        currentTimeAnimation.ResetAnimation();
    }

    #region Helper methods
    public void SetRenderCamera(Camera camera)
    {
		uiCanvas.worldCamera = camera;
    }
    #endregion

    #region Time of Day UI Methods
    public void UpdateMinute(uint minute)
    {
        //Update the text
        tensText.text = (minute / 10).ToString();
        onesText.text = (minute % 10).ToString();


        //Play ticking animations if needed
        /*
        if (minute % 10 == 0) StartCoroutine(tensPlaceDigit.PlayTickAnimation(minute / 10, 0.15f));
        StartCoroutine(onesPlaceDigit.PlayTickAnimation(minute % 10, 0.0f));*/
    }
    
    public void UpdateHourAndMinute(uint hour, uint minute)
    {
        //Update the hour
        hourText.text = hour.ToString();
        //StartCoroutine(hourDigit.PlayTickAnimation(hour, 0.3f));

        //Update the minute using the existing method
        UpdateMinute(minute);
    }

    //Updates the UI for the time of day
    public void UpdateTimeOfDay()
    { 
        currentUISettings = timeUiInfo[GameplayManager.clock.timeOfDay];
        UpdateWeatherAndTimeUI();
        UpdateHourAndMinute(GameplayManager.clock.time.hour, GameplayManager.clock.time.minute);
    }
    #endregion

    #region Weather UI Methods

    public void UpdateWeather()
    {
        //currentUISettings = weatherUiInfo[GameplayManager.clock.weather];
        UpdateWeatherAndTimeUI();
    }

    void UpdateWeatherAndTimeUI()
    {
        currentTimeAnimation = currentUISettings.animation; //Set animation based on UI Settings
        currentTimeAnimation.Initialize(timeOfDayIcon, FRAMES_PER_WEATHER_ANIM_FRAME); //Set frame rate for anim
        currentTimeAnimation.ResetAnimation(); //Start animation at frame 0
    }

    #endregion
    
    #region Loading Screen Methods

    public void SetLoadingScreenAMaterial(Material mat)
    {
        loadingScreenImage.material = mat;
        loadingScreenImage.material.SetFloat("_Size", 0.0f);
    }

    //Plays the loading screen in animation
    public IEnumerator LoadingScreenIn(float duration = 1.0f)
    {
        loadingScreen.SetActive(true); //Set the loading screen to appear
        loadingScreenPlaying = true; //Mark that the animation is playing

        //Find and set the center for the transition animation
        Vector3 screenPos = GameplayManager.mainCamera.WorldToScreenPoint(GameplayManager.player.transform.position);
        screenPos.x /= GameplayManager.mainCamera.pixelWidth;
        screenPos.y /= GameplayManager.mainCamera.pixelHeight;
        loadingScreenImage.material.SetFloat("_XPos", screenPos.x);
        loadingScreenImage.material.SetFloat("_YPos", screenPos.y);

        float length = 0.0f;
        while(length < duration)
        {
            length += Time.deltaTime;
            loadingScreenImage.material.SetFloat("_Size", 1.0f - (length / duration));
            yield return null;
        }
        loadingScreenPlaying = false; //Mark that the animation is done
        yield return null;
    }

    //Plays the loading screen out animation
    public IEnumerator LoadingScreenOut(float duration = 1.0f)
    {
        loadingScreenPlaying = true; //Mark that the animation is playing

        //Find and set the center for the transition animation
        Vector3 screenPos = GameplayManager.mainCamera.WorldToScreenPoint(GameplayManager.player.transform.position);
        screenPos.x /= GameplayManager.mainCamera.pixelWidth;
        screenPos.y /= GameplayManager.mainCamera.pixelHeight;
        loadingScreenImage.material.SetFloat("_XPos", screenPos.x);
        loadingScreenImage.material.SetFloat("_YPos", screenPos.y);

        float length = 0.0f;
        while (length < duration)
        {
            length += Time.deltaTime;
            loadingScreenImage.material.SetFloat("_Size", length / duration);
            yield return null;
        }

        loadingScreen.SetActive(false); //Set the loading screen to disappear
        loadingScreenPlaying = false; //Mark that the animation is done
        yield return null;
    }

    //Used to lock the loading screen as playing before calling the LoadingScreenIn as a Coroutine
    public void LockLoadingScreen() { loadingScreenPlaying = true; }
    public void ShowSavingIcon() { savingIcon.SetActive(true); }
    public void HideSavingIcon() { savingIcon.SetActive(false); }

    #endregion

    #region Money UI Methods

    int moneyDiff; //Used to figure out difference in money for animation
    void HandleMoneyAnimation()
    {
        //If the money animation has not yet reached
        if(moneyInterp != GameplayManager.saveData.money)
        {
            //Only bother interpolating every few frames
            if (moneyTimer % 6 == 0)
            {
                moneyDiff = (int)GameplayManager.saveData.money - (int)moneyInterp;

                if (moneyTimer % 72 == 0) //Update speed if animation is taking a while
                    moneySpeed += moneyAccel;
                if (moneyTimer % 186 == 0) //Update accel if animation is taking quite a while
                    moneyAccel++;

                if (moneySpeed * moneyTimer % 6 == 0)
                    GameplayManager.audioPlayer.PlaySFX("items/coin_tick");


                if (System.Math.Abs(moneyDiff) <= moneySpeed) //If the money would go past in the animation, set directly
                    moneyInterp = GameplayManager.saveData.money;
                else
                    moneyInterp = moneyInterp + (int)(System.Math.Sign(moneyDiff) * moneySpeed);

                moneyText.text = GetMoneyString();
            }

            moneyTimer++; //Update moneyTimer

            //If the target was reached, reset variables
            if (moneyInterp == GameplayManager.saveData.money)
                ResetMoneySpeed(); 
        }
    }

    void ResetMoneySpeed()
    {
        moneySpeed = 1;
        moneyAccel = 1;
        moneyTimer = 0;
    }

    public void SetMoneyValueImmediately(int money_interpolate)
    {
        moneyInterp = money_interpolate;
        moneyText.text = GetMoneyString();
    }

    string GetMoneyString()
    {
        const int digits = 5;
        char[] moneyChars = new char[digits];
        int tempInterp = moneyInterp;
        for(int i = digits - 1; i >= 0; i--)
        {
            moneyChars[i] = System.Convert.ToChar(48 + tempInterp % 10);
            tempInterp /= 10;
        }

        return new string(moneyChars);
        /*
        if (moneyInterp == 0)
            return "00000";

        string moneyString = "";
        int tempInterp = moneyInterp;
        //Find how many zeroes should be in the beginning of the string
        while(tempInterp < 10000)
        {
            moneyString += '0';
            tempInterp *= 10;
        }
        moneyString += moneyInterp;

        return moneyString;*/
    }

    #endregion

    #region Hiding and Showing Main UI

    //Hides the main UI
    public void HideUI()
    {
        uiCanvas.enabled = false;
    }

    //Shows the main UI
    public void ShowUI()
    {
        uiCanvas.enabled = true;
    }

    #endregion

    #region Other UI Methods

    public void ChangeActionText(string action_text)
    {
        actionText.text = action_text;
    }

    public void ChangeActionTextToNone()
    {
        actionText.text = "Nothing To Do";
    }

    const float ROOM_ANIM_TIME = 3.6f; //How long to keep the room name on screen
    public IEnumerator PlayRoomTitleAnimation(string room_name)
    {
        roomName.text = room_name;
        roomInfoParent.SetActive(true);

        roomInfoParent.transform.localScale = Vector3.zero; //Initialize scale to 0

        LTDescr ltd = LeanTween.scale(roomInfoParent, Vector3.one, ROOM_ANIM_TIME).setEase(roomAnimationCurve);
        ltd.setOnComplete(() => { roomInfoParent.SetActive(false); }); //Set to disable roomInfoParent when complete

        yield return null;
    }

    #endregion
}
