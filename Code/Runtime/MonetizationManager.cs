using System;
using System.Collections;

using TMPro;

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

    /*
    public void ABTestInitialization()
    {
        if (PlayerPrefs.GetInt("ab", 0) == 0)
        {
            if (UnityEngine.Random.Range(0, 100) < 0)
            {
                PlayerPrefs.SetInt("ab", 1);
                MonetizationManager.Instance.ReportEvent("ab" + " " + "1");
                Debug.Log("ab" + " " + "1");
            }
            else
            {
                PlayerPrefs.SetInt("ab", 2);
                MonetizationManager.Instance.ReportEvent("ab" + " " + "2");
                Debug.Log("ab" + " " + "2");
            }
        }
    }
    */

    #endregion



    public void ReportEvent(string message)
    {
        //AppMetrica.Instance.ReportEvent(message);
    }

    string tenjinuserId;

    public void TenjinConnect()
    {
        BaseTenjin instance = Tenjin.getInstance(Keys.Tenjin_API_KEY);
#if UNITY_ANDROID
        instance.SetAppStoreType(AppStoreType.googleplay);
#endif

        instance.SetCustomerUserId("user_id");
        tenjinuserId = instance.GetCustomerUserId();
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
        instance.Connect();
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
            //InitializeRewardedAds();
            //InitializeBannerAds();
            //InitializeMRecAds();

            // Show Mediation Debugger
            //MaxSdk.ShowMediationDebugger();
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
            ShowInterstitialOnTimer();
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
            ShowNoConnectionPopUp();
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

    [SerializeField] private GameObject _contentTimer;
    [SerializeField] private TextMeshProUGUI _textTimer;
    [SerializeField] private Image _progressTimer;
    [SerializeField] private float _timeAds;

    public void ShowInterstitialOnTimer()
    {
        StartCoroutine(WaitToShowInterstitial());
    }

    private IEnumerator WaitToShowInterstitial()
    {
        _contentTimer.SetActive(true);
        var progress = _timeAds + 1f;
        while (progress > 1f)
        {
            progress -= Time.smoothDeltaTime;
            SetValue(progress, (progress - 1f) / _timeAds);
            yield return null;
        }

        _contentTimer.SetActive(false);
        ShowInterstitial();
    }

    public void SetValue(float value, float progress)
    {
        var time = TimeSpan.FromSeconds(value);
        _progressTimer.fillAmount = progress;
        _textTimer.text = $"{Mathf.CeilToInt(time.Seconds)}";
    }

    #endregion

    #region No Connection

    [SerializeField] private GameObject _panelNoConnectionPopUp;

    private bool _isOpenedNoConnectionPopUp;

    void Update()
    {
        if (_isOpenedNoConnectionPopUp)
        {
            if (IsConnectedToNetwork())
            {
                HideNoConnectionPopUp();
            }
        }
    }

    private void ShowNoConnectionPopUp()
    {
        _isOpenedNoConnectionPopUp = true;
        _panelNoConnectionPopUp.SetActive(true);
        Time.timeScale = 0.0f;
    }

    private void HideNoConnectionPopUp()
    {
        _isOpenedNoConnectionPopUp = false;
        _panelNoConnectionPopUp.SetActive(false);
        Time.timeScale = 1.0f;
    }


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
            MaxSdk.ShowInterstitial(Keys.ApplovinMax_InterstitialUnitId, "version " + Application.version);
        }

        m_ShowInterstitialCoroutine = null;
    }




    public Button FirstButton;
    public bool isGameLaunched = true;

    public void OnButtonPressShowInterstitialInitialization()
    {
        FirstButton.onClick.AddListener(delegate { if (isGameLaunched) { ShowInterstitialOnTimer(); isGameLaunched = false; } });

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
            ShowNoConnectionPopUp();
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






}