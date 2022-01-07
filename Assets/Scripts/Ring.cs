/*****************************************************************************************************************
 - Ring.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class will keep track of the current color and amount of points of the ring and updates the color 
     and the points of the rings when needed.

 Available Attributes:

     - int Index = index of Color and PointsAwarded value found in the Colors and Points array in GameManager.

     - int PointsAwarded = the amount of points this ring will award when passing through it successfully.

     - Color Color = the color of the ring. This will be an indication of how many point this ring awards.

 Available Functions:

     * void SetColor(int colorIndex)
        - A helper method for SetColor(Color, int). This sets the color based on the index provided. Will 
          randomly select a color and appropriate points awarded by default. To override this, provide a valid 
          color index. Just the color index is needed because Colors and Points are parallel arrays.
        - colorIndex = the index at which the Color and Points can be located within the Color and Points array.

     * void SetColor(Color updatedColor, int updatedScore)
        - Sets Color and PointsAwarded to updatedColor and updatedScore, respectively and if their values are
          different. The material of ring will be updated within this function as well.
        - updatedColor = the new color, if different than the current color.
        - updatedScore = the new score, if different than the current score.

-----------------------------------------------------------------------------------------------------------------
Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;

public class Ring : MovingEntity
{
    /// <summary>
    /// The index where the Color and PointAwarded are located within the Colors and Points array
    /// in GameManager. Can be used in place of Color and PointsAwarded variables, if desired.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The amount of points that is rewarded to the player when they go through this ring
    /// </summary>
    public int PointsAwarded { get; set; }

    /// <summary>
    /// The color of the ring. Has no actual mechanic to the game besides being an indiciator
    /// of how many point this ring will award.
    /// </summary>
    public Color Color { get; set; }


    /// <summary>
    /// Sets the color and points of the ring based on the provdided color index (see GameManager class for colors and points).
    /// If the index provided is less than 0 or greater than the number of colors available, then a random color with its corresponding
    /// points will be selected.
    /// </summary>
    /// 
    /// <param name="colorIndex"> The index where to find Color and PointsAwarded in the Colors and Points array (respectively) in GameManager </param>
    public void SetColor(int colorIndex = -1)
    {
        Index = (colorIndex <= -1 || colorIndex >= GameManager.Instance.Colors.Length) ? Random.Range(0, GameManager.Instance.Colors.Length) : colorIndex;
        SetColor(GameManager.Instance.Colors[Index], GameManager.Instance.Points[Index]);
    }

    /// <summary>
    /// Sets the color and points of the ring. Ensures that the material of the object also changes to the updated color.
    /// </summary>
    /// 
    /// <param name="updatedColor"> The new color to be shown, if different than the current color </param>
    /// <param name="updatedPoints"> The new amount of points to be awarded, if different than the current value </param>
    public void SetColor(Color updatedColor, int updatedPoints)
    {
        if (Color != updatedColor)
        {
            Color = updatedColor;
            GetComponent<Renderer>().material.color = updatedColor;
        }

        if (PointsAwarded != updatedPoints)
        {
            PointsAwarded = updatedPoints;
        }
    }
}