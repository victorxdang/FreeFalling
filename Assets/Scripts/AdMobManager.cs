/*****************************************************************************************************************
 - AdMobManager.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  Free Falling
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Ads are handled this script. Currently, only Google AdMob are supported. To call ads from this script, call 
     the ShowInterstitialAd() or ShowRewardedVideoAd() methods appropriately. To de-reference variables that point 
     to certain ads, use the CleanUp() method. There will only be one instance of this class.


 Available Functions:
     
     * void ShowBannerAd()
        - Displays a banner ad if one is available.

     * void ShowInterstitalAd()
        - Displays an interstital ad if one is available.

     * void ShowRewardedVideoAd()
        - Displays a rewarded video ad if one is available.

     * bool InterstitalAdLoaded()
        - Returns the status of whether or not the interstital ad is loaded and ready to be displayed.
        - returns : true if the interstital is loaded and ready to be displayed, false otherwise.

     * bool RewardedAdLoaded()
        - Returns the status of whether or not the rewarded video ad is loaded and ready to be displayed.
        - returns : true if the rewarded video ad is loaded and ready to be displayed, false otherwise.

     * void DestroyBanner(bool forceDestroy)
        - Will hide or destory the banner ad, depending on what is specified.
        - forceDestroy = if true, then the banner ad will be de-referenced, otherwise the banner ad will just
                         be hidden.

     * void DestroyInterstital()
        - Completely de-references the interstitial ad, it will not be hidden.

     * CleanUp() 
        - De-references both banner and interstital ads if they still exist.


     NOTE: To enable live ads, set the AD_TEST_MODE variable 
     in GameManager to true. Set it to false to use test ads.
-----------------------------------------------------------------------------------------------------------------
 Version:
    - 1.0.0 : (2/17/19) First offical release.
*****************************************************************************************************************/

using GoogleMobileAds.Api;

public class AdMobManager : UnityEngine.MonoBehaviour
{
    #region Editable Region

    //---------------------------------------------------------------------------
    // Begin Editable variables
    //---------------------------------------------------------------------------

    #if UNITY_ANDROID
        // the id of the app (AdMob)
        const string ADMOB_APPID_ANDROID = "ca-app-pub-8168057146331582~6869190135";

        // the id of each live ad unit (AdMob)
        const string BANNER_ANDROID = "ca-app-pub-8168057146331582/6733458768";
        const string INTERSTITIAL_ANDROID = "ca-app-pub-8168057146331582/3831963253";
        const string REWARDED_ANDROID = "ca-app-pub-8168057146331582/6541887073";

        // the id of each test ad unit (AdMob)
        const string TEST_BANNER_ANDROID = "ca-app-pub-3940256099942544/6300978111";
        const string TEST_INTERSTITIAL_ANDROID = "ca-app-pub-3940256099942544/1033173712";
        const string TEST_REWARDED_ANDROID = "ca-app-pub-3940256099942544/5224354917";

    #elif UNITY_IOS
        // the id of the app (AdMob)
        const string ADMOB_APPID_IPHONE = "";

        // the id of each test ad unit (AdMob)
        const string BANNER_IPHONE = "";
        const string INTERSTITIAL_IPHONE = "";
        const string REWARDED_IPHONE = "";

        // the id of each live ad unit (AdMob)
        const string TEST_BANNER_IPHONE = "ca-app-pub-3940256099942544/2934735716";
        const string TEST_INTERSTITIAL_IPHONE = "ca-app-pub-3940256099942544/4411468910";
        const string TEST_REWARDED_IPHONE = "ca-app-pub-3940256099942544/1712485313";
    #endif

    //---------------------------------------------------------------------------
    // End Editable variables
    //---------------------------------------------------------------------------

    #endregion


    #region Fields

    public static AdMobManager Instance { get; private set; }

    // cached variables
    string adMobAppID, 
           adMobBannerID, 
           adMobInterstitalID, 
           adMobRewardedID;

    BannerView adMobBanner;
    InterstitialAd adMobInterstitial;
    RewardBasedVideoAd adMobRewardVideo;

    #endregion


    #region Unity Built-In Functions

    /// <summary>
    /// Awake is called before start, whenever the object is created.
    /// </summary>
    public void Awake()
    {
        if (FindObjectsOfType<AdMobManager>().Length == 1)
        {
            Instance = this;

            // setup appropriate ID's for the type of platform this game will be on, if test mode is enabled, then use the test
            // ad id's
            #if UNITY_ANDROID
                adMobAppID = ADMOB_APPID_ANDROID;
                adMobBannerID = (GameManager.Instance.AD_TEST_MODE) ? TEST_BANNER_ANDROID : BANNER_ANDROID;
                adMobInterstitalID = (GameManager.Instance.AD_TEST_MODE) ? TEST_INTERSTITIAL_ANDROID : INTERSTITIAL_ANDROID;
                adMobRewardedID = (GameManager.Instance.AD_TEST_MODE) ? TEST_REWARDED_ANDROID : REWARDED_ANDROID;
                UnityEngine.Debug.Log("Test Mode? " + GameManager.Instance.AD_TEST_MODE);
            #elif UNITY_IOS
                adMobAppID = ADMOB_APPID_IPHONE;
                adMobBannerID = (GameManager.Instance.AD_TEST_MODE) ? TEST_BANNER_IPHONE : BANNER_IPHONE;
                adMobInterstitalID = (GameManager.Instance.AD_TEST_MODE) ? TEST_INTERSTITIAL_IPHONE : INTERSTITIAL_IPHONE;
                adMobRewardedID = (GameManager.Instance.AD_TEST_MODE) ? TEST_REWARDED_IPHONE : REWARDED_IPHONE;
            #else
                adMobAppID = "unexpected_platform";
                adMobBannerID = "unexpected_platform";
                adMobInterstitalID = "unexpected_platform";
                adMobRewardedID = "unexpected_platform";
            #endif

            // initialize AdMob ads
            MobileAds.Initialize(adMobAppID);

            // initialize and load the various ad types, banners show up immediately after
            // being called so it is defaulted to hide the ad when loaded. Use ShowBannerAd()
            // method to display the ad when needed
            RequestAdMobBannerAd();
            RequestAdMobInterstitialAd();
            RequestAdMobRewardedVideoAd();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion


    #region Show Ads

    /// <summary>
    /// Displays a banner ad at the bottom of the screen.
    /// </summary>
    /// <param name="showAd"></param>
    public void ShowBannerAd()
    {
        if (adMobBanner == null)
        {
            RequestAdMobBannerAd();
        }

        adMobBanner.Show();
    }

    /// <summary>
    /// Shows a full screen ad, either interactable or static, or a skippable video.
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (adMobInterstitial.IsLoaded())
        {
            adMobInterstitial.Show();
        }
    }

    /// <summary>
    /// Call to play a rewarded video ad (unskippable). Prioritizes AdMob over Unity ads.
    /// </summary>
    public void ShowRewardedVideoAd()
    {
        if (adMobRewardVideo.IsLoaded())
        {
            adMobRewardVideo.Show();
        }
    }

    #endregion


    #region AdMob Ads

    /// <summary>
    /// Creates a new banner ad that will be displayed on the bottom of screen.
    /// There is no Show() method call for banner so after it is finished creating, then
    /// the banner ad will be displayed on the screen.
    /// 
    /// This method should only be called once.
    /// </summary>
    void RequestAdMobBannerAd()
    {
        // create the banner ad
        adMobBanner = new BannerView(adMobBannerID, AdSize.SmartBanner, AdPosition.Bottom);

        adMobBanner.OnAdLoaded += HandleAdMobBannerAdLoaded;
        adMobBanner.OnAdFailedToLoad += HandleAdMobBannerAdFailedToLoad;

        // load the banner ad to display
        adMobBanner.LoadAd(new AdRequest.Builder().Build());
    }

    /// <summary>
    /// Creates a new full screen interstitial ad. To show an interstital, the
    /// ShowInterstitialAd() method needs to be called, this method only loads the
    /// interstital ad.
    /// </summary>
    void RequestAdMobInterstitialAd()
    {
        // destroy the previous interstital (if one exists) before creating a new one
        DestroyInterstitial();

        // create a new interstitial ad
        adMobInterstitial = new InterstitialAd(adMobInterstitalID);

        adMobInterstitial.OnAdLoaded += HandleAdMobInterstitialAdLoaded;
        adMobInterstitial.OnAdFailedToLoad += HandleAdMobInterstitialAdFailedToLoad;

        // load the interstital ad
        adMobInterstitial.LoadAd(new AdRequest.Builder().Build());
    }

    /// <summary>
    /// Loads a rewarded video ad. To show the rewarded video ad, the 
    /// ShowRewardedVideoAd() needs to be called, like the interstitial
    /// method, this method will only load the rewarded video ad.
    /// </summary>
    void RequestAdMobRewardedVideoAd()
    {
        // get the singleton of rewarded video ads
        adMobRewardVideo = RewardBasedVideoAd.Instance;

        adMobRewardVideo.OnAdRewarded += HandleAdMobRewardedAd;
        adMobRewardVideo.OnAdFailedToLoad += HandleAdMobRewardedVideoFailedToLoad;

        // load the rewarded video ad
        adMobRewardVideo.LoadAd(new AdRequest.Builder().Build(), adMobRewardedID);
    }

    #endregion


    // all callbacks are empty, there is a bug in this current version of Unity (see above) where callbacks are not able to
    // be called causing freezing of the game or crashing. Do not use these callbacks until Unity fixes them. It is fine to
    // leave them empty, but any code within them will cause the game to freeze or crash.
    #region AdMob Callbacks

    /// <summary>
    /// This method handles the events that should occur after a banner ad is loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void HandleAdMobBannerAdLoaded(object sender, System.EventArgs args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// Handle for whenever a banner ad fails to load (i.e. no internet connection).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void HandleAdMobBannerAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// This method handles the events that should occur after an interstitial ad is loaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void HandleAdMobInterstitialAdLoaded(object sender, System.EventArgs args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// Handle for whenever an interstitial ad fails to load (i.e. no internet connection).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void HandleAdMobInterstitialAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// This handles the events that should occur after the rewarded video ad is completed and 
    /// closed by the user.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void HandleAdMobRewardedAd(object sender, Reward args)
    {
        // nothing happens here, yet
    }

    /// <summary>
    /// Handle for whenever an rewarded video fails to load (i.e. no internet connection).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void HandleAdMobRewardedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        // nothing happens here, yet
    }

    #endregion


    #region Ad Utilites

    /// <summary>
    /// Returns whether or not an interstital ad has been loaded.
    /// </summary>
    /// <returns></returns>
    public bool InterstitalAdLoaded()
    {
        return adMobInterstitial.IsLoaded();
    }

    /// <summary>
    /// Returns whether or not a rewarded video ad has been loaded.
    /// </summary>
    /// <returns></returns>
    public bool RewardedAdLoaded()
    {
        return adMobRewardVideo.IsLoaded();
    }

    /// <summary>
    /// Should be called whenever the game exits.
    /// </summary>
    public void CleanUp()
    {
        DestroyBanner(true); // actually destroy the banner
        DestroyInterstitial();
    }

    /// <summary>
    /// Destroys any banner ads on the screen. However, under normal circumstances, the banner will only be
    /// hidden and not actually destroyed. If froceDestroy is true, then the banner will then be de-referenced.
    /// </summary>
    /// <param name="forceDestroy"> Set to true to de-reference banner, false to hide it. </param>
    public void DestroyBanner(bool forceDestroy = false)
    {
        // destroy AdMob banner ad (if specified), else hide it
        if (adMobBanner != null)
        {
            if (forceDestroy)
                adMobBanner.Destroy();
            else
                adMobBanner.Hide();
        }
    }

    /// <summary>
    /// De-references the interstitial ad on the screen.
    /// </summary>
    public void DestroyInterstitial()
    {
        if (adMobInterstitial != null)
            adMobInterstitial.Destroy();
    }

    #endregion
}
