/*****************************************************************************************************************
 - SaveManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Local saving and loading of the player's data files happens here. This class will persist and there will
     only be one instance of this class per game. There will only be one instance of this class and this class
     will persist through scene transitions.

     * Available Functions * 
     
     + bool Save(T save, string path)
        - save = the class that is to be save. Can be a class of any type. The default file type is JSON.
        - path = directory where the file will be saved. For most cases, use SavePath variable found in the 
                editable region.
        - returns: true if the save was successful, false if an exception occured.

     + bool Load(out T save, string path)
        - save = reference to the variable that will hold the save data.
        - path = directory to where the file was saved to.
        - returns: true if the load was successful, false if an exception occured.
-----------------------------------------------------------------------------------------------------------------
Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;

public class SaveManager : MonoBehaviour
{
    #region Editable Region

    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    /// <summary>
    /// The filename and file type of the save file.
    /// </summary>
    const string LOCAL_SAVE_NAME = "save_free_falling.json";

    /// <summary>
    /// The path to which the save file will be saved to. This file will be a JSON file.
    /// </summary>
    public string SavePath { get { return Application.persistentDataPath + "/" + LOCAL_SAVE_NAME; } }

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------

    #endregion


    #region Fields

    public static SaveManager Instance { get; private set; }


    /// <summary>
    /// Contains the player's save data.
    /// </summary>
    public SaveData PlayerSaveData { get; private set; }

    /// <summary>
    /// Contains the data that is to be persisted between scene transitions.
    /// </summary>
    public PersistantData PlayerPersistentData { get; private set; }

    #endregion


    #region Unity Built-In Functions

    /// <summary>
    /// Awake is called before start, whenever the object is created.
    /// </summary>
    void Awake()
    {
        // this class will persist through scene transitions
        if (FindObjectsOfType<SaveManager>().Length == 1)
        {
            Instance = this;
            PlayerPersistentData = new PersistantData();

            // load local data from storage
            SaveData temp;
            PlayerSaveData = (Load(out temp, SavePath)) ? temp : new SaveData();

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion


    #region Local Save/Load

    /// <summary>
    /// Saves the json file to a particular path. Returns true if successful,
    /// false if an exception occured.
    /// </summary>
    /// 
    /// <typeparam name="T"></typeparam>
    /// <param name="saveFile"> The class to be saved. </param>
    /// <param name="path"> The path to save the data to. </param>
    /// 
    /// <returns> True if save was successful, false if an exception occurred. </returns>
    public bool Save<T>(T saveFile, string path)
    {
        try
        {
            System.IO.File.WriteAllText(path, JsonUtility.ToJson(saveFile));
            return true;
        }
        catch (System.Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// Loads the json file from a particular path. Returns true if successful,
    /// false if an exception occured or the save file doesn't exist. On false,
    /// the method will return a default value for T.
    /// </summary>
    /// 
    /// <typeparam name="T"></typeparam>
    /// <param name="loadFile"> The file to be loaded. </param>
    /// <param name="path"> The file to load the file from. </param>
    /// 
    /// <returns> True if successfully loaded, false if encountered exception or file does not exist. </returns>
    public bool Load<T>(out T loadFile, string path)
    {
        try
        {
            if (System.IO.File.Exists(path))
            {
                loadFile = JsonUtility.FromJson<T>(System.IO.File.ReadAllText(path));
                return true;
            }

            loadFile = default(T);
            return false;
        }
        catch (System.Exception e)
        {
            loadFile = default(T);
            return false;
        }
    }

    #endregion
}
