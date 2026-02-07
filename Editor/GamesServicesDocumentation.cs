// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace BizSim.GPlay.Games.Editor
{
    /// <summary>
    /// Documentation window for Google Play Games Services.
    /// Provides quick access to development plans, API docs, and sample code.
    /// </summary>
    public class GamesServicesDocumentation : EditorWindow
    {
        private const string MENU_PATH = "BizSim/Google Play/Games Services/Documentation";
        private const string WINDOW_TITLE = "Google Play Games - Documentation";

        private Vector2 scrollPosition;

        [MenuItem(MENU_PATH, false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<GamesServicesDocumentation>(false, WINDOW_TITLE, true);
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(15);

            DrawDevelopmentPlanSection();
            EditorGUILayout.Space(15);

            DrawAPIDocumentationSection();
            EditorGUILayout.Space(15);

            DrawComplianceSection();
            EditorGUILayout.Space(15);

            DrawQuickStartSection();
            EditorGUILayout.Space(15);

            DrawResourcesSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("BizSim Google Play Games Services", headerStyle);
            EditorGUILayout.LabelField("Package Documentation", EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "This package provides a modern, ISocialPlatform-independent wrapper for Google Play Games Services v2.",
                MessageType.Info);
        }

        private void DrawDevelopmentPlanSection()
        {
            EditorGUILayout.LabelField("Development Plan", EditorStyles.boldLabel);

            DrawDocLink("00 - Index",
                "Quick reference and success criteria",
                "D:/Projects/CeanDooStudios/JunkyardTycoon/docs/development-plans/google-play-games/00-INDEX.md");

            DrawDocLink("01 - Overview & Problem",
                "Why we need this package (deprecated plugin, PGS v2 migration)",
                "D:/Projects/CeanDooStudios/JunkyardTycoon/docs/development-plans/google-play-games/01-overview-and-problem.md");

            DrawDocLink("02 - Architecture & Design",
                "Service interfaces, JNI bridge, error handling",
                "D:/Projects/CeanDooStudios/JunkyardTycoon/docs/development-plans/google-play-games/02-architecture-and-design.md");

            DrawDocLink("03 - Implementation Phases",
                "Phase 1-6 breakdown with code examples",
                "D:/Projects/CeanDooStudios/JunkyardTycoon/docs/development-plans/google-play-games/03-implementation-phases.md");

            DrawDocLink("04 - Migration & Integration",
                "How to migrate from old plugin",
                "D:/Projects/CeanDooStudios/JunkyardTycoon/docs/development-plans/google-play-games/04-migration-and-integration.md");

            DrawDocLink("05 - Risks & References",
                "Known risks, mitigation strategies, external links",
                "D:/Projects/CeanDooStudios/JunkyardTycoon/docs/development-plans/google-play-games/05-risks-and-references.md");
        }

        private void DrawAPIDocumentationSection()
        {
            EditorGUILayout.LabelField("API Documentation", EditorStyles.boldLabel);

            if (GUILayout.Button("Phase 1: Authentication (IGamesAuthProvider)", GUILayout.Height(25)))
            {
                EditorUtility.DisplayDialog("Authentication API",
                    "IGamesAuthProvider - Authentication Service\n\n" +
                    "Methods:\n" +
                    "• Task<GamesPlayer> AuthenticateAsync(CancellationToken)\n" +
                    "• Task<string> RequestServerSideAccessAsync(string serverClientId, bool forceRefresh, CancellationToken)\n\n" +
                    "Events:\n" +
                    "• OnAuthenticationSuccess(GamesPlayer)\n" +
                    "• OnAuthenticationFailed(GamesAuthError)\n\n" +
                    "See: Runtime/Auth/",
                    "OK");
            }

            if (GUILayout.Button("Phase 2: Achievements (IGamesAchievementProvider)", GUILayout.Height(25)))
            {
                EditorUtility.DisplayDialog("Achievements API",
                    "IGamesAchievementProvider - Achievements Service\n\n" +
                    "Methods:\n" +
                    "• Task UnlockAchievementAsync(string achievementId)\n" +
                    "• Task IncrementAchievementAsync(string achievementId, int steps)\n" +
                    "• Task RevealAchievementAsync(string achievementId)\n" +
                    "• Task ShowAchievementsUIAsync()\n" +
                    "• Task<List<GamesAchievement>> LoadAchievementsAsync(bool forceReload)\n" +
                    "• Task UnlockMultipleAsync(List<string> achievementIds)\n\n" +
                    "Events:\n" +
                    "• OnAchievementUnlocked(string achievementId)\n" +
                    "• OnAchievementIncremented(string achievementId, int currentSteps)\n" +
                    "• OnAchievementRevealed(string achievementId)\n" +
                    "• OnAchievementError(GamesAchievementError)\n\n" +
                    "See: Runtime/Achievements/",
                    "OK");
            }

            if (GUILayout.Button("Phase 3: Leaderboards (IGamesLeaderboardProvider)", GUILayout.Height(25)))
            {
                EditorUtility.DisplayDialog("Leaderboards API",
                    "IGamesLeaderboardProvider - Leaderboards Service\n\n" +
                    "Methods:\n" +
                    "• Task SubmitScoreAsync(string leaderboardId, long score, string scoreTag = null)\n" +
                    "• Task ShowLeaderboardUIAsync(string leaderboardId)\n" +
                    "• Task ShowAllLeaderboardsUIAsync()\n" +
                    "• Task<List<GamesLeaderboardEntry>> LoadTopScoresAsync(string leaderboardId, LeaderboardTimeSpan timeSpan, LeaderboardCollection collection, int maxResults)\n" +
                    "• Task<List<GamesLeaderboardEntry>> LoadPlayerCenteredScoresAsync(...)\n\n" +
                    "Events:\n" +
                    "• OnScoreSubmitted(string leaderboardId, long score)\n" +
                    "• OnScoresLoaded(string leaderboardId, List<GamesLeaderboardEntry> scores)\n" +
                    "• OnLeaderboardError(GamesLeaderboardError)\n\n" +
                    "See: Runtime/Leaderboards/",
                    "OK");
            }

            if (GUILayout.Button("Phase 4: Cloud Save (IGamesCloudSaveProvider)", GUILayout.Height(25)))
            {
                EditorUtility.DisplayDialog("Cloud Save API",
                    "IGamesCloudSaveProvider - Saved Games Service\n\n" +
                    "Transaction API:\n" +
                    "• Task<SnapshotHandle> OpenSnapshotAsync(string filename, bool createIfNotFound)\n" +
                    "• Task<byte[]> ReadSnapshotAsync(SnapshotHandle handle)\n" +
                    "• Task CommitSnapshotAsync(SnapshotHandle handle, byte[] data, string description, long playedTimeMillis, byte[] coverImage)\n" +
                    "• Task DeleteSnapshotAsync(string filename)\n\n" +
                    "Convenience API:\n" +
                    "• Task SaveAsync(string filename, byte[] data, string description)\n" +
                    "• Task<byte[]> LoadAsync(string filename)\n\n" +
                    "Events:\n" +
                    "• OnSnapshotOpened(SnapshotHandle)\n" +
                    "• OnSnapshotCommitted(string filename)\n" +
                    "• OnConflictDetected(SavedGameConflict) - IMPORTANT: Call conflict.ResolveAsync() within 60s\n" +
                    "• OnCloudSaveError(GamesCloudSaveError)\n\n" +
                    "⚠️ REQUIRED METADATA: Cover image, description, timestamp (Quality Checklist 6.1)\n\n" +
                    "See: Runtime/CloudSave/",
                    "OK");
            }

            if (GUILayout.Button("Phase 5: Player Stats (IGamesStatsProvider)", GUILayout.Height(25)))
            {
                EditorUtility.DisplayDialog("Player Stats API",
                    "IGamesStatsProvider - Player Statistics Service\n\n" +
                    "Methods:\n" +
                    "• Task<GamesPlayerStats> LoadPlayerStatsAsync(bool forceReload)\n\n" +
                    "Events:\n" +
                    "• OnStatsLoaded(GamesPlayerStats)\n" +
                    "• OnStatsError(GamesStatsError)\n\n" +
                    "GamesPlayerStats fields:\n" +
                    "• avgSessionLengthMinutes\n" +
                    "• daysSinceLastPlayed\n" +
                    "• numberOfPurchases\n" +
                    "• numberOfSessions\n" +
                    "• sessionPercentile\n" +
                    "• spendPercentile\n" +
                    "• churnProbability\n" +
                    "• highSpenderProbability\n\n" +
                    "See: Runtime/Stats/",
                    "OK");
            }
        }

        private void DrawComplianceSection()
        {
            EditorGUILayout.LabelField("⚠️ Compliance & Policies (REQUIRED)", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Google Play Games Services has mandatory requirements for authentication, achievements, saved games, and data handling. " +
                "Failure to comply may result in app rejection or removal from Google Play.",
                MessageType.Warning);

            // Quality Checklist
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📋 Quality Checklist (REQUIRED)", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("• Authentication: Provide manual sign-in button if auto-auth fails", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Achievements: 10+ visible, 4+ achievable in 1 hour, unique names/icons/descriptions", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Achievement Icons: 512x512 PNG/JPEG on transparent background", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Saved Games: MUST include cover image, description, timestamp (see Phase 4 API)", EditorStyles.wordWrappedMiniLabel);
            if (GUILayout.Button("🔗 Full Quality Checklist", GUILayout.Height(22)))
            {
                Application.OpenURL("https://developer.android.com/games/pgs/quality");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Branding Guidelines
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🎨 Branding Guidelines (REQUIRED)", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("• Use Google Play game controller icon for all Games Services UI entry points", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Icon colors: Green (active), Gray (neutral), White (light backgrounds)", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• NEVER suppress pop-ups (welcome back, achievement unlock)", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• NEVER include 'Google', 'Google Play', or 'Google Play Games' in your game TITLE", EditorStyles.wordWrappedMiniLabel);
            if (GUILayout.Button("🔗 Branding Guidelines & Icon Downloads", GUILayout.Height(22)))
            {
                Application.OpenURL("https://developer.android.com/games/pgs/branding");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Data Collection
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔒 Data Collection & Privacy (REQUIRED)", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("• Friends Data: 30-DAY MAXIMUM retention - must delete or refresh after 30 days", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Friends Data: ONLY for displaying friends list UI - NOT for analytics/advertising", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Complete Google Play Data Safety form accurately", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Disclose all collected data types (achievements, scores, friends, analytics)", EditorStyles.wordWrappedMiniLabel);
            if (GUILayout.Button("🔗 Data Collection Policies", GUILayout.Height(22)))
            {
                Application.OpenURL("https://developer.android.com/games/pgs/data-collection");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Terms of Service
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⚖️ Terms of Service (REQUIRED)", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("• NEVER submit false gameplay data (invalid scores, unearned achievements)", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• NEVER send multiplayer invites/gifts without explicit user approval", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• NEVER use player data for advertising purposes", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• NEVER share friends data with third parties", EditorStyles.wordWrappedMiniLabel);
            if (GUILayout.Button("🔗 Terms of Service", GUILayout.Height(22)))
            {
                Application.OpenURL("https://developer.android.com/games/pgs/terms");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Critical Warning
            EditorGUILayout.HelpBox(
                "🚨 CRITICAL: Phase 6 (Friends) - When implementing Friends Service, you MUST:\n" +
                "1. Delete friends data after 30 days OR refresh via new API calls\n" +
                "2. Use friends data ONLY for in-game friends list UI display\n" +
                "3. NEVER share, sell, or use for analytics/marketing",
                MessageType.Error);
        }

        private void DrawQuickStartSection()
        {
            EditorGUILayout.LabelField("Quick Start", EditorStyles.boldLabel);

            var codeStyle = new GUIStyle(EditorStyles.textArea)
            {
                font = EditorStyles.standardFont,
                wordWrap = false
            };

            EditorGUILayout.LabelField("1. Initialize in your MonoBehaviour:", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(
                "void Start()\n" +
                "{\n" +
                "    GamesServicesManager.Initialize();\n" +
                "    GamesServicesManager.Auth.OnAuthenticationSuccess += OnAuthSuccess;\n" +
                "    GamesServicesManager.Auth.OnAuthenticationFailed += OnAuthFailed;\n" +
                "}",
                codeStyle, GUILayout.Height(80));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("2. Authenticate user:", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(
                "async void AuthenticateUser()\n" +
                "{\n" +
                "    try {\n" +
                "        var player = await GamesServicesManager.Auth.AuthenticateAsync();\n" +
                "        Debug.Log($\"Authenticated: {player.displayName}\");\n" +
                "    } catch (Exception ex) {\n" +
                "        Debug.LogError($\"Auth failed: {ex.Message}\");\n" +
                "    }\n" +
                "}",
                codeStyle, GUILayout.Height(100));
        }

        private void DrawResourcesSection()
        {
            EditorGUILayout.LabelField("External Resources", EditorStyles.boldLabel);

            DrawWebLink("Google Play Games Services Documentation",
                "https://developers.google.com/games/services");

            DrawWebLink("PGS v2 SDK Reference",
                "https://developers.google.com/android/reference/com/google/android/gms/games/v2/package-summary");

            DrawWebLink("Unity Package on GitHub (deprecated reference)",
                "https://github.com/playgameservices/play-games-plugin-for-unity");
        }

        private void DrawDocLink(string title, string description, string filePath)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("📄", GUILayout.Width(30), GUILayout.Height(25)))
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start(filePath);
                }
                else
                {
                    EditorUtility.DisplayDialog("File Not Found",
                        $"Documentation file not found:\n{filePath}",
                        "OK");
                }
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        private void DrawWebLink(string title, string url)
        {
            if (GUILayout.Button($"🔗 {title}", GUILayout.Height(25)))
            {
                Application.OpenURL(url);
            }
        }
    }
}
