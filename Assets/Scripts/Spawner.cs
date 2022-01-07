/*****************************************************************************************************************
 - Spawner.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class spawns the objects (obstacle and ring). The respawn methods in this class will attempt to make
     sure that there will be no (or at least minimal) overlapping of the objects. There will only be one 
     instance of this class.
-----------------------------------------------------------------------------------------------------------------
Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;

public class Spawner : MonoBehaviour
{
    #region Editable Region

    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    /// <summary>
    /// The max amount of rings that can spawn with a given set. This is 
    /// practically the amount of rings that can appear on the screen at 
    /// a time. Default value is 10.
    /// </summary>
    const int MAX_RINGS_PER_SET = 5;

    /// <summary>
    /// The amount of obstacles that can spawn with a given set. Just like
    /// the rings, this is the amount of obsctacles that are on the screen
    /// at a time. Default value is 5.
    /// </summary>
    const int MAX_NUM_OF_OBSTACLES = 10;

    /// <summary>
    /// This is the y-coordinate where the objects will resapwn from. 
    /// Default value is negative -40.
    /// </summary>
    const float RESPAWN_Y = -70;

    /// <summary>
    /// The amout of space to add between obstacle when they spawn.
    /// </summary>
    const float XZ_OFFSET = 20;

    /// <summary>
    /// The amount of space to add in between objects during spawn. Primarily
    /// used when spawning rings. Default value is 2.5.
    /// </summary>
    const float Y_OFFSET = 2.5f;

    /// <summary>
    /// The y-coordinate where the rings will spawn.
    /// </summary>
    const float Y_SPAWN = -30;
                
    /// <summary>
    /// The default rotation of the rings. DO NOT CHANGE VALUE!
    /// </summary>
    Vector3 RING_ROTATION = new Vector3(90, 0, 0);

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------

    #endregion


    #region Fields

    public static Spawner Instance { get; private set; }

    [SerializeField] Ball ball;
    [SerializeField] Ring prefabRing;
    [SerializeField] Obstacle prefabObstacle;
    [SerializeField] Transform ringParent,
                               obstacleParent;

    // cached variables
    int numOfRings,
        pointRing;

    float ySpawn;

    Vector3 ringSpawnPos,
            obstacleSpawnPos;

    Color colorRing;
    Ring spawnedRing;
    Obstacle spawnedObstacle;

    #endregion


    #region Unity Built-In Functions

    void Awake()
    {
        // this class does not need to be persisted, but there can only be one instance of this class
        if (FindObjectsOfType<Spawner>().Length == 1)
        {
            Instance = this;
            ySpawn = Y_SPAWN;
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
        SetupRings();
        SetupObstacles();
    }

    #endregion


    #region Setup

    /// <summary>
    /// This will spawn the initial set of rings with a uniform gap in between each ring. Overlapping is not taken into
    /// consideration at this stage.
    /// </summary>
    void SetupRings()
    {
        // sets a new random position in the world
        ringSpawnPos = RandomCoordinates(0, 0, 0, XZ_OFFSET - 10);

        for (int i = 0; i < MAX_RINGS_PER_SET; i++)
        {
            // set a uniform gap in between each ring
            ySpawn -= Y_OFFSET;
            ringSpawnPos.y = ySpawn;

            spawnedRing = Instantiate(prefabRing, ringSpawnPos, Quaternion.Euler(RING_ROTATION));
            spawnedRing.transform.parent = ringParent;
            
            // only randomly select a color and point value for the first ring, every ring after will have the same color
            // and points value as the first ring
            if (i == 0)
            {
                spawnedRing.SetColor();
                pointRing = spawnedRing.PointsAwarded;
                colorRing = spawnedRing.Color;
            }
            else
            {
                spawnedRing.SetColor(colorRing, pointRing);
            }
        }

        numOfRings = 0;
    }

    /// <summary>
    /// This will spawn the initial set of obstacles with a random amount of gap in between each obstacle. Overlapping is not taken into
    /// consideration at this stage.
    /// </summary>
    void SetupObstacles()
    {
        for (int i = 0; i < MAX_NUM_OF_OBSTACLES; i++)
        {
            // set new random coordinates
            obstacleSpawnPos = RandomCoordinates(0, 0, 0, XZ_OFFSET);
            ySpawn -= (Y_OFFSET * Random.Range(5, 15));
            obstacleSpawnPos.y = ySpawn;

            spawnedObstacle = Instantiate(prefabObstacle, obstacleSpawnPos, Quaternion.Euler(0, Random.Range(0, 45), 0));
            spawnedObstacle.transform.parent = obstacleParent;
            spawnedObstacle.SelectObstacle();
        }
    }

    #endregion


    #region Respawn

    /// <summary>
    /// Respawning of the ring, checks to make sure that there will not be any overlapping during the respawn.
    /// Gaps between the rings will be random. Will also randomly select a new color and amount of points.
    /// </summary>
    /// 
    /// <param name="ringObject"> The ring object that is to be respawned with a new position, color and point value. </param>
    public void RespawnRing(Ring ringObject)
    {
        // Randomly choose a coordinate, color and points awarded if this is the first ring in the set to respawn.
        // Otherwise, spawn the ring directly below the first ring that respawned and set the color and points
        // to the color and points of the first ring.
        if (numOfRings == 0)
        {
            spawnedRing = ringObject;
            spawnedRing.SetColor();
            ringSpawnPos = RandomCoordinates(0, 0, 0, XZ_OFFSET - 10); // the other three are left 0 so they can be randomly selected
        }
        else
        {
            ringSpawnPos.x = spawnedRing.transform.position.x;
            ringSpawnPos.z = spawnedRing.transform.position.z;
            ringObject.SetColor(spawnedRing.Color, spawnedRing.PointsAwarded);
        }

        // y-coordinate is assigned here because each ring will have a random gap
        ringSpawnPos.y = RESPAWN_Y - Random.Range(5, 15);
        ringObject.transform.position = ringSpawnPos;

        numOfRings++;

        if (numOfRings >= MAX_RINGS_PER_SET)
        {
            numOfRings = 0;
        }
    }

    /// <summary>
    /// Respawn the obstacles, checks to make sure that not obstacles will spawn into or top of each other.
    /// Will also randomly select a new obstacle mesh to display.
    /// </summary>
    /// 
    /// <param name="obstacle"> The obstacle object that is to be respawned with a new position and new mesh. </param>
    public void RespawnObstacle(Obstacle obstacle)
    {
        // set a random position for the obstacle
        obstacleSpawnPos = RandomCoordinates(0, RESPAWN_Y - Random.Range(5, 15), 0, XZ_OFFSET);
        obstacle.transform.position = obstacleSpawnPos;

        // set a random rotation on the y-axis
        obstacle.transform.rotation = Quaternion.Euler(0, Random.Range(0, 45), 0);

        // select the new obstacle mesh
        obstacle.SelectObstacle();
    }

    #endregion


    #region Utilities

    /// <summary>
    /// Checks to make sure that if there is an object where this object is about to spawn, then randomly select
    /// a new spawn point. This method will continously occur until it reaches the maximum number of checks or if it 
    /// found a suitable spawn point.
    /// </summary>
    /// 
    /// <param name="overrideX"> The x-coordinate to be set, leave default to have random coordinates selected. </param>
    /// <param name="overrideY"> The y-coordinate to be set, leave default to have random coordinates selected. </param>
    /// <param name="overrideZ"> The z-coordinate to be set, leave default to have random coordinates selected. </param>
    /// <param name="offset"> The amount of leeway (in units) that the obstacles and ring can spawn. </param>
    /// 
    /// <returns> 
    /// A new Vector3 with random x- and z-coordinates. Y will be either 0 or the override value since y will be overwritten 
    /// from the calling function anyways. 
    /// </returns>
    Vector3 RandomCoordinates(float overrideX = 0, float overrideY = 0, float overrideZ = 0, float offset = 0)
    {
        return new Vector3((overrideX != 0) ? overrideX : Random.Range(ball.transform.position.x - offset, ball.transform.position.x + offset),
                           (overrideY != 0) ? overrideY : 0,
                           (overrideZ != 0) ? overrideZ : Random.Range(ball.transform.position.z - offset, ball.transform.position.z + offset));
    }

    #endregion
}
