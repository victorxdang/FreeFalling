/*****************************************************************************************************************
 - GameManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class handles menus, scores, scene transitions, etc. All background methods are handled here. There 
     will only be one instance of this class.
-----------------------------------------------------------------------------------------------------------------
Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Editable Region

    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    /// <summary>
    /// The unit of distance at which an object will move per second initially.
    /// This object speed itself will change based on how far the player has
    /// gotten in their game, but for starting off, it is recommended to be kept 
    /// at this speed.
    const float INITIAL_OBJECT_SPEED = 20;

    /// <summary>
    /// The amount to increase the speed of the platform by (not recommended to be 
    /// greater than 2 or 3). Also, recommendation applies to this constant is the 
    /// same as DISTANCE_TO_INCREASE when changing this value.
    /// </summary>
    const float OBJECT_SPEED_INCREASE = 0.5f;

    /// <summary>
    /// The distance (in units) the ball needs to travel in order to increase 
    /// speed.
    /// </summary>
    const float DISTANCE_TO_INCREASE = 100;

    /// <summary>
    /// The five colors in the game. The colors are representative of the points 
    /// it awards.
    /// The colors are in the order of magenta, cyan, yellow, orange, purple.
    /// </summary>
    public readonly Color[] Colors = { Color.magenta, Color.cyan, Color.yellow, new Color(1, 0.4980392f, 0), new Color(0.4980392f, 0, 1) };

    /// <summary>
    /// Parallel array to the Colors array.
    /// </summary>
    public readonly int[] Points = { 1, 3, 5, 7, 10 };


    // Debugging variables, these variables can be accessed through the editor to 
    // do real-time value changes.
    // 
    // Default values:
    // INVINCIBLE_BALL = false (will be automatically false when playing on phone)
    // ENABLE_GPGS = true (not currently implemented so the current value will be false)
    // AD_TEST_MODE = true (set to false when deploying game)

    /// <summary>
    /// Determines if the ball can go through objects or not. Used only for 
    /// debugging/playtesting purposes.
    /// </summary>
    public bool INVINCIBLE_BALL = false;

    /// <summary>
    /// Determines if Google Play Games Service is enabled or not. Used only
    /// for debugging/playtesting purposes.
    /// 
    /// NOTE: Not currently implemented.
    /// </summary>
    public bool ENABLE_GPGS = false;

    /// <summary>
    /// Sets the ads to either be test ads or live ads. Always use test ads
    /// during development phase!
    /// </summary>
    public bool AD_TEST_MODE = false;

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------

    #endregion


    #region Fields 

    /// <summary>
    /// The fastest an object should be able to travel. Any number higher than this
    /// may produce unexpected results and thus should be kept at a number 
    /// equal or lower than the max speed.
    /// 
    /// NOTE: this constant is not within the editable region because it is not
    /// editable!
    /// </summary>
    const float MAX_OBJECT_SPEED = 60;


    public static GameManager Instance { get; private set; }

    public bool GameStart { get; private set; }
    public bool GamePaused { get; private set; }
    public bool GameOver { get; private set; }
    
    public float ObjectSpeed { get; private set; }
    public float DistanceTravelled { get; private set; }

    // calls the methods subscribed to this event
    // remember to subscribe Calibration() in Ball in
    // order to call it
    public event System.Action Calibration; 

    #endregion


    #region Unity Built-In Functions

    /// <summary>
    /// Awake is called before start, whenever the object is created.
    /// </summary>
    void Awake()
    {
        // this class does not need to be persisted, but there can only be one instance of this class
        if (FindObjectsOfType<GameManager>().Length == 1)
        {
            Instance = this;
            GameStart = false;
            GameOver = false;
            GamePaused = false;

            ObjectSpeed = INITIAL_OBJECT_SPEED;
            DistanceTravelled = 0;

            SetDebuggingValues();
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
        // Start the game immediately whenever the game was restarted or an ad was watched.
        // Otherwise, display the banner ad and wait for the player to start the game.
        if (SaveManager.Instance.PlayerPersistentData.game_restarted || SaveManager.Instance.PlayerPersistentData.watched_videoad)
        {
            AdMobManager.Instance.DestroyBanner();
            UIManager.Instance.StartGame();

            if (SaveManager.Instance.PlayerPersistentData.game_restarted)
            {
                SaveManager.Instance.PlayerPersistentData.game_restarted = false;
            }
        }
        else
        {
            AdMobManager.Instance.ShowBannerAd();
        }
    }

    /// <summary>
    /// Called every frame.
    /// </summary>
    void Update()
    {
        // Keeps track of the distance travelled for the purpose of increase difficulty once the
        // player has acheived a certain distance.
        if (IsCurrentlyPlaying())
        {
            DistanceTravelled += ObjectSpeed * Time.deltaTime;

            // Increase the object speed once the player reached the targeted distance, but only 
            // increase the speed if the current speed of the object is less than the hardcap. 
            if (DistanceTravelled > DISTANCE_TO_INCREASE && ObjectSpeed < MAX_OBJECT_SPEED)
            {
                ObjectSpeed += OBJECT_SPEED_INCREASE;
                DistanceTravelled = 0;
            }
        }
    }

    #endregion


    #region Start Game

    /// <summary>
    /// Starts the game.
    /// </summary>
    public void StartGame()
    {
        System.GC.Collect();
        AdMobManager.Instance.DestroyBanner();
        GameStart = true;
    }

    #endregion


    #region Pause Game

    /// <summary>
    /// Pauses the game and displays a banner ad at the bottom of the screen.
    /// </summary>
    public void PauseGame()
    {
        AdMobManager.Instance.ShowBannerAd();
        GamePaused = true;
    }

    /// <summary>
    /// Unpauses the game.
    /// </summary>
    public void ResumeGame()
    {
        GamePaused = false;
        System.GC.Collect();
        AdMobManager.Instance.DestroyBanner();
    }

    /// <summary>
    /// Restarts the game and allows the player to watch an ad again when the game ends, if they had watched one previously.
    /// </summary>
    /// 
    /// <param name="adWatched"> Did the player watched an ad to continue their progress? </param>
    public void RestartGame(bool adWatched = false)
    {
        AdMobManager.Instance.CleanUp();
        System.GC.Collect();

        SaveManager.Instance.PlayerPersistentData.watched_videoad = adWatched;
        SaveManager.Instance.PlayerPersistentData.game_restarted = !adWatched;

        // reset the score to 0 if the player restarts the game, keep the score if the player watched a video ad
        if (!adWatched)
        {
            SaveManager.Instance.PlayerPersistentData.current_score = 0;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    #endregion


    #region Game End

    /// <summary>
    /// Start a delay before actually ending the game. Default value is 3 seconds.
    /// </summary>
    public void InvokeGameEnd()
    {
        GameOver = true;
        StartCoroutine(IEnumDelayGameEnd());
    }

    /// <summary>
    /// Delays the game for 3 seconds.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator IEnumDelayGameEnd()
    {
        yield return new WaitForSeconds(3);

        GameEnd();
    }

    /// <summary>
    /// End the game by determining if an intersitial ad should be displayed and saving the current score of the player to the cloud and locally 
    /// if they beat their previous high score.
    /// </summary>
    void GameEnd()
    {
        bool newRecord = false;

        UIManager.Instance.GameOverMenu();
        AdMobManager.Instance.ShowBannerAd();
        SaveManager.Instance.PlayerPersistentData.games_until_ad--;

        if (SaveManager.Instance.PlayerPersistentData.games_until_ad <= 0)
        {
            AdMobManager.Instance.ShowInterstitialAd();
            SaveManager.Instance.PlayerPersistentData.games_until_ad = 5;
        }

        // player has set a new record
        if (SaveManager.Instance.PlayerPersistentData.current_score > SaveManager.Instance.PlayerSaveData.best_score)
        {
            newRecord = true;

            // save the player's score locally
            SaveManager.Instance.PlayerSaveData.best_score = SaveManager.Instance.PlayerPersistentData.current_score;
        }

        UIManager.Instance.UpdateGameOverText(SaveManager.Instance.PlayerPersistentData.current_score, SaveManager.Instance.PlayerSaveData.best_score, newRecord);
        SaveManager.Instance.PlayerSaveData.first_time = false;

        // save data locally
        SaveManager.Instance.Save(SaveManager.Instance.PlayerSaveData, SaveManager.Instance.SavePath);

        System.GC.Collect();
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Used to calibrate the phone.
    /// </summary>
    public void CalibratePhone()
    {
        Calibration();
    }

    /// <summary>
    /// Checks if the game is currently in play and not paused or over.
    /// </summary>
    /// <returns></returns>
    public bool IsCurrentlyPlaying()
    {
        return GameStart && !GamePaused && !GameOver;
    }

    /// <summary>
    /// Automatically sets the correct debugging values when building and deploying the game.
    /// </summary>
    void SetDebuggingValues()
    {
        #if !UNITY_EDITOR
            INVINCIBLE_BALL = false;
        #endif
    }

    #endregion
}
