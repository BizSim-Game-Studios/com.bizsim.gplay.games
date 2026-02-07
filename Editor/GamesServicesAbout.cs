// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEditor;
using UnityEngine;
using System.IO;

namespace BizSim.GPlay.Games.Editor
{
    /// <summary>
    /// About window for Google Play Games Services package.
    /// Displays package version, author, license, and dependency information.
    /// </summary>
    public class GamesServicesAbout : EditorWindow
    {
        private const string MENU_PATH = "BizSim/Google Play/Games Services/About";
        private const string WINDOW_TITLE = "About - Google Play Games Services";
        private const string PACKAGE_JSON_PATH = "Packages/com.bizsim.gplay.games/package.json";

        private Vector2 scrollPosition;
        private string packageVersion = "0.1.0";
        private string packageDisplayName = "BizSim Google Play Games Services";
        private string packageDescription = "Modern wrapper for Google Play Games Services v2 (PGS v2 SDK)";

        [MenuItem(MENU_PATH, false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<GamesServicesAbout>(false, WINDOW_TITLE, true);
            window.minSize = new Vector2(450, 500);
            window.maxSize = new Vector2(600, 700);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPackageInfo();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(15);

            DrawPackageInfo();
            EditorGUILayout.Space(15);

            DrawAuthorInfo();
            EditorGUILayout.Space(15);

            DrawDependencies();
            EditorGUILayout.Space(15);

            DrawLicense();
            EditorGUILayout.Space(15);

            DrawImplementationStatus();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            // Logo placeholder (you can add actual logo asset later)
            var logoStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("🎮", logoStyle, GUILayout.Height(40));
            EditorGUILayout.LabelField(packageDisplayName, logoStyle);
            EditorGUILayout.LabelField($"Version {packageVersion}", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawPackageInfo()
        {
            EditorGUILayout.LabelField("Package Information", EditorStyles.boldLabel);

            DrawInfoRow("Name:", "com.bizsim.gplay.games");
            DrawInfoRow("Display Name:", packageDisplayName);
            DrawInfoRow("Version:", packageVersion);
            DrawInfoRow("Unity Version:", "6000.1+");
            DrawInfoRow("Description:", packageDescription);
        }

        private void DrawAuthorInfo()
        {
            EditorGUILayout.LabelField("Author Information", EditorStyles.boldLabel);

            DrawInfoRow("Organization:", "BizSim Game Studios");
            DrawInfoRow("Developer:", "Aşkın Ceyhan");
            DrawInfoRow("Support:", "github.com/BizSimGameStudios");
        }

        private void DrawDependencies()
        {
            EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "External Dependencies (auto-injected via Gradle):\n" +
                "• com.google.android.gms:play-services-games-v2:20.1.1",
                MessageType.Info);

            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Unity Packages:\n" +
                "• com.unity.addressables.android (recommended for Android builds)\n" +
                "• TextMeshPro (for UI rendering)",
                MessageType.Info);
        }

        private void DrawLicense()
        {
            EditorGUILayout.LabelField("License", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Copyright (c) 2025 BizSim Game Studios. All rights reserved.\n\n" +
                "This package is proprietary software developed for internal use in BizSim game projects.\n\n" +
                "Redistribution and use in source and binary forms, with or without modification, " +
                "are permitted exclusively for BizSim Game Studios projects.",
                MessageType.None);
        }

        private void DrawImplementationStatus()
        {
            EditorGUILayout.LabelField("Implementation Status", EditorStyles.boldLabel);

            DrawStatusRow("✅ Phase 1: Authentication", true);
            DrawStatusRow("✅ Phase 2: Achievements", true);
            DrawStatusRow("✅ Phase 3: Leaderboards", true);
            DrawStatusRow("✅ Phase 4: Cloud Save (Saved Games)", true);
            DrawStatusRow("✅ Phase 5: Player Stats", true);
            DrawStatusRow("⏳ Phase 6: Friends & Events (Future)", false);

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "✅ v1.0.0 COMPLETE - All core features implemented!\n\n" +
                "Implemented Features:\n" +
                "• Authentication (silent + manual, server-side access)\n" +
                "• Achievements (unlock, increment, reveal, batch operations)\n" +
                "• Leaderboards (submit scores, load rankings, scoretags)\n" +
                "• Cloud Save (transaction-based with conflict resolution)\n" +
                "• Player Stats (churn prediction, engagement metrics)\n\n" +
                "Next: Phase 6 (Friends & Events) - Planned for future release",
                MessageType.Info);
        }

        private void DrawInfoRow(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.LabelField(value, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusRow(string label, bool isComplete)
        {
            var statusColor = isComplete ? Color.green : Color.gray;
            var prevColor = GUI.contentColor;
            GUI.contentColor = statusColor;

            EditorGUILayout.LabelField(label, EditorStyles.label);

            GUI.contentColor = prevColor;
        }

        private void LoadPackageInfo()
        {
            try
            {
                if (File.Exists(PACKAGE_JSON_PATH))
                {
                    string json = File.ReadAllText(PACKAGE_JSON_PATH);

                    // Simple JSON parsing (avoid JsonUtility for editor-only code)
                    if (json.Contains("\"version\""))
                    {
                        int versionStart = json.IndexOf("\"version\"") + 11;
                        int versionEnd = json.IndexOf("\"", versionStart);
                        packageVersion = json.Substring(versionStart, versionEnd - versionStart);
                    }

                    if (json.Contains("\"displayName\""))
                    {
                        int nameStart = json.IndexOf("\"displayName\"") + 15;
                        int nameEnd = json.IndexOf("\"", nameStart);
                        packageDisplayName = json.Substring(nameStart, nameEnd - nameStart);
                    }

                    if (json.Contains("\"description\""))
                    {
                        int descStart = json.IndexOf("\"description\"") + 15;
                        int descEnd = json.IndexOf("\"", descStart);
                        packageDescription = json.Substring(descStart, descEnd - descStart);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GamesServices About] Could not load package.json: {ex.Message}");
            }
        }
    }
}
