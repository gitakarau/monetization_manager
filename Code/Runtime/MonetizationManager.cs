using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.UI;

public class MonetizationManager : MonoBehaviour
{

    public static MonetizationManager Instance { get; private set; }

    public UnityAction OnRewardFinish;

    public void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        
        ABTestInitialization();

        m_MRecContent.gameObject.SetActive(false);
        m_MRecContent.anchoredPosition = Vector2.zero;
        m_CloseMRecButton.onClick?.AddListener(OnClickCloseMRec);

        Instance = this;
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame

    public bool CONSENT;

    private static KeysTemplate s_Keys;
    public static KeysTemplate Keys
    {
        get
        {
            if (s_Keys == null)
            {
                s_Keys = Resources.Load("keys") as KeysTemplate;
            }

            return s_Keys;
        }
    }

    private void Start()
    {


        Application.targetFrameRate = 60;
        CONSENT = true;
        TenjinConnect();
#if UNITY_Android

#endif


#if UNITY_IOS
        Device.RequestStoreReview();
#endif
        ApplovinInititalization();
        MintegralROASInitialization();
    }


    public void MintegralROASInitialization()
    {
        MBridgeSDKManager.initialize(Keys.mtgAppId, Keys.mtgAppKey);




    }




    #region AB Tests

    public int AB { get; private set; }
    
    public void ABTestInitialization()
    {
        AB = PlayerPrefs.GetInt("ab", 0);
        
        if (AB == 0)
        {
            AB = UnityEngine.Random.Range(0, 100) < 50 ? 1 : 2;
        }

        Debug.Log("ab " + AB);
        PlayerPrefs.SetInt("ab", AB);
    }

    #endregion



    public void ReportEvent(string message)
    {
        //AppMetrica.Instance.ReportEvent(message);
    }

    string tenjinuserId;
    BaseTenjin tenjinInstance;

    public void TenjinConnect()
    {
        tenjinInstance = Tenjin.getInstance(Keys.Tenjin_API_KEY);
#if UNITY_ANDROID
        tenjinInstance.SetAppStoreType(AppStoreType.googleplay);
#endif

        tenjinInstance.SetCustomerUserId("user_id");
        tenjinuserId = tenjinInstance.GetCustomerUserId();
        /*
        if (instance.OptInOutUsingCMP())
        {
            instance.OptIn();
        }
        else
        {
            instance.OptOut();
        }
        */
        // Sends install/open event to Tenjin
        tenjinInstance.Connect();
    }




    #region Applovin Methods


    #region Applovin variables

    #endregion

    #region Applovin Initialization

    public void ApplovinInititalization()
    {



        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {

            MaxSdk.SetDoNotSell(true);
            //MaxSdk.SetHasUserConsent(CONSENT);
            // AppLovin SDK is initialized, configure and start loading ads.
            Debug.Log("MAX SDK Initialized");



#if UNITY_IOS
            TenjinConnect();
#endif
            InitializeInterstitialAds();

            if (Keys.InitializeBanner)
            {
                InitializeBannerAds();
            }

            if (Keys.InitializeMRec)
            {
                InitializeMRecAds();
            }
            
            //InitializeRewardedAds();
            //InitializeBannerAds();
            //InitializeMRecAds();

            // Show Mediation Debugger
            //MaxSdk.ShowMediationDebugger();
            
            tenjinInstance.SubscribeAppLovinImpressions();
        };

        MaxSdk.SetSdkKey(Keys.ApplovinMax_MaxSdkKey);
        MaxSdk.InitializeSdk();
        //MaxSdk.InitializeSdk(new string[] { InterstitialUnitId });
    }



    #endregion

    #region Interstitial Applovin Methods

    int retryAttemptInterstitial;

    public static UnityEvent OnLostConnection { get; private set; } = new UnityEvent();

    public static bool IsConnectedToNetwork()
    {
        if (Application.isEditor)
        {
            //return true;
        }

        return Application.internetReachability != NetworkReachability.NotReachable;
    }
    
    public void ShowHideBanner(bool isShow)
    {
        if (isShow)
        {
            MaxSdk.ShowBanner(Keys.BannerAdId);
        }
        else
        {
            MaxSdk.HideBanner(Keys.BannerAdId);
        }
    }

    public void ShowHideMRec(bool isShow)
    {
        if (isShow)
        {
            if (!m_IsMRecReady)
            {
                return;
            }
            
            m_MRecContent.gameObject.SetActive(true);
            Time.timeScale = 0f;

            m_IsMRecReady = false;
            MaxSdk.ShowMRec(Keys.MRecAdId);
        }
        else
        {
            m_MRecContent.gameObject.SetActive(false);

            m_IsMRecReady = false;
            Time.timeScale = 1.0f;
            MaxSdk.HideMRec(Keys.MRecAdId);
            MaxSdk.LoadMRec(Keys.MRecAdId);
        }
    }
    
    public void InitializeBannerAds()
    {
        MaxSdk.SetBannerPlacement(Keys.BannerAdId, "version " + Application.version);
        MaxSdk.CreateBanner(Keys.BannerAdId, MaxSdkBase.BannerPosition.BottomCenter);
        
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannedAdLoadedEvent;
	MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

    }
    
    public void OnBannedAdLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        if (Keys.ShowBannerOnStart && !m_IsBannerShowed)
        {
            ShowHideBanner(true);
        }
        
        m_IsBannerShowed = true;
    }
    
    public void InitializeMRecAds()
    {
        MaxSdk.SetMRecPlacement(Keys.MRecAdId, "version " + Application.version);
        MaxSdk.CreateMRec(Keys.MRecAdId, MaxSdkBase.AdViewPosition.Centered);
        MaxSdk.StopMRecAutoRefresh(Keys.MRecAdId);
        
        MaxSdkCallbacks.MRec.OnAdLoadedEvent      += OnMRecAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent  += OnMRecAdLoadFailedEvent;
        MaxSdkCallbacks.MRec.OnAdClickedEvent     += OnMRecAdClickedEvent;
	MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
    }

    private bool m_IsMRecReady;
    private bool m_IsBannerShowed;

    public void OnMRecAdLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        m_IsMRecReady = true;
    }

    public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdk.ErrorInfo error)
    {
        m_IsMRecReady = false;
        
        MaxSdk.LoadMRec(Keys.MRecAdId);
    }

    public void OnMRecAdClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        ShowHideMRec(false);
    }
    
    private void OnClickCloseMRec()
    {
        ShowHideMRec(false);
    }




    void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        MBridgeRevenueParamsEntity mBridgeRevenueParamsEntity = new MBridgeRevenueParamsEntity(MBridgeRevenueParamsEntity.ATTRIBUTION_PLATFORM_TENJIN, tenjinuserId);
        #if MAX
        mBridgeRevenueParamsEntity.SetMaxAdInfo(adInfo);
        #endif
        MBridgeRevenueManager.Track(mBridgeRevenueParamsEntity);

    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    { 
         MBridgeRevenueParamsEntity mBridgeRevenueParamsEntity = new MBridgeRevenueParamsEntity(MBridgeRevenueParamsEntity.ATTRIBUTION_PLATFORM_TENJIN, tenjinuserId);
        #if MAX
        mBridgeRevenueParamsEntity.SetMaxAdInfo(adInfo);
        #endif
        MBridgeRevenueManager.Track(mBridgeRevenueParamsEntity);
    }



    public void InitializeInterstitialAds()
    {
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;

        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;

        // Load the first interstitial
        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(Keys.ApplovinMax_InterstitialUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        if (isGameLaunched)
        {
            // ShowInterstitialOnTimer();
            isGameLaunched = false;
        }
        // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(InterstitialUnitId) now returns 'true'

        // Reset retry attempt
        retryAttemptInterstitial = 0;
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

        retryAttemptInterstitial++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttemptInterstitial));

        Invoke("LoadInterstitial", (float)retryDelay);
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
        MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        LoadInterstitial();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad.
        LoadInterstitial();
    }

    void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {

        /*
        ATTRIBUTION_PLATFORM_APPSFLYER = "AppsFlyer";
        ATTRIBUTION_PLATFORM_ADJUST = "Adjust";
        ATTRIBUTION_PLATFORM_TENJIN = "Tenjin";
        ATTRIBUTION_PLATFORM_SINGULAR = "Singular";
        ATTRIBUTION_PLATFORM_KOCHAVA = "Kochava";
        ATTRIBUTION_PLATFORM_BRANCH = "Branch";
        ATTRIBUTION_PLATFORM_REYUN = "Reyun";
        ATTRIBUTION_PLATFORM_SOLAR_ENGINE = "SolarEngine";
        ...
        ...
        */
        // Replace with your attribution platform name, for example, "Adjust", and replace "userid" with your attribution platform UID
        MBridgeRevenueParamsEntity mBridgeRevenueParamsEntity = new MBridgeRevenueParamsEntity(MBridgeRevenueParamsEntity.ATTRIBUTION_PLATFORM_TENJIN, tenjinuserId);

        // adInfo: a instance of MaxSdkBase.AdInfo
#if MAX
        mBridgeRevenueParamsEntity.SetMaxAdInfo(adInfo);
#endif
        MBridgeRevenueManager.Track(mBridgeRevenueParamsEntity);

    }

    private Coroutine m_ShowInterstitialCoroutine;

    public void ShowInterstitial()
    {
        if (!IsConnectedToNetwork())
        {
            OnLostConnection?.Invoke();
            return;
        }

        if (m_ShowInterstitialCoroutine != null)

        {
            StopCoroutine(m_ShowInterstitialCoroutine);
            m_ShowInterstitialCoroutine = null;
        }

        m_ShowInterstitialCoroutine = StartCoroutine(ShowInterstitialWithDelay());
    }






    #region ShowInterstitialOnTimer

    [SerializeField] private RectTransform m_MRecContent;
    [SerializeField] private Button m_CloseMRecButton;
    
    #endregion


    private IEnumerator ShowInterstitialWithDelay()
    {



        const int skipFrameCount = 0;

        for (int i = 0; i < skipFrameCount; i++)
        {
            yield return null;


        }


        if (MaxSdk.IsInterstitialReady(Keys.ApplovinMax_InterstitialUnitId))
        {

            Debug.Log("InterstitialReady");
            ReportEvent("InterstitialReady");
            MaxSdk.ShowInterstitial(Keys.ApplovinMax_InterstitialUnitId, "version " + Application.version + " " + (AB == 1 ? "a" : "b"));
        }

        m_ShowInterstitialCoroutine = null;
    }




    public Button FirstButton;
    public bool isGameLaunched = true;

    public void OnButtonPressShowInterstitialInitialization()
    {
        FirstButton.onClick.AddListener(delegate { if (isGameLaunched) { isGameLaunched = false; } });

    }


    #endregion

    #region Rewarded Applovin Methods

    int retryAttemptRewarded;

    public void InitializeRewardedAds()
    {
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        // Load the first rewarded ad
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(Keys.ApplovinMax_RewardedUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(RewardedUnitId) now returns 'true'.

        // Reset retry attempt
        retryAttemptRewarded = 0;
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

        retryAttemptRewarded++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryAttemptRewarded));

        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
        LoadRewardedAd();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // The rewarded ad displayed and the user should receive the reward.
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Ad revenue paid. Use this callback to track user revenue.
    }

    public void ShowRewarded()
    {
        if (IsConnectedToNetwork() == false)
        {
            OnLostConnection?.Invoke();
            return;
        }
        try
        {
            if (MaxSdk.IsRewardedAdReady(Keys.ApplovinMax_RewardedUnitId))
            {
                MaxSdk.ShowRewardedAd(Keys.ApplovinMax_RewardedUnitId);
            }
        }
        catch
        {

        }

    }

    #endregion

    #endregion



    #region Popup Methods
    public void PopupClose(GameObject Window)
    {
        Window.SetActive(false);
    }
    public void PopupOpen(GameObject Window)
    {
        Window.SetActive(true);
    }
    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }
    #endregion



    #region SubscriptionMethods

    private string SubscriptionID = "6months";
    public GameObject Popup_Subscription;
    public Button Close_Popup_Subscription;



    public void SubscriptionInitialization()
    {
        if (PlayerPrefs.GetInt("Subscription", 0) == 1)
        {
            OnPurchaseCompleteSubscription();
        }
        else
        {
            PopupOpen(Popup_Subscription);
        }

        Close_Popup_Subscription.onClick.AddListener(delegate
        {
            PopupClose(Popup_Subscription);
            ShowInterstitial();
        });

    }

    public GameObject Popup_PremiumUI;
    public void OnPurchaseCompleteSubscription()
    {


        PlayerPrefs.SetInt("Subscription", 1);
        PopupOpen(Popup_PremiumUI);
        Debug.Log("Subscribtion" + " " + SubscriptionID);
        Debug.Log(PlayerPrefs.GetInt("Subscription", 0));
        Invoke("PopupCloseSubscription", 0.1f);
    }

    public void OnPurchaseFailedSubscription()
    {

        //FirebaseInitialization();
        PopupClose(Popup_Subscription);
        ShowInterstitial();
    }

    public void PopupCloseSubscription()
    {
        PopupClose(Popup_Subscription);
    }







    #endregion




    private void OnDestroy()
    {
        m_CloseMRecButton.onClick?.RemoveListener(OnClickCloseMRec);
    }
}