/*****************************************************************************************************************
 - MovingEntity.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Parent class for all objects that are to be moved in game (except the ball, even if the ball is ever to be 
     moved). This class handles only the movement of the objects.
-----------------------------------------------------------------------------------------------------------------
 Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;

public class MovingEntity : MonoBehaviour
{
    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    /// <summary>
    /// The y-coordinate where all objects will despawn. Default value is 60.
    /// </summary>
    const float DESPAWN_Y = 60;

    /// <summary>
    /// The y-coordinate where all objects will spawn (or be set active).
    /// Default value is -30.
    /// </summary>
    const float SPAWN_Y = -60;

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------


    Vector3 newPos; // the new position of this object


    /// <summary>
    /// Called every frame.
    /// </summary>
    void Update ()
    {
        // only update the position of the object if the game has started, is not paused. The objects will still
        // move even when the game is over to continue the theme of free falling.
        // NOTE: objects will only move upwards along the y-axis.
        if (GameManager.Instance.GameStart && !GameManager.Instance.GamePaused)
        {
            newPos = transform.position;
            newPos.y += GameManager.Instance.ObjectSpeed * Time.deltaTime;
            transform.position = newPos;
        }

        // once the objects reaches the despawn point, call the appropriate respawn method.
        if (transform.position.y > DESPAWN_Y)
        {
            if (CompareTag("Ring"))
            {
                Spawner.Instance.RespawnRing(GetComponent<Ring>());
            }
            else if (CompareTag("Obstacle"))
            {
                Spawner.Instance.RespawnObstacle(GetComponent<Obstacle>());
            }
        }
	}
}
