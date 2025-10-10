using UnityEngine;

[CreateAssetMenu(fileName = "keys", menuName = "Game/keys", order = 0)]
public class KeysTemplate : ScriptableObject
{
    [Header("App")]
    public string app_bundle_id = "";
    public string app_name = "";
    public string app_company_name = "";

#if UNITY_EDITOR
    [Header("Keystore (Visible only in Unity)")]
    public string keystore_password = "";
    public string key_password = "";
#endif

    [Header("mtg")]
    public string mtgAppKey = "";
    public string mtgAppId_android = "";
    public string mtgAppId_ios = "";

#if UNITY_IOS
    public string mtgAppId { get => mtgAppId_ios; }
#else 
    public string mtgAppId { get => mtgAppId_android; }
#endif

    [Header("IronSource")]
    public string IronSource_appKey_android = "";
    public string IronSource_appKey_ios = "";

    [Header("ApplovinMax")]

    public string ApplovinMax_MaxSdkKey = "";
    public string ApplovinMax_InterstitialUnitId_android = "";
    public string ApplovinMax_RewardedUnitId_android;
    public string ApplovinMax_InterstitialUnitId_ios = "";
    public string ApplovinMax_RewardedUnitId_ios;

    [Header("Tenjin")]
    public string Tenjin_API_KEY = "";

    [Header("Admob")]
    public string admob_app_id_android = "";
    public string admob_app_id_ios = "";
        
    [Header("MRec + Banner")]
    public bool ShowBannerOnStart = true;
    public string MRecAdId_android = "";
    public string MRecAdId_ios = "";
    public string BannerAdId_android = "";
    public string BannerAdId_ios = "";

#if UNITY_IOS
    public string ApplovinMax_InterstitialUnitId { get => ApplovinMax_InterstitialUnitId_ios; }
    public string ApplovinMax_RewardedUnitId { get => ApplovinMax_RewardedUnitId_ios; } 
    public string MRecAdId { get => MRecAdId_ios; }
    public string BannerAdId { get => BannerAdId_ios; }
#else
    public string ApplovinMax_InterstitialUnitId { get => ApplovinMax_InterstitialUnitId_android; }
    public string ApplovinMax_RewardedUnitId { get => ApplovinMax_RewardedUnitId_android; }
    public string MRecAdId { get => MRecAdId_android; }
    public string BannerAdId { get => BannerAdId_android; }
#endif

    public string privacy_policy { get; } = "https://akarau.com/privacypolicy.html";
    public string terms_of_service { get; } = "https://akarau.com/termsofuse.html";
}