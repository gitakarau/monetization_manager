using UnityEngine;

[CreateAssetMenu(fileName = "keys", menuName = "Game/keys", order = 0)]
public class KeysTemplate : ScriptableObject
{
    [Header("App")]
    public string app_bundle_id = "";
    public string app_name = "";
    public string app_company_name = "";
    public string keystore_password = "";
    public string key_password = "";

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

#if UNITY_IOS
    public string ApplovinMax_InterstitialUnitId { get => ApplovinMax_InterstitialUnitId_ios; }
    public string ApplovinMax_RewardedUnitId { get => ApplovinMax_RewardedUnitId_ios; } 
#else
    public string ApplovinMax_InterstitialUnitId { get => ApplovinMax_InterstitialUnitId_android; }
    public string ApplovinMax_RewardedUnitId { get => ApplovinMax_RewardedUnitId_android; }

#endif

    public string privacy_policy { get; } = "https://akarau.com/privacypolicy.html";
    public string terms_of_service { get; } = "https://akarau.com/termsofuse.html";
}