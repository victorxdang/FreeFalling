/*****************************************************************************************************************
 - UIManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class handles the UI's of the game. All logic involving buttons, text outputs, transitions, anything
     invloving the UI's will happen here. There will only be one instance of this class.
-----------------------------------------------------------------------------------------------------------------
Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Editable Region

    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    /// <summary>
    /// The amount of delay (in seconds) before resuming the game. Default 
    /// value is 3 seconds.
    /// </summary>
    const float RESUME_DELAY = 3;

    /// <summary>
    /// The furthest right the UI can move to, based on the animation.
    /// </summary>
    const float UI_MAX_X = 90;

    /// <summary>
    /// The name of the animation that will play the settings bar anim.
    /// </summary>
    const string SETTING_BAR_ANIM_NAME = "SettingsBarAnim";
    
    /// <summary>
    /// The name of the animation that will play the tilt button anim.
    /// </summary>
    const string TILT_BUTTON_ANIM_NAME = "TiltButtonAnim";

    /// <summary>
    /// The name of the animation that will play the D-Pad button anim.
    /// </summary>
    const string DPAD_BUTTON_ANIM_NAME = "DPadButtonAnim";

    /// <summary>
    /// The name of the animation that will play the credits button anim.
    /// </summary>
    const string CREDITS_BUTTON_ANIM_NAME = "CreditsButtonAnim";

    /// <summary>
    /// The name of the animation that will play the help button anim.
    /// </summary>
    const string HELP_BUTTON_ANIM_NAME = "HelpButtonAnim";

    /// <summary>
    /// The name of the animation for the credits text anim.
    /// </summary>
    const string CREDITS_TEXT_ANIM_NAME = "CreditsTextAnim";

    /// <summary>
    /// The name of the animation for the credits text anim.
    /// </summary>
    const string HELP_TEXT_ANIM_NAME = "HelpTextAnim";

    /// <summary>
    /// The name of the animation for the pause menu.
    /// </summary>
    const string PAUSE_BUTTON_ANIM = "PauseButtonAnim";

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------

    #endregion


    #region Fields

    public static UIManager Instance { get; private set; }

    // menus
    [SerializeField] GameObject menuStart,
                                menuSettings,
                                menuInGame,
                                menuPause,
                                menuGameOver,
                                menuCalibration,
                                menuCredits,
                                menuHelp;

    // buttons
    [SerializeField] Button buttonStart,
                            buttonSettings,
                            buttonDPad,
                            buttonTilt,
                            buttonCredits,
                            buttonHelp,
                            buttonPause,
                            buttonResume,
                            buttonPauseRestart,
                            buttonWatchAd,
                            buttonGameOverRestart,
                            buttonExitCalibration,
                            buttonSappheiros,

                            buttonLeaderboards, //
                            buttonAchievements, // these three are implemented but not active
                            buttonGoggleSignIn; // 

    // texts
    [SerializeField] Text textScore,
                          textResume, 
                          textGameOver,
                          textTapAnywhere,
                          textCalibration;

    // button holder
    [SerializeField] GameObject buttonHolderSettings,
                                buttonHolderPause;

    // text holders
    [SerializeField] GameObject textDragSelected,
                                textTiltSelected,
                                textCreditsSelected,
                                textHelpSelected,
                                textHolderCredits,
                                textHolderHelp;

    [SerializeField] GameObject joystick;

    // cached variables
    bool creditsOpen = false,
         helpOpen = false,
         calibrateFromStartMenu,
         requiresCalibration;

    float start,
          end;

    ColorBlock colors;
    WaitForSeconds wait;

    #endregion


    #region Unity Built-In Functions

    /// <summary>
    /// Awake is called before start, whenever the object is created.
    /// </summary>
    void Awake()
    {
        // this class does not need to be persisted, but there can only be one instance of this class
        if (FindObjectsOfType<UIManager>().Length == 1)
        {
            Instance = this;

            // set the texts to the appropriate string
            textResume.text = "";
            textTapAnywhere.text = "";
            textCalibration.text = "";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when the game object is enabled. 
    /// </summary>
    void Start()
    {
        // update the score text
        textScore.text = SaveManager.Instance.PlayerPersistentData.current_score.ToString();

        // updates the drag or tilt button to highlight which setting is currently active
        UpdateSwipeOrTilt(((SaveManager.Instance.PlayerSaveData.using_dpad) ? buttonDPad : buttonTilt), true);

        SetupButtons();
        SetupMenus();
    }

    #endregion


    #region Start/In-game Menu

    /// <summary>
    /// Starts the game after the Start Game button has been pressed. This method also ensures that the 
    /// settings bar's y-scale is 0 so that the animation can run smoothly the next the settings bar is
    /// opened.
    /// </summary>
    public void StartGame()
    {
        // ensures that the settings bar's y-scale is 0 so that there will not be any odd animations when the settings bar is open again
        buttonHolderSettings.GetComponent<RectTransform>().localScale = new Vector3(1, 0, 1);

        // if the player is using tilt, calibrate the phone, otherwise, start the game
        if (!SaveManager.Instance.PlayerSaveData.using_dpad && !SaveManager.Instance.PlayerPersistentData.game_restarted && !SaveManager.Instance.PlayerPersistentData.watched_videoad)
        {
            CalibartionMenu(true, menuStart, buttonSettings.gameObject);
        }
        else
        {
            InGameMenu(true, menuStart, buttonSettings.gameObject);
            GameManager.Instance.StartGame();
        }
    }

    /// <summary>
    /// Displays the main game UI once the player has started the game or when the player is resuming
    /// from a pause. This UI does fade in, but it will only do so after pressing the Start Game button 
    /// or if the game has restarted. There will be no fade otherwise.
    /// 
    /// This method can also take mulitple menus to be deactivated. Add as many menus needed to the method's
    /// parameter list to be deactivated.
    /// </summary>
    /// 
    /// <param name="fade"> Should the menu fade? Fade is usually called after the start game buttons is pressed. </param>
    /// <param name="currentMenus"> The menus to be deactivated. </param>
    void InGameMenu(bool fade, params GameObject[] currentMenus)
    {
        foreach (GameObject menu in currentMenus)
        {
            menu.SetActive(false);
        }

        menuInGame.SetActive(true);

        if (fade)
        {
            StartCoroutine(IEnumMenuFadeIn(true, menuInGame.GetComponent<CanvasGroup>()));
        }
    }

    /// <summary>
    /// Used to make the in-game UI fade in once the game starts.
    /// </summary>
    /// 
    /// <param name="fadeIn"> Should the UI fade in? </param>
    /// <param name="cg"> The canvas group that is attached to the menu. </param>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumMenuFadeIn(bool fadeIn, CanvasGroup cg)
    {
        float time = 0;
        start = (fadeIn) ? 0 : 1;
        end = (fadeIn) ? 1 : 0;

        while (time <= 1.25f)
        {
            cg.alpha = Mathf.Lerp(start, end, time);
            time += Time.deltaTime;
            yield return null;
        }
    }

    #endregion


    #region Calibration Menu

    /// <summary>
    /// Opens up the calibration menu.
    /// </summary>
    /// 
    /// <param name="fromStart"> Is this function being called from the start menu? This is needed for FinishCalibration(). </param>
    /// <param name="menus"> The menus to be deactivated. </param>
    void CalibartionMenu(bool fromStart, params GameObject[] menus)
    {
        if (menus.Length > 0)
        {
            foreach (GameObject m in menus)
            {
                m.SetActive(false);
            }
        }

        textCalibration.text = "Calibrating\n\nHold phone in desired position";
        calibrateFromStartMenu = fromStart;
        menuCalibration.SetActive(true);
        buttonExitCalibration.interactable = false;

        StartCoroutine(IEnumWaitForCalibration());
    }

    /// <summary>
    /// Finializes calibration by switching over to in-game menu. This method is called by clicking buttonExitCalibation.
    /// </summary>
    /// 
    /// <param name="fromStart"> 
    /// Is this function called from the start menu? If so, then go straight into the game, 
    /// otherwise (called from pause menu), display the resume timer. 
    /// </param>
    void FinishCalibration(bool fromStart)
    {
        textTapAnywhere.text = "";
        textCalibration.text = "";
        textTapAnywhere.gameObject.SetActive(false);
        InGameMenu(fromStart, menuCalibration);

        // actual calibration of the phone is called through this method
        GameManager.Instance.CalibratePhone();

        if (fromStart)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            textScore.text = "";
            StartCoroutine(IEnumResumeTimer());
        }
    }

    /// <summary>
    /// Waits a moment before displaying "tap anywhere when ready text".
    /// </summary>
    /// 
    /// <returns> Yield for 3 seconds before finishing calibration. </returns>
    System.Collections.IEnumerator IEnumWaitForCalibration()
    {
        yield return new WaitForSeconds(3);

        textTapAnywhere.gameObject.SetActive(true);
        textCalibration.text = "Calibartion complete\n\nTo re-calibrate, tap the Tilt button in the Settings drop-down menu";
        textTapAnywhere.text = "Tap anywhere to start";
        buttonExitCalibration.interactable = true;
    }

    #endregion


    #region Pause/Settings Menu

    /// <summary>
    /// Opens up the pause menu. Will also move the pause menu into place.
    /// </summary>
    /// 
    /// <param name="currentMenu"> The menu that this function is being called from. </param>
    void PauseMenu()
    {
        menuInGame.SetActive(false);
        menuPause.SetActive(true);
        buttonSettings.gameObject.SetActive(true);
        PlayAnimation(true, PAUSE_BUTTON_ANIM, buttonHolderPause.GetComponent<Animation>());

        GameManager.Instance.PauseGame();
    }

    /// <summary>
    /// Resumes the game. Will also activate the countdown timer so that the player can be ready
    /// to play again. This function will close the settings bar if it is still open and the player
    /// presses resume game.
    /// </summary>
    void ResumeGame()
    {
        joystick.SetActive(false);
        buttonPause.gameObject.SetActive(false);
        buttonHolderSettings.GetComponent<RectTransform>().localScale = new Vector3(1, 0, 1);
        buttonSettings.gameObject.SetActive(false);
        menuInGame.SetActive(true);

        // play the menu transition animation
        PlayAnimation(false, PAUSE_BUTTON_ANIM, buttonHolderPause.GetComponent<Animation>());

        // bring up the calibration menu if re-calibration is required
        if (requiresCalibration)
        {
            textScore.text = "";
            CalibartionMenu(false);
        }
        else
        {
            StartCoroutine(IEnumResumeTimer());
        }
    }

    /// <summary>
    /// Opens up the settings menu.
    /// </summary>
    void SettingsMenu()
    {
        PlayAnimation(buttonHolderSettings.GetComponent<RectTransform>().localScale.y == 0, SETTING_BAR_ANIM_NAME, buttonHolderSettings.GetComponent<Animation>());
    }

    /// <summary>
    /// Opens up the credits menu.
    /// </summary>
    void CreditsMenu()
    {
        creditsOpen = !creditsOpen;

        if (textHolderHelp.GetComponent<RectTransform>().anchoredPosition.x > -UI_MAX_X)
        {
            helpOpen = false;
            PlayAnimation(false, HELP_TEXT_ANIM_NAME, textHolderHelp.GetComponent<Animation>());
            UpdateButtonColor(false, buttonHelp);
        }
        else if (!GameManager.Instance.GameStart && menuStart.activeSelf)
        {
            menuStart.SetActive(false);
        }
        else if (buttonHolderPause.GetComponent<RectTransform>().anchoredPosition.x > -UI_MAX_X)
        {
            PlayAnimation(false, PAUSE_BUTTON_ANIM, buttonHolderPause.GetComponent<Animation>());
        }

        // highlight the credits button if the credits menu is active
        UpdateButtonColor(textHolderCredits.GetComponent<RectTransform>().anchoredPosition.x < UI_MAX_X, buttonCredits);
        PlayAnimation(true, CREDITS_BUTTON_ANIM_NAME, textCreditsSelected.GetComponent<Animation>());
        PlayAnimation(textHolderCredits.GetComponent<RectTransform>().anchoredPosition.x < UI_MAX_X, CREDITS_TEXT_ANIM_NAME, textHolderCredits.GetComponent<Animation>());

        // if the game has not started yet, hide the start menu so that the credits menu can appear,
        // otherwise, if the game has started, then hide the pause menu so that the credits menu can appear
        if (!helpOpen && !creditsOpen)
        {
            if (!GameManager.Instance.GameStart)
            {
                menuStart.SetActive(true);
            }
            else
            {
                PlayAnimation(true, PAUSE_BUTTON_ANIM, buttonHolderPause.GetComponent<Animation>());
            }
        }
    }

    /// <summary>
    /// Opens up the help menu.
    /// </summary>
    void HelpMenu()
    {
        helpOpen = !helpOpen;

        if (textHolderCredits.GetComponent<RectTransform>().anchoredPosition.x > -UI_MAX_X)
        {
            creditsOpen = false;
            PlayAnimation(false, CREDITS_TEXT_ANIM_NAME, textHolderCredits.GetComponent<Animation>());
            UpdateButtonColor(false, buttonCredits);
        }
        else if (!GameManager.Instance.GameStart && menuStart.activeSelf)
        {
            menuStart.SetActive(false);
        }
        else if (buttonHolderPause.GetComponent<RectTransform>().anchoredPosition.x > -UI_MAX_X)
        {
            PlayAnimation(false, PAUSE_BUTTON_ANIM, buttonHolderPause.GetComponent<Animation>());
        }

        // highlight the credits button if the credits menu is active
        UpdateButtonColor(textHolderHelp.GetComponent<RectTransform>().anchoredPosition.x < UI_MAX_X, buttonHelp);
        PlayAnimation(true, HELP_BUTTON_ANIM_NAME, textHelpSelected.GetComponent<Animation>());
        PlayAnimation(textHolderHelp.GetComponent<RectTransform>().anchoredPosition.x < UI_MAX_X, HELP_TEXT_ANIM_NAME, textHolderHelp.GetComponent<Animation>());

        // if the game has not started yet, hide the start menu so that the credits menu can appear,
        // otherwise, if the game has started, then hide the pause menu so that the credits menu can appear
        if (!helpOpen && !creditsOpen)
        {
            if (!GameManager.Instance.GameStart)
            {
                menuStart.SetActive(true);
            }
            else
            {
                PlayAnimation(true, PAUSE_BUTTON_ANIM, buttonHolderPause.GetComponent<Animation>());
            }
        }
    }

    /// <summary>
    /// Changes the colors of the Tilt and Drag buttons based on the current setting.
    /// If tilt is active, then the Tilt button will be highlighted, signifying that it is the current
    /// setting selected. Likewise for D-Pad, if it is selected.
    /// </summary>
    /// 
    /// <param name="button"> The button that was pressed (either drag or tilt button). </param>
    /// <param name="initializing"> Is this function being called from the Start() function? </param>
    void UpdateSwipeOrTilt(Button button, bool initializing = false)
    {
        SaveManager.Instance.PlayerSaveData.using_dpad = (button == buttonDPad);

        // turn the joystick on or off appropriately
        joystick.SetActive(button == buttonDPad);

        // update colors for swipe, if currently in use
        UpdateButtonColor(button == buttonDPad, buttonDPad);

        // update colors for tilt, if currently in use
        UpdateButtonColor(button != buttonDPad, buttonTilt);

        // plays the appropriate animation based on the current selected setting
        if (!initializing)
        {
            if (SaveManager.Instance.PlayerSaveData.using_dpad) // D-Pad
            {
                // don't do any calibration if the player has decided to press the tilt button and then change back
                // to drag
                requiresCalibration = false;

                PlayAnimation(true, DPAD_BUTTON_ANIM_NAME, textDragSelected.GetComponent<Animation>());
            }
            else // tilt
            {
                // calibrate the phone again when the player goes back to tilt
                requiresCalibration = true;

                PlayAnimation(true, TILT_BUTTON_ANIM_NAME, textTiltSelected.GetComponent<Animation>());
            }
        }
    }

    /// <summary>
    /// A timer to count down the number of seconds left before getting back into the game. Default value
    /// is 3 seconds.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumResumeTimer()
    {
        float time = RESUME_DELAY;
        wait = new WaitForSeconds(1);
        textScore.text = "";

        // wait until the pause menu's animation is complete
        while (buttonHolderPause.GetComponent<Animation>().isPlaying)
        {
            yield return null;
        }

        menuPause.SetActive(false);

        while (time > 0)
        {
            textResume.text = "Resuming In:\n" + time.ToString();
            time--;
            yield return wait;
        }

        textResume.text = "";
        buttonPause.gameObject.SetActive(true);

        if (SaveManager.Instance.PlayerSaveData.using_dpad)
        {
            joystick.SetActive(true);
        }

        UpdateScoreText();
        GameManager.Instance.ResumeGame();
    }

    #endregion


    #region Game Over Menu

    /// <summary>
    /// Activates the game over menu and deactivates all other potential menus that could still pop up even
    /// when the game is over (in-game, pause and settings button).
    /// </summary>
    public void GameOverMenu()
    {
        // hides all other menus that could potentially pop-up even when the game is over (pause, settings, in-game menus) and
        // then displays the game over menu
        menuInGame.SetActive(false);
        menuPause.SetActive(false);
        buttonSettings.gameObject.SetActive(false);
        menuGameOver.SetActive(true);

        // deactivate the watch ad button if the player has previously watched an ad
        buttonWatchAd.interactable = !SaveManager.Instance.PlayerPersistentData.watched_videoad && AdMobManager.Instance.RewardedAdLoaded();
    }

    /// <summary>
    /// Displays a video ad so that the player can continue their progress once the ad is finished.
    /// </summary>
    void ShowVideoAd()
    {
        AdMobManager.Instance.ShowRewardedVideoAd();

        // only call this coroutine if the ad is actually loaded and played
        if (AdMobManager.Instance.RewardedAdLoaded())
        {
            StartCoroutine(IEnumCheckIfRewardedAdLoaded());
        }
    }

    /// <summary>
    /// Delays the game a bit before restarting the game.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumCheckIfRewardedAdLoaded()
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.RestartGame(true);
    }

    /// <summary>
    /// Restarts the game. Also allows the player to watch a video ad again if they watched one previously to continue their progress.
    /// </summary>
    void RestartGame()
    {
        GameManager.Instance.RestartGame();
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Used to update the player's current score that is displayed on screen.
    /// </summary>
    /// 
    /// <param name="score"> The score to be displayed on the in-game menu score text. </param>
    public void UpdateScoreText(int score = -1)
    {
        textScore.text = ((score < 0) ? SaveManager.Instance.PlayerPersistentData.current_score : score).ToString();
    }

    /// <summary>
    /// Displays the player's current score and their best score onto the screen when the game is over. Will also display
    /// if the player has set a new record.
    /// </summary>
    /// 
    /// <param name="score"> The final score of this gaming session. </param>
    /// <param name="record"> The player's highest score that is saved to their device. </param>
    /// <param name="newRecord"> Did the player set a new record? </param>
    public void UpdateGameOverText(int score, int record, bool newRecord = false)
    {
        textGameOver.text = (newRecord) ? "NEW RECORD!\n" + score.ToString() : "Score:\n" + score.ToString() + "\nBest:\n" + record;
    }

    /// <summary>
    /// Plays the specified animation. Animations can be played in reverse by setting playNormal to false.
    /// </summary>
    /// 
    /// <param name="playNormal"> Should the animation play normally? Set to false to play animation in reverse. </param>
    /// <param name="animationName"> The name of the animation to be played. </param>
    /// <param name="animation"> The animation component attached to the game object. </param>
    bool PlayAnimation(bool playNormal, string animationName, Animation animation)
    {
        animation[animationName].speed = (playNormal) ? 1 : -1;

        // set the starting time of the animation to the length of the animation if the animation is to be played in reverse
        if (animation[animationName].speed == -1)
        {
            animation[animationName].time = animation[animationName].length;
        }

        return animation.Play();
    }

    /// <summary>
    /// Updates the color of the button based on the condition provided.
    /// If the condition is true, then the button will turn yellow, otherwise
    /// it will be white.
    /// </summary>
    /// 
    /// <param name="condition"> The boolean expression that will determine the color of the button. </param>
    /// <param name="button"> The button to update the colors of. </param>
    void UpdateButtonColor(bool condition, Button button)
    {
        colors = button.colors;
        colors.normalColor = (condition) ? Color.yellow : Color.white;
        colors.highlightedColor = colors.normalColor;
        button.colors = colors;
    }

    /// <summary>
    /// Sets up the buttons to have them function correctly. This can be done through the editor, but
    /// I like this way better.
    /// </summary>
    void SetupButtons()
    {
        // buttons for start menu
        buttonStart.onClick.AddListener(StartGame);

        // buttons for settings menu
        buttonDPad.onClick.AddListener(delegate { UpdateSwipeOrTilt(buttonDPad); });
        buttonTilt.onClick.AddListener(delegate { UpdateSwipeOrTilt(buttonTilt); });
        buttonCredits.onClick.AddListener(CreditsMenu);
        buttonSettings.onClick.AddListener(SettingsMenu);
        buttonHelp.onClick.AddListener(HelpMenu);
        buttonExitCalibration.onClick.AddListener(delegate { FinishCalibration(calibrateFromStartMenu); });
        buttonSappheiros.onClick.AddListener(delegate { Application.OpenURL("https://soundcloud.com/sappheirosmusic"); });

        // buttons for in-game menu
        buttonPause.onClick.AddListener(PauseMenu);

        // buttons for pause menu
        buttonResume.onClick.AddListener(ResumeGame);
        buttonPauseRestart.onClick.AddListener(RestartGame);

        // buttons for game over menu
        buttonWatchAd.onClick.AddListener(ShowVideoAd);
        buttonGameOverRestart.onClick.AddListener(RestartGame);

        buttonSettings.gameObject.SetActive(!SaveManager.Instance.PlayerPersistentData.game_restarted);
        buttonExitCalibration.interactable = false;
    }

    /// <summary>
    /// Configures the menus so that the appropriate menu displays at the appropriate time.
    /// </summary>
    void SetupMenus()
    {
        menuStart.SetActive(!SaveManager.Instance.PlayerPersistentData.game_restarted);
        menuInGame.SetActive(SaveManager.Instance.PlayerPersistentData.game_restarted);
        menuGameOver.SetActive(false);
        menuCalibration.SetActive(false);
        menuPause.SetActive(false);
        menuCredits.SetActive(true);  
        menuSettings.SetActive(true);
        menuHelp.SetActive(true);     

        // deactivate the settings bar and make sure its scale is 0 so that there won't be
        // any animation glitches.
        buttonHolderSettings.GetComponent<RectTransform>().localScale = new Vector3(1, 0, 1);
        textTapAnywhere.gameObject.SetActive(false);
        textHolderCredits.SetActive(true);
        textHolderHelp.SetActive(true);
    }

    #endregion
}
