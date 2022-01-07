/*****************************************************************************************************************
 - Obstacle.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will record the current obstacle mesh that is displayed and will set a new one when needed.

 Available Attribues:

     - int Index = the index of the child mesh that is to be displayed.

     - GameObject ObstacleMesh = reference of the mesh that is currently active.

 Available Functions:

     * void SelectedObstacle(int obstacleIndex)
        - Selects an obstacle to be displayed, will randomly selected a mesh by default. Overriding mesh
          selection can be done by passing a valid index to this function. (Valid index is an index that not
          less than 0 or greater than the number of children that the obstacle has)
        - obstacleIndex = the index of the child mesh to be active. -1 (random) by default.

-----------------------------------------------------------------------------------------------------------------
 Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;

public class Obstacle : MovingEntity
{
    /// <summary>
    /// The obstacle mesh that is to be displayed. This is recorded as an int since it will be used as an index.
    /// The obstacle itself is a parent object that contains the deactivated child meshes. The index will be
    /// used to find the child obstacle meshes.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// Holds a reference to the obstacle mesh that is currently active. This can be used instead of using the
    /// ObstacleType index above and grabbing the child object.
    /// </summary>
    public GameObject ObstacleMesh { get; private set; }


    /// <summary>
    /// Sets the obstacle mesh to be displayed. 
    /// A number less 0 and greater than the number of available obstacles will have an obstacle type randomly selected.
    /// Otherwise, an obstacle will be displayed based on the provided index.
    /// </summary>
    /// 
    /// <param name="obstacleIndex"> The index of the child object mesh to be displayed </param>
    public void SelectObstacle(int obstacleIndex = -1)
    {
        if (ObstacleMesh != null)
        {
            ObstacleMesh.SetActive(false);
        }
        
        // set the index of the child object
        Index = (obstacleIndex <= -1 || obstacleIndex >= transform.childCount) ? Random.Range(0, transform.childCount) : obstacleIndex;

        // set the actual game object of the child object
        ObstacleMesh = transform.GetChild(Index).gameObject;
        ObstacleMesh.SetActive(true);
    }

    /// <summary>
    /// Trigger used to check if this obstacle is overlapping a ring or not. If it is overlapping a ring, then 
    /// respawn the obstacle until it is not overlapping the ring anymore. This will not check overlapping with
    /// other obstacles.
    /// </summary>
    /// 
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ring"))
        {
            Spawner.Instance.RespawnObstacle(this);
        }
    }
}
