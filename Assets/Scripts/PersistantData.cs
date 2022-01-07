/*****************************************************************************************************************
 - PersistentData.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class holds the variables needed to persist between transitions in games. Data in this class will 
     not be saved to the device and will be handled by the SaveManager class.
-----------------------------------------------------------------------------------------------------------------
Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

public class PersistantData
{
    /// <summary>
    /// Determines if the player has watched a rewarded video ad or not.
    /// </summary>
    public bool watched_videoad = false;

    /// <summary>
    /// Determines if the player has restarted the game, either through the pause or game
    /// over menu.
    /// </summary>
    public bool game_restarted = false;

    /// <summary>
    /// Used to keep track of how many consecutive games the player has played. 
    /// Display an interstital ad after 5 games.
    /// </summary>
    public int games_until_ad = 5;

    /// <summary>
    /// Keeps track of the player's current score. Will be resetted when the game restarts. Will
    /// not be resetted if the player watched a video ad.
    /// </summary>
    public int current_score = 0;
}
