#if UNITY_EDITOR
using System.IO;

using UnityEditor;
using UnityEditor.Android;
using UnityEditor.iOS;

using UnityEngine;

public class ProjectSetupUtility : EditorWindow
{
    private Texture2D m_Icon;

    private KeysTemplate Keys => MonetizationManager.Keys;

    [MenuItem("Tools/Setup Project")]
    public static void Open()
    {
        var window = EditorWindow.GetWindow<ProjectSetupUtility>();
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label(PlayerSettings.applicationIdentifier);
        GUILayout.Space(10);

        SDKUpdateGUI();
        GUILayout.Space(10);

        if (Keys == null)
        {
            GUILayout.Label("Keys object is null...");
            return;
        }

        SetupBundleVersion();
        GUILayout.Space(10);

        if (GUILayout.Button("Setup Applovin"))
        {
            SetupApplovin();
        }

        GUILayout.Space(10);

        SetupIcon();

        GUILayout.Space(10);

        SetupTitle();

        GUILayout.Space(10);

        SetupKeys();

        GUILayout.Space(10);
    }

    private void SetupBundleVersion()
    {
        GUILayout.BeginHorizontal();

        if (!int.TryParse(PlayerSettings.bundleVersion, out int version))
        {
            PlayerSettings.bundleVersion = "0";
            version = 0;
        }

        GUILayout.Label("Bundle version: " + version);

        if (GUILayout.Button("+"))
        {
            version++;

            SetVersion(version);
        }

        if (GUILayout.Button("-"))
        {
            version--;

            if (version < 0)
            {
                version = 0;
            }

            SetVersion(version);
        }

        GUILayout.EndHorizontal();
    }

    private void SetVersion(int version)
    {
        PlayerSettings.bundleVersion = version.ToString();
        PlayerSettings.Android.bundleVersionCode = version;
        PlayerSettings.iOS.buildNumber = version.ToString();
    }

    private void SetupApplovin()
    {
        AppLovinSettings.Instance.AdMobAndroidAppId = Keys.admob_app_id_android;
        AppLovinSettings.Instance.AdMobIosAppId = Keys.admob_app_id_ios;

        AppLovinSettings.Instance.QualityServiceEnabled = false;

        AppLovinSettings.Instance.ConsentFlowEnabled = true;
        AppLovinSettings.Instance.ConsentFlowPrivacyPolicyUrl = Keys.privacy_policy;
        AppLovinSettings.Instance.ConsentFlowTermsOfServiceUrl = Keys.terms_of_service;

        AppLovinSettings.Instance.UserTrackingUsageLocalizationEnabled = true;

        AppLovinSettings.Instance.SetAttributionReportEndpoint = true;

        EditorUtility.DisplayDialog("Project Setup", "Applovin setuped", "OK");
    }

    private void SetupIcon()
    {
        GUILayout.Label("Icon", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        m_Icon = (Texture2D)EditorGUILayout.ObjectField(m_Icon, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));

        if (GUILayout.Button("Set Icon"))
        {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { m_Icon });
            SetPlatformIcon(BuildTargetGroup.Android, AndroidPlatformIconKind.Adaptive, new Texture2D[] { m_Icon, m_Icon });
            SetPlatformIcon(BuildTargetGroup.Android, AndroidPlatformIconKind.Round, m_Icon);
            SetPlatformIcon(BuildTargetGroup.Android, AndroidPlatformIconKind.Legacy, m_Icon);

            SetPlatformIcon(BuildTargetGroup.iOS, iOSPlatformIconKind.Application, m_Icon);
            SetPlatformIcon(BuildTargetGroup.iOS, iOSPlatformIconKind.Spotlight, m_Icon);
            SetPlatformIcon(BuildTargetGroup.iOS, iOSPlatformIconKind.Settings, m_Icon);
            SetPlatformIcon(BuildTargetGroup.iOS, iOSPlatformIconKind.Notification, m_Icon);
            SetPlatformIcon(BuildTargetGroup.iOS, iOSPlatformIconKind.Marketing, m_Icon);

            EditorUtility.DisplayDialog("Project Setup", "Icon changed", "OK");
        }

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
    }

    private void SetupTitle()
    {
        GUILayout.Label("Titles", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"keys' app name: {Keys.app_name}");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"keys' company name: {Keys.app_company_name}");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"keys' bundle id: {Keys.app_bundle_id}");
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Update titles"))
        {
            PlayerSettings.productName = Keys.app_name;
            PlayerSettings.companyName = Keys.app_company_name;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, Keys.app_bundle_id);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, Keys.app_bundle_id);
        }
    }

    private void SetupKeys()
    {
        GUILayout.Label("Keys", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"keys' keystore password: {Keys.keystore_password}");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"keys' keys password: {Keys.key_password}");
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Update keys"))
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.keystorePass = Keys.keystore_password;
            PlayerSettings.keyaliasPass = Keys.key_password;
        }
    }

    private void SetPlatformIcon(BuildTargetGroup platform, PlatformIconKind kind, params Texture2D[] icon)
    {
        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(platform, kind);

        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].SetTextures(icon);
        }

        PlayerSettings.SetPlatformIcons(platform, kind, icons);
    }

    private void SDKUpdateGUI()
    {
        GUILayout.Label("SDK Update");

        if (GUILayout.Button("Create assemblies"))
        {
            CreateAssemblyDefinition("ROAS/Scripts", "ROAS.Runtime", "MaxSdk.Scripts");
            CreateAssemblyDefinition("Tenjin/Scripts", "Tenjin.Runtime", "MaxSdk.Scripts");
            CreateAssemblyDefinition("Tenjin/Scripts/Editor", "Tenjin.Editor");
        }
    }

    private void CreateAssemblyDefinition(string folderName, string assemblyName, params string[] references)
    {
        string folderPath = $"Assets/{folderName}";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Folder not found: " + folderPath);
            return;
        }

        string asmdefPath = Path.Combine(folderPath, $"{assemblyName}.asmdef");

        if (File.Exists(asmdefPath))
        {
            return;
        }

        string referencesJson = "";
        for (int i = 0; i < references.Length; i++)
        {
            referencesJson += $"\"{references[i]}\"";
            if (i < references.Length - 1)
            {
                referencesJson += ", ";
            }
        }

        string asmdefContent = $@"
        {{
            ""name"": ""{assemblyName}"",
            ""references"": [
                {referencesJson}
            ],
            ""includePlatforms"": [],
            ""excludePlatforms"": [],
            ""allowUnsafeCode"": false,
            ""overrideReferences"": false,
            ""precompiledReferences"": [],
            ""autoReferenced"": true,
            ""defineConstraints"": [],
            ""versionDefines"": [],
            ""noEngineReferences"": false
        }}";

        File.WriteAllText(asmdefPath, asmdefContent);
        AssetDatabase.Refresh();
    }
}
#endif