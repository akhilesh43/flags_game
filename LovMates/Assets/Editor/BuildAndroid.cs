#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LovMates.EditorTools
{
    public static class BuildAndroid
    {
        private const string OutputPath = "Builds/LovMates.apk";

        [MenuItem("LovMates/Build APK")]
        public static void BuildMenu()
        {
            BuildCI();
        }

        public static void BuildCI()
        {
            // Build settings mein enabled scenes lo
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                // Fallback: Agar koi scene configured nahi hai toh default scene bana kar register karo
                const string fallbackScene = "Assets/Scenes/Main.unity";
                Directory.CreateDirectory("Assets/Scenes");
                if (!File.Exists(fallbackScene))
                {
                    var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(newScene, fallbackScene);
                }
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(fallbackScene, true) };
                scenes = new[] { fallbackScene };
            }

            // Builds folder na ho toh bana do
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

            // APK format (not AAB)
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.Exception("Android build fail hua: " + report.summary.result);
            }

            Debug.Log("APK ban gaya: " + OutputPath);
        }
    }
}
#endif
