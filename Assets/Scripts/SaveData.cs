/*****************************************************************************************************************
 - SaveInfo.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will hold the settings of the user. This data will be saved to the local drive of the device.
     This class will only hold just purely data and nothing else. Add more variables/data as needed. Handled
     by the SaveManager class.
-----------------------------------------------------------------------------------------------------------------
Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

public class SaveData
{
    /// <summary>
    /// Determines if the player is playing the game for the first time. Not currently used for anything as of
    /// now but will still be here just in case it is needed in a future implementation.
    /// </summary>
    public bool first_time = false;

    /// <summary>
    /// If true, then drag is enabled, if not, then tilt is enabled. Tilt is
    /// enabled by default.
    /// </summary>
    public bool using_dpad = false;

    /// <summary>
    /// The player's highest score that is saved to the device.
    /// </summary>
    public int best_score = 0;
}
