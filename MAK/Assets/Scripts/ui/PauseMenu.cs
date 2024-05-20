using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour, IUIElement
{
    [SerializeField] MenuOption[] menuOptions;
    [SerializeField] SettingsMenu settingsMenu;
    int currentItem; //Item currently selected
    bool control; //Can the menu be controlled?

    // Start is called before the first frame update
    void Start()
    {
        currentItem = 0;
        control = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!control)
            return;

        //Handle controls
        if (ControlManager.DownPressed())
        {
            StartCoroutine(menuOptions[currentItem].UnhighlightedAnimation());
            currentItem++;
            currentItem %= menuOptions.Length;
            StartCoroutine(menuOptions[currentItem].HighlightedAnimation());
        }
        else if (ControlManager.UpPressed())
        {
            StartCoroutine(menuOptions[currentItem].UnhighlightedAnimation());
            currentItem--;
            if (currentItem < 0) currentItem = menuOptions.Length - 1;
            StartCoroutine(menuOptions[currentItem].HighlightedAnimation());
        }
        else if (ControlManager.StartPressed()) //Close the menu if start is pressed
        {
            ExitMenu();
        }
        else if (ControlManager.AttackPressed() || ControlManager.JumpPressed()) //If an option is selected
        {
            switch (currentItem)
            {
                case 0: //Close menu
                    ExitMenu();
                    break;
                case 1://Soul Log
                       //TODO: Make UI for Soul Log
                    break;
                case 2: //Settings
                    control = false;
                    settingsMenu.gameObject.SetActive(true);
                    settingsMenu.OpenMenu();
                    break;
                case 3: //Go back to title screen
                        //TODO: Go back to a title screen menu and display info about saving
                    break;
            }
        }
    }

    #region Opening and Closing the Menu
    public void OpenMenu()
    {
        control = true;
        StartCoroutine(EnterAnimation());
        StartCoroutine(menuOptions[currentItem].HighlightedAnimation()); //Highlight the selected option
    }

    void ExitMenu() {
        control = false;
        this.gameObject.SetActive(false);
        StartCoroutine(menuOptions[currentItem].UnhighlightedAnimation());
        StartCoroutine(CloseAnimation());
    }

    public void EnableControl() { control = true; }

    public void SetInactive() { gameObject.SetActive(false); }
    public void SetActive() { gameObject.SetActive(true); control = true; }
    #endregion

    #region UI Element Methods
    public IEnumerator CloseAnimation()
    {
        yield return null;
    }

    public IEnumerator EnterAnimation()
    {
        control = true;

        StartCoroutine(IdleAnimation()); //After done, go to the idle animation
        yield return null;
    }

    public IEnumerator IdleAnimation()
    {
        yield return null;
    }
    #endregion
}
