// Copyright (c) BizSim Game Studios. All rights reserved.

using BizSim.GPlay.EditorCore;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BizSim.GPlay.Games.Editor
{
    /// <summary>
    /// Wizard-style setup window for Google Play Games Services configuration.
    /// Accessible via: BizSim > Google Play > Games Services > Setup > Android Setup
    /// </summary>
    public class GamesServicesSetupWindow : EditorWindow
    {
        private const string MENU_PATH = "BizSim/Google Play/Games Services/Setup/Android Setup";
        private const string WINDOW_TITLE = "Google Play Games - Setup Wizard";
        private const string RESOURCES_PATH = "Assets/Plugins/Android/GooglePlayGamesManifest.androidlib/res/values/games-ids.xml";

        private enum SetupStep
        {
            Welcome = 0,
            Instructions = 1,
            PasteXML = 2,
            Verification = 3,
            OptionalSettings = 4,
            Complete = 5
        }

        // Wizard state
        private SetupStep currentStep = SetupStep.Welcome;

        // Configuration fields
        private string packageName = "";
        private string appId = "";
        private string webClientId = "";
        private string resourcesXml = "";

        private Vector2 scrollPosition;
        private bool autoDetectedPackage = false;
        private bool xmlParsed = false;

        [MenuItem(MENU_PATH, false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<GamesServicesSetupWindow>(true, WINDOW_TITLE, true);
            window.minSize = new Vector2(650, 550);
            window.maxSize = new Vector2(800, 700);
            window.Show();
        }

        private void OnEnable()
        {
            // Auto-detect package name from PlayerSettings
            packageName = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            autoDetectedPackage = !string.IsNullOrEmpty(packageName);

            // Try to load existing configuration
            LoadExistingConfig();
        }

        private void OnGUI()
        {
            DrawProgressBar();
            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (currentStep)
            {
                case SetupStep.Welcome:
                    DrawWelcomeStep();
                    break;
                case SetupStep.Instructions:
                    DrawInstructionsStep();
                    break;
                case SetupStep.PasteXML:
                    DrawPasteXMLStep();
                    break;
                case SetupStep.Verification:
                    DrawVerificationStep();
                    break;
                case SetupStep.OptionalSettings:
                    DrawOptionalSettingsStep();
                    break;
                case SetupStep.Complete:
                    DrawCompleteStep();
                    break;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            DrawNavigationButtons();
        }

        private void DrawProgressBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            int totalSteps = 6;
            int currentStepIndex = (int)currentStep + 1;

            GUILayout.Label($"Step {currentStepIndex} of {totalSteps}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            // Progress indicator
            string[] stepNames = { "Welcome", "Instructions", "Paste XML", "Verify", "Optional", "Complete" };
            GUILayout.Label(stepNames[(int)currentStep], EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();

            // Progress bar
            Rect progressRect = GUILayoutUtility.GetRect(18, 6);
            float progress = (float)currentStepIndex / totalSteps;
            EditorGUI.ProgressBar(progressRect, progress, "");
        }

        #region Step: Welcome

        private void DrawWelcomeStep()
        {
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("🎮 Google Play Games Services", titleStyle, GUILayout.Height(30));
            EditorGUILayout.LabelField("Configuration Wizard", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "This wizard will guide you through the configuration process for Google Play Games Services.\n\n" +
                "You will need:\n" +
                "• Access to Google Play Console\n" +
                "• Your game configured with Play Games Services\n" +
                "• At least 1 Achievement or Leaderboard created\n" +
                "• OAuth 2.0 credentials configured\n\n" +
                "The setup process takes approximately 5 minutes.",
                MessageType.Info);

            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Package Information", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            GUI.enabled = false;
            EditorGUILayout.TextField("Android Package Name", packageName);
            GUI.enabled = true;

            if (autoDetectedPackage)
            {
                EditorGUILayout.LabelField("✓ Auto-detected from Player Settings", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Package name not found. Please set it in:\n" +
                    "Edit → Project Settings → Player → Android → Identification",
                    MessageType.Warning);
            }
        }

        #endregion

        #region Step: Instructions

        private void DrawInstructionsStep()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

            EditorGUILayout.LabelField("📋 Google Play Console Setup Instructions", headerStyle);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Follow these steps in Google Play Console to prepare your game configuration:",
                MessageType.Info);

            EditorGUILayout.Space(10);

            DrawInstructionStep("1", "Open Google Play Console",
                "Click the button below to open Google Play Console in your browser.");

            if (GUILayout.Button("🌐 Open Play Games Console", GUILayout.Height(30)))
            {
                Application.OpenURL("https://play.google.com/console");
            }

            EditorGUILayout.Space(10);

            DrawInstructionStep("2", "Select Your Game",
                "From the list of apps, select your game (Junkyard Tycoon).");

            EditorGUILayout.Space(5);

            DrawInstructionStep("3", "Navigate to Play Games Services",
                "In the left sidebar:\n" +
                "• Click \"Grow users\" section\n" +
                "• Select \"Play Games Services\"");

            EditorGUILayout.Space(5);

            DrawInstructionStep("4", "Open Configuration",
                "In the left menu:\n" +
                "• Expand \"Setup and management\"\n" +
                "• Click \"Configuration\"");

            EditorGUILayout.Space(5);

            DrawInstructionStep("5", "Verify Prerequisites",
                "Ensure you have configured:\n" +
                "✓ OAuth 2.0 Credentials (under \"Credentials\" tab)\n" +
                "✓ At least 1 Achievement OR 1 Leaderboard");

            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "⚠️ Important: If you haven't added Credentials or at least one Achievement/Leaderboard, " +
                "the \"Get resources\" link will not be available in the next step.",
                MessageType.Warning);
        }

        private void DrawInstructionStep(string number, string title, string description)
        {
            EditorGUILayout.BeginHorizontal();

            // Number badge
            var numberStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedWidth = 30,
                fixedHeight = 30,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Box(number, numberStyle);

            // Content
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Step: Paste XML

        private void DrawPasteXMLStep()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

            EditorGUILayout.LabelField("📄 Get Resources XML", headerStyle);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Now you need to copy the Android Resources XML from Google Play Console.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            DrawInstructionStep("1", "Click \"Get resources\"",
                "In the Configuration page, find and click the \"Get resources\" link.\n" +
                "This will open a popup window with XML content.");

            EditorGUILayout.Space(5);

            DrawInstructionStep("2", "Copy XML Content",
                "In the popup window:\n" +
                "• Click the \"Copy\" button in the top-right corner\n" +
                "• The entire XML content will be copied to clipboard");

            EditorGUILayout.Space(5);

            DrawInstructionStep("3", "Paste XML Below",
                "Paste the copied XML into the text area below and click \"Parse XML\".");

            EditorGUILayout.Space(10);

            // XML paste area
            EditorGUILayout.LabelField("XML Content (from Play Console):", EditorStyles.boldLabel);

            var textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = false,
                font = EditorStyles.standardFont
            };

            resourcesXml = EditorGUILayout.TextArea(resourcesXml, textAreaStyle, GUILayout.Height(180));

            EditorGUILayout.Space(5);

            // Parse button
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("🔍 Parse XML", GUILayout.Height(30)))
            {
                ParseResourcesXml();
            }
            GUI.backgroundColor = Color.white;

            // Show parse result
            if (xmlParsed && !string.IsNullOrEmpty(appId))
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox($"✓ Successfully parsed!\nApp ID: {appId}", MessageType.Info);
            }
        }

        #endregion

        #region Step: Verification

        private void DrawVerificationStep()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

            EditorGUILayout.LabelField("✓ Verification", headerStyle);
            EditorGUILayout.Space(10);

            if (!xmlParsed || string.IsNullOrEmpty(appId))
            {
                EditorGUILayout.HelpBox(
                    "⚠️ XML not parsed yet.\n\nPlease go back to the previous step and parse your XML.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox(
                "Configuration successfully parsed! Please verify the information below.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Package info
            EditorGUILayout.LabelField("📦 Package Information", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUI.enabled = false;
            EditorGUILayout.TextField("Package Name", packageName);
            EditorGUILayout.TextField("Play Games App ID", appId);
            GUI.enabled = true;

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Preview XML stats
            EditorGUILayout.LabelField("📊 Resources Summary", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int achievementCount = CountXmlElements("achievement_");
            int leaderboardCount = CountXmlElements("leaderboard_");

            EditorGUILayout.LabelField($"Achievements: {achievementCount}");
            EditorGUILayout.LabelField($"Leaderboards: {leaderboardCount}");

            EditorGUILayout.EndVertical();

            if (achievementCount == 0 && leaderboardCount == 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    "⚠️ No Achievements or Leaderboards found in XML.\n" +
                    "You can still proceed, but consider adding them in Play Console.",
                    MessageType.Warning);
            }
        }

        #endregion

        #region Step: Optional Settings

        private void DrawOptionalSettingsStep()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

            EditorGUILayout.LabelField("⚙️ Optional Settings", headerStyle);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "These settings are optional. You can skip this step if you don't need them.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Firebase Analytics Integration
            DrawFirebaseIntegrationSection();

            EditorGUILayout.Space(15);

            // Web Client ID
            EditorGUILayout.LabelField("Web App Client ID", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The web app client ID is needed to access the user's ID token and call other APIs on behalf of the user.\n\n" +
                "Required for: ServerSideAccess (backend authentication)\n\n" +
                "To get this:\n" +
                "1. Go to Play Console → Configuration → Credentials\n" +
                "2. Create a new OAuth 2.0 client (Web type)\n" +
                "3. Copy the Client ID\n\n" +
                "Example format: 123456789012-abcdefghijklm.apps.googleusercontent.com",
                MessageType.None);

            EditorGUILayout.Space(5);
            webClientId = EditorGUILayout.TextField("Client ID (Optional)", webClientId);

            if (!string.IsNullOrEmpty(webClientId))
            {
                EditorGUILayout.Space(5);
                if (webClientId.Contains(".apps.googleusercontent.com"))
                {
                    EditorGUILayout.LabelField("✓ Valid format", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "⚠️ Client ID format looks incorrect. It should end with .apps.googleusercontent.com",
                        MessageType.Warning);
                }
            }
        }

        private void DrawFirebaseIntegrationSection()
        {
            EditorGUILayout.LabelField("📦 Firebase Analytics Integration", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Package status
            bool packageInstalled = BizSimDefineManager.IsFirebaseAnalyticsInstalled();
            string version = BizSimDefineManager.GetFirebaseAnalyticsVersion();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Package:", GUILayout.Width(90));
            if (packageInstalled)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField($"✓ Installed (v{version})", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(1f, 0.5f, 0f);
                EditorGUILayout.LabelField("✗ Not Found", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            // Define status
            bool definePresent = BizSimDefineManager.IsFirebaseDefinePresentAnywhere();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Define:", GUILayout.Width(90));
            if (definePresent)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("✓ BIZSIM_FIREBASE", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = new Color(1f, 0.5f, 0f);
                EditorGUILayout.LabelField("✗ Not Active", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Status message
            MessageType messageType;
            string statusMessage = BizSimDefineManager.GetFirebaseStatusMessage(out messageType);

            if (messageType != MessageType.None)
            {
                EditorGUILayout.HelpBox(statusMessage, messageType);
                EditorGUILayout.Space(5);
            }

            // Buttons
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = packageInstalled && !definePresent;
            if (GUILayout.Button("Enable Firebase", GUILayout.Height(25)))
            {
                BizSimDefineManager.AddFirebaseDefineAllPlatforms();
                Debug.Log("[Games Services] Firebase Analytics integration enabled.");
            }
            GUI.enabled = true;

            GUI.enabled = definePresent;
            if (GUILayout.Button("Disable Firebase", GUILayout.Height(25)))
            {
                BizSimDefineManager.RemoveFirebaseDefineAllPlatforms();
                Debug.Log("[Games Services] Firebase Analytics integration disabled.");
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Step: Complete

        private void DrawCompleteStep()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

            EditorGUILayout.LabelField("🎉 Ready to Complete Setup", headerStyle);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "All configuration is ready! Click \"Complete Setup\" to finalize.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Summary
            EditorGUILayout.LabelField("📋 Configuration Summary", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Package Name:", packageName);
            EditorGUILayout.LabelField("App ID:", appId);
            if (!string.IsNullOrEmpty(webClientId))
            {
                EditorGUILayout.LabelField("Web Client ID:", webClientId);
            }

            int achievementCount = CountXmlElements("achievement_");
            int leaderboardCount = CountXmlElements("leaderboard_");
            EditorGUILayout.LabelField($"Achievements: {achievementCount}");
            EditorGUILayout.LabelField($"Leaderboards: {leaderboardCount}");

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("📁 Files to be Created/Updated", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"• {RESOURCES_PATH}");
            EditorGUILayout.LabelField($"• {GPGS_IDS_PATH} (C# constants)");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "After setup:\n" +
                "1. Build your Android app\n" +
                "2. Test authentication with GamesServicesManager.Auth.AuthenticateAsync()\n" +
                "3. Check Documentation for Achievements, Leaderboards, SavedGames implementation",
                MessageType.None);
        }

        #endregion

        #region Navigation Buttons

        private void DrawNavigationButtons()
        {
            EditorGUILayout.BeginHorizontal();

            // Previous button
            GUI.enabled = currentStep != SetupStep.Welcome;
            if (GUILayout.Button("← Previous", GUILayout.Width(100), GUILayout.Height(30)))
            {
                currentStep--;
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            // Cancel button
            if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(30)))
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Cancel Setup",
                    "Are you sure you want to cancel the setup?",
                    "Yes, Cancel",
                    "No, Continue");

                if (confirm)
                {
                    Close();
                }
            }

            GUILayout.Space(10);

            // Next / Complete button
            if (currentStep == SetupStep.Complete)
            {
                // Complete Setup button (only enabled if XML parsed)
                GUI.enabled = xmlParsed && !string.IsNullOrEmpty(appId);
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("✓ Complete Setup", GUILayout.Width(130), GUILayout.Height(30)))
                {
                    PerformSetup();
                }
                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                // Next button logic
                bool canProceed = CanProceedToNextStep();
                GUI.enabled = canProceed;

                if (GUILayout.Button("Next →", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    currentStep++;
                }

                GUI.enabled = true;

                // Show warning if can't proceed
                if (!canProceed && currentStep == SetupStep.PasteXML)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox(
                        "⚠️ Please paste and parse XML before proceeding.",
                        MessageType.Warning);
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool CanProceedToNextStep()
        {
            switch (currentStep)
            {
                case SetupStep.Welcome:
                    return !string.IsNullOrEmpty(packageName);

                case SetupStep.Instructions:
                    return true; // Always can proceed from instructions

                case SetupStep.PasteXML:
                    return xmlParsed && !string.IsNullOrEmpty(appId);

                case SetupStep.Verification:
                    return xmlParsed && !string.IsNullOrEmpty(appId);

                case SetupStep.OptionalSettings:
                    return true; // Optional, always can proceed

                default:
                    return true;
            }
        }

        #endregion

        #region Helper Methods

        private void LoadExistingConfig()
        {
            if (File.Exists(RESOURCES_PATH))
            {
                try
                {
                    resourcesXml = File.ReadAllText(RESOURCES_PATH);
                    ParseResourcesXml();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[GamesServices Setup] Could not load existing config: {ex.Message}");
                }
            }
        }

        private void ParseResourcesXml()
        {
            if (string.IsNullOrWhiteSpace(resourcesXml))
            {
                EditorUtility.DisplayDialog("Parse Error", "Please paste XML content first.", "OK");
                return;
            }

            try
            {
                var doc = XDocument.Parse(resourcesXml);
                var appIdElement = doc.Root?.Element("string");

                if (appIdElement != null && appIdElement.Attribute("name")?.Value == "app_id")
                {
                    appId = appIdElement.Value;
                    xmlParsed = true;
                    Debug.Log($"[GamesServices Setup] Parsed App ID: {appId}");
                }
                else
                {
                    xmlParsed = false;
                    EditorUtility.DisplayDialog("Parse Error",
                        "Could not find app_id in XML. Make sure you pasted the correct resources XML from Play Console.",
                        "OK");
                }
            }
            catch (System.Exception ex)
            {
                xmlParsed = false;
                EditorUtility.DisplayDialog("Parse Error",
                    $"Failed to parse XML:\n\n{ex.Message}\n\nMake sure you copied the entire XML content.",
                    "OK");
            }
        }

        private int CountXmlElements(string namePrefix)
        {
            if (string.IsNullOrEmpty(resourcesXml))
                return 0;

            try
            {
                var doc = XDocument.Parse(resourcesXml);
                return doc.Descendants("string")
                    .Count(e => e.Attribute("name")?.Value.StartsWith(namePrefix) == true);
            }
            catch
            {
                return 0;
            }
        }

        private const string GPGS_IDS_PATH = "Assets/GPGSIds.cs";

        private void PerformSetup()
        {
            if (string.IsNullOrEmpty(packageName))
            {
                EditorUtility.DisplayDialog("Setup Error", "Package name is required.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(appId))
            {
                EditorUtility.DisplayDialog("Setup Error",
                    "App ID is required. Please paste and parse the resources XML first.",
                    "OK");
                return;
            }

            try
            {
                // Create Android resources directory
                string resourcesDir = Path.GetDirectoryName(RESOURCES_PATH);
                if (!Directory.Exists(resourcesDir))
                {
                    Directory.CreateDirectory(resourcesDir);
                }

                // Write games-ids.xml
                File.WriteAllText(RESOURCES_PATH, resourcesXml);
                Debug.Log($"[GamesServices Setup] Created: {RESOURCES_PATH}");

                // Generate GPGSIds.cs constants file
                GenerateGPGSIds();

                // Refresh AssetDatabase
                AssetDatabase.Refresh();

                int achievementCount = CountXmlElements("achievement_");
                int leaderboardCount = CountXmlElements("leaderboard_");

                // Show success dialog
                EditorUtility.DisplayDialog("Setup Complete!",
                    $"Google Play Games Services configured successfully!\n\n" +
                    $"Package: {packageName}\n" +
                    $"App ID: {appId}\n" +
                    (string.IsNullOrEmpty(webClientId) ? "" : $"Web Client ID: {webClientId}\n") +
                    $"Achievements: {achievementCount}\n" +
                    $"Leaderboards: {leaderboardCount}\n" +
                    $"\nGenerated files:\n" +
                    $"  {RESOURCES_PATH}\n" +
                    $"  {GPGS_IDS_PATH}\n" +
                    $"\nNext steps:\n" +
                    $"1. Build your Android app (File > Build Settings)\n" +
                    $"2. Test authentication with GamesServicesManager.Auth.AuthenticateAsync()\n" +
                    $"3. See Documentation for Achievements, Leaderboards, SavedGames",
                    "Got it!");

                Close();
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Setup Error",
                    $"Failed to write configuration:\n\n{ex.Message}",
                    "OK");
            }
        }

        private void GenerateGPGSIds()
        {
            if (string.IsNullOrEmpty(resourcesXml)) return;

            try
            {
                var doc = XDocument.Parse(resourcesXml);
                var elements = doc.Descendants("string")
                    .Where(e => e.Attribute("name") != null)
                    .ToArray();

                if (elements.Length == 0) return;

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("// <auto-generated>");
                sb.AppendLine("// This file was generated by BizSim Google Play Games Services Setup Wizard.");
                sb.AppendLine("// Do not modify manually. Re-run the setup wizard to regenerate.");
                sb.AppendLine("// </auto-generated>");
                sb.AppendLine();
                sb.AppendLine("public static class GPGSIds");
                sb.AppendLine("{");

                foreach (var element in elements)
                {
                    string name = element.Attribute("name").Value;
                    string value = element.Value.Trim();

                    // Skip app_id and package_name — not needed as C# constants
                    if (name == "app_id" || name == "package_name") continue;

                    sb.AppendLine($"        public const string {name} = \"{value}\";");
                }

                sb.AppendLine("}");

                File.WriteAllText(GPGS_IDS_PATH, sb.ToString());
                Debug.Log($"[GamesServices Setup] Generated: {GPGS_IDS_PATH}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GamesServices Setup] Could not generate GPGSIds.cs: {ex.Message}");
            }
        }

        #endregion
    }
}
