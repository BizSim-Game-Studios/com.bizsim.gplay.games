// Copyright (c) BizSim Game Studios. All rights reserved.

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BizSim.GPlay.Games.Editor
{
    /// <summary>
    /// Injects Google Play Games Services v2 dependency into Gradle build.
    /// Runs before Android build to ensure play-services-games-v2 is available.
    ///
    /// Strategy:
    /// 1. Ensure mainTemplate.gradle exists (copy from Unity defaults if missing)
    /// 2. Ensure settingsTemplate.gradle exists for Unity 6+
    /// 3. Inject repositories into settingsTemplate (or mainTemplate for legacy)
    /// 4. Inject dependency into mainTemplate
    /// 5. Never throws BuildFailedException — handles everything in a single pass
    /// </summary>
    public class GradleDependencyInjector : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private const string DependencyLine = "    implementation 'com.google.android.gms:play-services-games-v2:20.1.1'";
        private const string GoogleRepoLine = "        google()";
        private const string MavenCentralLine = "        mavenCentral()";

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            Debug.Log("[GamesServices] Injecting Gradle dependencies...");

            string pluginsDir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Assets/Plugins/Android");
            string mainPath = Path.Combine(pluginsDir, "mainTemplate.gradle");
            string settingsPath = Path.Combine(pluginsDir, "settingsTemplate.gradle");

            // Ensure templates exist (copy from Unity defaults if missing)
            EnsureGradleTemplate("mainTemplate.gradle", pluginsDir);

            if (IsUnity2022OrNewer())
            {
                EnsureGradleTemplate("settingsTemplate.gradle", pluginsDir);
            }

            // Inject dependencies
            if (File.Exists(settingsPath))
            {
                InjectRepositoriesToSettings(settingsPath);
                InjectDependenciesToMain(mainPath);
                Debug.Log("[GamesServices] Dependencies injected (settingsTemplate + mainTemplate)");
            }
            else
            {
                InjectDependencies(mainPath);
                Debug.Log("[GamesServices] Dependencies injected (mainTemplate only)");
            }
        }

        /// <summary>
        /// Ensures a Gradle template file exists. If missing, copies from Unity's defaults.
        /// </summary>
        private void EnsureGradleTemplate(string templateFileName, string targetDir)
        {
            string targetPath = Path.Combine(targetDir, templateFileName);

            // Already exists — nothing to do
            if (File.Exists(targetPath))
                return;

            // Find Unity's default template
            string unityDefaultDir = Path.Combine(
                EditorApplication.applicationContentsPath,
                "PlaybackEngines/AndroidPlayer/Tools/GradleTemplates");
            string defaultPath = Path.Combine(unityDefaultDir, templateFileName);

            if (!File.Exists(defaultPath))
            {
                Debug.LogWarning($"[GamesServices] Unity default template not found: {defaultPath}");
                return;
            }

            // Create target directory if needed
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Copy default template
            File.Copy(defaultPath, targetPath);
            Debug.Log($"[GamesServices] Created {templateFileName} from Unity defaults");

            // Sync the EditorUserBuildSettings flag
            if (templateFileName == "mainTemplate.gradle")
                EditorUserBuildSettings.SetPlatformSettings("Android", "customMainGradleTemplate", "true");
            else if (templateFileName == "settingsTemplate.gradle")
                EditorUserBuildSettings.SetPlatformSettings("Android", "customSettingsGradleTemplate", "true");

            // Refresh so Unity picks up the new file
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private void InjectRepositoriesToSettings(string path)
        {
            if (!File.Exists(path)) return;

            string content = File.ReadAllText(path);

            if (content.Contains("google()")) return;

            if (content.Contains("dependencyResolutionManagement"))
            {
                content = content.Replace(
                    "dependencyResolutionManagement {",
                    "dependencyResolutionManagement {\n    repositories {\n" +
                    GoogleRepoLine + "\n" +
                    MavenCentralLine + "\n" +
                    "    }"
                );
            }
            else
            {
                content += "\ndependencyResolutionManagement {\n    repositories {\n" +
                           GoogleRepoLine + "\n" +
                           MavenCentralLine + "\n" +
                           "    }\n}\n";
            }

            File.WriteAllText(path, content);
        }

        private void InjectDependenciesToMain(string path)
        {
            if (!File.Exists(path)) return;

            string content = File.ReadAllText(path);

            if (content.Contains("play-services-games-v2")) return;

            if (content.Contains("dependencies {"))
            {
                content = content.Replace(
                    "dependencies {",
                    "dependencies {\n" + DependencyLine
                );
            }
            else
            {
                content += "\ndependencies {\n" + DependencyLine + "\n}\n";
            }

            File.WriteAllText(path, content);
        }

        private void InjectDependencies(string path)
        {
            if (!File.Exists(path)) return;

            string content = File.ReadAllText(path);

            // Inject repositories
            if (!content.Contains("google()") && content.Contains("allprojects {"))
            {
                content = content.Replace(
                    "allprojects {",
                    "allprojects {\n    repositories {\n" +
                    GoogleRepoLine + "\n" +
                    MavenCentralLine + "\n" +
                    "    }"
                );
            }

            // Inject dependency
            if (!content.Contains("play-services-games-v2") && content.Contains("dependencies {"))
            {
                content = content.Replace(
                    "dependencies {",
                    "dependencies {\n" + DependencyLine
                );
            }

            File.WriteAllText(path, content);
        }

        private bool IsUnity2022OrNewer()
        {
            return Application.unityVersion.StartsWith("2022") ||
                   Application.unityVersion.StartsWith("2023") ||
                   Application.unityVersion.StartsWith("6000");
        }
    }
}
