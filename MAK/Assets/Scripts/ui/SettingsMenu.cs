using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoogalooGame;

public class SettingsMenu : MonoBehaviour, IUIElement
{
    [SerializeField] MenuOption[] settingsOptions;
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] Material fullscreenMaterial, sfxMaterial, musicMaterial;
    [SerializeField] TMPro.TMP_Text resolutionText, musicVolText, sfxVolText;
    bool selectingOption; //Whether the person is selecting the option to control
    bool control;
    int currentItem;
    Settings tempSettings;
    bool changed;

    const int RESOLUTION = 0;
    const int FULLSCREEN = 1;
    const int MUSIC_VOLUME = 2;
    const int SFX_VOLUME = 3;
    const int DEFAULT_SETTINGS = 4;

    const float SOUND_CHANGE = 0.1f; //How much the sound changes by hitting up or down

    // Start is called before the first frame update
    void Start()
    {
        currentItem = 0;
        selectingOption = true;
        changed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!control)
            return;

        if (selectingOption)
        {
            //Handle controls
            if (ControlManager.DownPressed())
            {
                StartCoroutine(settingsOptions[currentItem].UnhighlightedAnimation());
                currentItem++;
                currentItem %= settingsOptions.Length;
                StartCoroutine(settingsOptions[currentItem].HighlightedAnimation());
            }
            else if (ControlManager.UpPressed())
            {
                StartCoroutine(settingsOptions[currentItem].UnhighlightedAnimation());
                currentItem--;
                if (currentItem < 0) currentItem = settingsOptions.Length - 1;
                StartCoroutine(settingsOptions[currentItem].HighlightedAnimation());
            }
            else if (ControlManager.BackPressed()) //Close the menu if start is pressed
            {
                CloseMenu();
            }
            else if (ControlManager.AttackPressed() || ControlManager.JumpPressed()) //If an option is selected
            {
                selectingOption = false;
                changed = false;
            }
        }
        else //If an option is currently being selected
        {
            if (ControlManager.BackPressed()) //If back is pressed, go back to selecting an option
            {
                selectingOption = true;

                //If the settings were changed, revert
                if(changed)
                {
                    tempSettings = new Settings(GameplayManager.settings);
                    changed = false;
                }

                //TODO: Play SFX

                return;
            }
            else if(ControlManager.AttackPressed() || ControlManager.JumpPressed()) //If result is accepted
            {
                OverwriteSettings();
                //TODO: Play SFX
            }

            switch (currentItem)
            {
                case RESOLUTION: //Resolution
                    //Handle controls
                    if (ControlManager.DownPressed())
                    {
                        tempSettings.resolutionChoice++;
                        tempSettings.resolutionChoice %= settingsOptions.Length;
                        resolutionText.text = Screen.resolutions[tempSettings.resolutionChoice].ToString(); //Update UI
                        changed = true;
                    }
                    else if (ControlManager.UpPressed())
                    {
                        tempSettings.resolutionChoice--;
                        if (tempSettings.resolutionChoice < 0) tempSettings.resolutionChoice = Screen.resolutions.Length - 1;
                        resolutionText.text = Screen.resolutions[tempSettings.resolutionChoice].ToString(); //Update UI
                        changed = true;
                    }
                    break;
                case FULLSCREEN://Fullscreen
                    tempSettings.isFullscreen = !tempSettings.isFullscreen;
                    fullscreenMaterial.SetFloat("Percent", tempSettings.isFullscreen ? 1.0f : 0.0f);
                    OverwriteSettings();
                    //TODO: Play SFX
                    //TODO: Update check box for fullscreen
                    break;
                case MUSIC_VOLUME: //Music Volume
                    //TODO: Make UI for Settings
                    //Handle controls
                    if (ControlManager.LeftPressed())
                    {
                        tempSettings.musicVolume -= SOUND_CHANGE;
                        if (tempSettings.musicVolume < 0.0f) tempSettings.musicVolume = 0.0f;
                        GameplayManager.audioPlayer.SetMusicVolume(tempSettings.musicVolume); //Set volume so player can hear it
                        musicVolText.text = (int)(tempSettings.musicVolume * 100.0f) + "%";
                        musicMaterial.SetFloat("Percent", tempSettings.musicVolume);
                        //TODO: Update Material
                        changed = true;
                    }
                    else if (ControlManager.RightPressed())
                    {
                        tempSettings.musicVolume += SOUND_CHANGE;
                        if (tempSettings.musicVolume > 1.0f) tempSettings.musicVolume = 1.0f;
                        GameplayManager.audioPlayer.SetMusicVolume(tempSettings.musicVolume); //Set volume so player can hear it
                        musicVolText.text = (int)(tempSettings.sfxVolume * 100.0f) + "%";
                        musicMaterial.SetFloat("Percent", tempSettings.musicVolume);
                        //TODO: Update Material
                        changed = true;
                    }
                    break;
                case SFX_VOLUME: //SFX Volume
                    //Handle controls
                    if (ControlManager.LeftPressed())
                    {
                        tempSettings.sfxVolume -= SOUND_CHANGE;
                        if (tempSettings.sfxVolume < 0.0f) tempSettings.sfxVolume = 0.0f;
                        GameplayManager.audioPlayer.SetSFXVolume(tempSettings.sfxVolume); //Set volume so player can hear it
                        sfxMaterial.SetFloat("Percent", tempSettings.sfxVolume); //Update UI
                        sfxVolText.text = (int)(tempSettings.sfxVolume * 100.0f) + "%"; //Update text
                        //TODO: Play a sfx to test
                        changed = true;
                    }
                    else if (ControlManager.RightPressed())
                    {
                        tempSettings.sfxVolume += SOUND_CHANGE;
                        if (tempSettings.sfxVolume > 1.0f) tempSettings.sfxVolume = 1.0f;
                        GameplayManager.audioPlayer.SetSFXVolume(tempSettings.sfxVolume); //Set volume so player can hear it
                        sfxMaterial.SetFloat("Percent", tempSettings.sfxVolume); //Update UI
                        sfxVolText.text = (int)(tempSettings.sfxVolume * 100.0f) + "%"; //Update text
                        //TODO: Play a sfx to test

                        changed = true;
                    }
                    break;
                case DEFAULT_SETTINGS: //Revert default settings
                    tempSettings = new Settings();
                    GameplayManager.SetSettings(tempSettings);
                    GameplayManager.gameplayManager.ApplySettings();
                    break;
            }
        }
        
    }

    public void OpenMenu()
    {
        StartCoroutine(EnterAnimation());
        tempSettings = new Settings(GameplayManager.settings); //Clone existing settings
        SetSettingsUI();   
    }

    void CloseMenu()
    {
        GameplayManager.SetSettings(tempSettings); //Overwrite old settings
        GameplayManager.gameplayManager.ApplySettings(); //Apply the settings
        StartCoroutine(CloseAnimation());
    }

    void OverwriteSettings()
    {
        GameplayManager.SetSettings(tempSettings); //Overwrite old settings
        GameplayManager.gameplayManager.ApplySettings(); //Apply the settings
        changed = false;
    }

    //Initializes the settings UI and strings
    void SetSettingsUI()
    {
        //Set strings
        musicVolText.text = (int)(tempSettings.musicVolume * 100.0f) + "%";
        sfxVolText.text = (int)(tempSettings.sfxVolume * 100.0f) + "%";
        resolutionText.text = resolutionText.text = Screen.resolutions[tempSettings.resolutionChoice].ToString();

        //Set materials
        musicMaterial.SetFloat("Percent", tempSettings.musicVolume);
        sfxMaterial.SetFloat("Percent", tempSettings.sfxVolume);
        fullscreenMaterial.SetFloat("Percent", tempSettings.isFullscreen ? 0.9f : 0.12f);
    }

    #region UI Element Methods
    public IEnumerator CloseAnimation()
    {
        control = false;
        pauseMenu.SetActive();
        yield return null;
    }

    public IEnumerator EnterAnimation()
    {

        StartCoroutine(IdleAnimation()); //After done, go to the idle animation
        pauseMenu.SetInactive(); //Disable the pause menu after the animation is done
        control = true;
        yield return null;
    }

    public IEnumerator IdleAnimation()
    {
        yield return null;
    }
    #endregion
}
