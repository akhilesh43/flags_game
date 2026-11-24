#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LovMates.EditorTools
{
    /// <summary>Menu: LovMates > Build APK.   CLI: -executeMethod LovMates.EditorTools.BuildAndroid.BuildCI</summary>
    public static class BuildAndroid
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("LovMates/Build APK")]
        public static void BuildMenu() { Build(false); }

        public static void BuildCI() { Build(true); }

        static void Build(bool batch)
        {
            // 1. an (empty) scene is enough - GameManager boots itself at runtime
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);

            // 2. player settings
            PlayerSettings.companyName = "LovMates";
            PlayerSettings.productName = "LovMates";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.lovmates.ourlittleworld");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            // 3. build
            Directory.CreateDirectory("Builds");
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/LovMates.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            bool ok = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
            Debug.Log(ok ? "APK built: Builds/LovMates.apk" : "APK build FAILED: " + report.summary.result);
            if (batch) EditorApplication.Exit(ok ? 0 : 1);
        }
    }
}
#endif
