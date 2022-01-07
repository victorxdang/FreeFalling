/*****************************************************************************************************************
 - Ball.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class contains the ball, or the "main character" of the game. The player will be controlling this 
     entity throughout the game. Input of the game will depend whether the game is being played in the editor
     or when it is played on the phone. When the game is played in the editor, then use WASD when using tilt is
     selected and left mouse button when using drag. There can only be one ball on the map!
-----------------------------------------------------------------------------------------------------------------
 Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;

public class Ball : MonoBehaviour
{
    #region Editable Region

    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    /// <summary>
    /// How fast the ball will move (in units) when using the keyboard to play.
    /// </summary>
    const float DEBUG_SPEED = 15;

    /// <summary>
    /// How fast the ball will tilt (in units) when using the phone's accelerometer.
    /// </summary>
    const float TILT_SPEED = 30;

    /// <summary>
    /// How fast the ball will move when the player uses the joystick to move the 
    /// ball.
    /// </summary>
    const float JOYSTICK_SPEED = 10;

    /// <summary>
    /// How far and above the camera will be from the ball. DO NOT CHANGE VALUE!
    /// </summary>
    Vector3 CAMERA_POS_OFFSET = new Vector3(0, 10, -1);

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------

    #endregion


    #region Fields

    [SerializeField] Camera cam;
    [SerializeField] GameObject fracturedBall;
    [SerializeField] Joystick joystick;

    // cache variable
    MeshRenderer mesh;
    Matrix4x4 calibration;
    Vector3 tiltInput;

    #endregion


    #region Unity Built-In Functions

    /// <summary>
    /// Awake is called before start, whenever the object is created.
    /// </summary>
    void Awake()
    {
        // this class does not need to be persisted, but there can only be one instance of this class
        if (FindObjectsOfType<Ball>().Length == 1)
        {
            // initially set the mesh of the ball to be disabled
            mesh = GetComponent<MeshRenderer>();
            mesh.enabled = false;

            // pre-calibrate the phone, re-calibration requires restarting the game or going to the settings and
            // tapping the tilt button again.
            Calibrate();

            // subscribe Calibrate() to the GameManger's Calibration event so that Calibrate can be called whenever
            // GameManger calls its Calibration event
            GameManager.Instance.Calibration += Calibrate;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called every frame.
    /// </summary>
    void Update()
    {
        if (GameManager.Instance.IsCurrentlyPlaying())
        {
            // enable the ball's mesh when the game has started
            if (!mesh.enabled)
            {
                mesh.enabled = true;
            }

            // use the keyboard (WASD) and mouse input when in the editor and tilt and touch
            // when playing on the phone
            #if UNITY_EDITOR
                if (!SaveManager.Instance.PlayerSaveData.using_dpad)
                    ProcessKeyboardInput();
                else
                    ProcessJoystickInput();
            #else
                if (!SaveManager.Instance.PlayerSaveData.using_dpad)
                    ProcessTiltInput();
                else 
                    ProcessJoystickInput();
            #endif

            // keep the camera at a specified distance from the ball
            cam.transform.position = transform.position + CAMERA_POS_OFFSET;
        }
    }

    /// <summary>
    /// When the ball enters the trigger of the ring, add the points of the ring to the player's score.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ring"))
        {
            SaveManager.Instance.PlayerPersistentData.current_score += other.GetComponent<Ring>().PointsAwarded;
            UIManager.Instance.UpdateScoreText(SaveManager.Instance.PlayerPersistentData.current_score);
        }
    }

    /// <summary>
    /// Ends the game if the ball collides with the ring or obstacle.
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        if (!GameManager.Instance.GameOver && !GameManager.Instance.INVINCIBLE_BALL)
        {
            gameObject.SetActive(false);
            Instantiate(fracturedBall, transform.position, Quaternion.identity);
            GameManager.Instance.InvokeGameEnd();
        }
    }

    #endregion


    #region Tilt/Keyboard

    /// <summary>
    /// The input of the keyboard will be processed in this method when playing the game through the editor.
    /// Use WASD to move the ball.
    /// </summary>
    void ProcessKeyboardInput()
    {
        // move left and right
        if (Input.GetKey(KeyCode.A))
            transform.Translate(-DEBUG_SPEED * Time.deltaTime, 0, 0);
        else if (Input.GetKey(KeyCode.D))
            transform.Translate(DEBUG_SPEED * Time.deltaTime, 0, 0);

        // move forward and backward
        if (Input.GetKey(KeyCode.W))
            transform.Translate(0, 0, DEBUG_SPEED * Time.deltaTime);
        else if (Input.GetKey(KeyCode.S))
            transform.Translate(0, 0, -DEBUG_SPEED * Time.deltaTime);
    }

    /// <summary>
    /// The input of the gyroscope of the phone will be processed through this method.
    /// </summary>
    void ProcessTiltInput()
    {
        tiltInput = GetAccelerometer(Input.acceleration);
        transform.Translate(tiltInput.x * TILT_SPEED * Time.deltaTime, 0, tiltInput.y * TILT_SPEED * Time.deltaTime);
    }

    /// <summary>
    /// Calibrates the phone so that the current position that the player is holding the phone is zero.
    /// </summary>
    public void Calibrate()
    {
        Quaternion rotation = Quaternion.FromToRotation(new Vector3(0, 0, -1), Input.acceleration);
        calibration = Matrix4x4.TRS(Vector3.zero, rotation, new Vector3(1, 1, 1)).inverse;
    }

    /// <summary>
    /// Gets the updated input vector that takes the phone's position and angle from the most recent calibration into account.
    /// </summary>
    /// 
    /// <param name="input"></param>
    /// 
    /// <returns> The updated vector, taking into account the phone's position and angle. </returns>
    Vector3 GetAccelerometer(Vector3 input)
    {
        return calibration.MultiplyVector(input);
    }

    #endregion


    #region Joystick

    /// <summary>
    /// Uses the joystick as a way to move the ball so that a finger isn't in the way of the player's view (using touch/drag)
    /// and is simpler in terms of implementation and UI layout.
    /// </summary>
    void ProcessJoystickInput()
    {
        transform.Translate(joystick.Horizontal * JOYSTICK_SPEED * Time.deltaTime, 0, joystick.Vertical * JOYSTICK_SPEED * Time.deltaTime);
    }

    #endregion
}