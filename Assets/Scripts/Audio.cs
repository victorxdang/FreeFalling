/*****************************************************************************************************************
 - Audio.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     This class handles playing background music and sound effect clips. Sound effects are not currently in use,
     but there is method that will play clips, see PlayClipSE() in the Sound Effects region. PlayClipSE() is a
     private method. In order to use it, write public helper methods that will call PlayClipSE(). There will 
     only be one instance of this class.
-----------------------------------------------------------------------------------------------------------------
 Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Audio : MonoBehaviour
{
    #region Editable Region

    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    /// <summary>
    /// Background music volume.
    /// </summary>
    const float BGM_VOLUME = 1;

    /// <summary>
    /// Sound effect volume.
    /// </summary>
    const float SE_VOLUME = 1;

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------

    #endregion


    #region Fields

    [SerializeField] AudioClip[] bgmClips;
    
    public static Audio Instance { get; private set; }

    // cache variables
    AudioClip nextClip;
    AudioSource source;

    #endregion


    #region Unity Built-In Functions

    /// <summary>
    /// Called when the game object is enabled.
    /// </summary>
    void Awake()
    {
        // this class will persist through scene transitions
        if (FindObjectsOfType<Audio>().Length == 1)
        {
            Instance = this;
            source = GetComponent<AudioSource>();

            // select a random bgm and play it if there is a bgm to be played
            if (bgmClips.Length > 0)
            {
                nextClip = GetRandomBGM();
                StartCoroutine(IEnumPlayBGM());
            }

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion


    #region BGM

    /// <summary>
    /// Plays the next bgm in queue. A new bgm will then be randomly selected and inserted into queue.
    /// </summary>
    /// 
    /// <returns> Yields for the length of the clip. </returns>
    System.Collections.IEnumerator IEnumPlayBGM()
    {
        PlayBGM(nextClip);
        nextClip = GetRandomBGM();

        yield return new WaitForSeconds(source.clip.length);
    }

    /// <summary>
    /// Plays a specified bgm clip with the specified volume.
    /// </summary>
    /// 
    /// <param name="bgmClip"> The clip to be played. </param>
    /// <param name="volume"> How loud the clip will be played. </param>
    void PlayBGM(AudioClip bgmClip, float volume = BGM_VOLUME)
    {
        if (bgmClip != null)
        {
            source.volume = volume;
            source.clip = bgmClip;
            source.Play();
        }
    }

    /// <summary>
    /// Selects a random background music from the array bgmClips[]. This method will also make
    /// sure that the next selected clip is not the same as the current clip.
    /// </summary>
    /// 
    /// <returns> The randomly selected audio clip. </returns>
    AudioClip GetRandomBGM()
    {
        int randomClip;

        do
        {
            randomClip = Random.Range(0, bgmClips.Length);
        } while (bgmClips[randomClip] == nextClip);

        return bgmClips[randomClip];
    }

    #endregion


    #region Sound Effects

    /// <summary>
    /// Plays a specified audio clip with the specified volume. Create public helper methods to call this function
    /// when a specific sound effect is needed to be played.
    /// </summary>
    /// 
    /// <param name="seClip"> The clip to be played </param>
    /// <param name="volume"> How loud the clip will be played. </param>
    void PlayClipSE(AudioClip seClip, float volume = SE_VOLUME)
    {
        if (seClip != null)
        {
            source.PlayOneShot(seClip, volume);
        }
    }

    #endregion
}
