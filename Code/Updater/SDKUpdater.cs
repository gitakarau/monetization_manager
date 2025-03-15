#if UNITY_EDITOR
using System.IO;

using UnityEditor;

using UnityEngine;

namespace Test
{
    public class SDKUpdater : EditorWindow
    {
        [MenuItem("Tools/Monetization Manager Updater")]
        public static void Open()
        {
            var window = EditorWindow.GetWindow<SDKUpdater>("Monetization Manager Updater");
            window.Show();
        }

        private void OnGUI()
        {
            SDKUpdateGUI();
            GUILayout.Space(10);
        }

        private void SDKUpdateGUI()
        {
            if (GUILayout.Button("Click here after SDK update"))
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

            string referencesJson = "";
            for (int i = 0; i < references.Length; i++)
            {
                referencesJson += $"\"{references[i]}\"";
                if (i < references.Length - 1)
                {
                    referencesJson += ", ";
                }
            }

            string platforms = "[]";

            if (assemblyName.Contains("Editor"))
            {
                platforms = "[\"Editor\"]";
            }

            string asmdefContent = $@"
        {{
            ""name"": ""{assemblyName}"",
            ""references"": [
                {referencesJson}
            ],
            ""includePlatforms"": {platforms},
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
}
#endif