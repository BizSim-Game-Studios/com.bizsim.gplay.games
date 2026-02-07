using UnityEditor;
using UnityEngine;

namespace BizSim.GPlay.Games.Editor
{
    /// <summary>
    /// Custom inspector for GamesServicesMockConfig with card-based layout.
    /// </summary>
    [CustomEditor(typeof(GamesServicesMockConfig))]
    public class GamesServicesMockConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty authSucceeds;
        private SerializedProperty mockPlayerId;
        private SerializedProperty mockDisplayName;
        private SerializedProperty mockAuthErrorType;
        private SerializedProperty authDelaySeconds;
        private SerializedProperty mockUnlockedCount;
        private SerializedProperty mockScore;
        private SerializedProperty mockChurnProbability;

        private void OnEnable()
        {
            authSucceeds = serializedObject.FindProperty("authSucceeds");
            mockPlayerId = serializedObject.FindProperty("mockPlayerId");
            mockDisplayName = serializedObject.FindProperty("mockDisplayName");
            mockAuthErrorType = serializedObject.FindProperty("mockAuthErrorType");
            authDelaySeconds = serializedObject.FindProperty("authDelaySeconds");
            mockUnlockedCount = serializedObject.FindProperty("mockUnlockedCount");
            mockScore = serializedObject.FindProperty("mockScore");
            mockChurnProbability = serializedObject.FindProperty("mockChurnProbability");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var config = (GamesServicesMockConfig)target;

            // Header
            DrawPackageHeader();

            EditorGUILayout.Space(10);

            // Authentication Card
            DrawAuthenticationCard(config);

            EditorGUILayout.Space(5);

            // Achievements Card
            DrawAchievementsCard();

            EditorGUILayout.Space(5);

            // Leaderboards Card
            DrawLeaderboardsCard();

            EditorGUILayout.Space(5);

            // Player Stats Card
            DrawPlayerStatsCard();

            EditorGUILayout.Space(10);

            // Usage Info
            DrawUsageInfo();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPackageHeader()
        {
            EditorGUILayout.LabelField("Games Services Mock Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This mock config is used ONLY in Unity Editor for testing without real Google Play authentication. " +
                "On device, the real Google Play Games Services will be used.",
                MessageType.Info);
        }

        private void DrawAuthenticationCard(GamesServicesMockConfig config)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("🔐 Authentication", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Success toggle
            EditorGUILayout.PropertyField(authSucceeds, new GUIContent("Auth Succeeds"));

            EditorGUILayout.Space(3);

            if (authSucceeds.boolValue)
            {
                // Success path
                GUI.color = new Color(0.8f, 1f, 0.8f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = Color.white;

                EditorGUILayout.LabelField("✓ Success Scenario", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(mockPlayerId, new GUIContent("Player ID"));
                EditorGUILayout.PropertyField(mockDisplayName, new GUIContent("Display Name"));

                EditorGUILayout.EndVertical();
            }
            else
            {
                // Error path
                GUI.color = new Color(1f, 0.8f, 0.8f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = Color.white;

                EditorGUILayout.LabelField("✗ Error Scenario", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(mockAuthErrorType, new GUIContent("Error Type"));

                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Preview:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"  Code: {config.GetAuthErrorCode()}", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField($"  Message: {config.GetAuthErrorMessage()}", EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(3);
            EditorGUILayout.PropertyField(authDelaySeconds, new GUIContent("Delay (seconds)"));

            EditorGUILayout.EndVertical();
        }

        private void DrawAchievementsCard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("🏆 Achievements", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(mockUnlockedCount, new GUIContent("Unlocked Count"));

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("ℹ️ Mock unlocked achievements count for testing UI.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawLeaderboardsCard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("📊 Leaderboards", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(mockScore, new GUIContent("Mock Score"));

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("ℹ️ Mock leaderboard score for testing score submission.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawPlayerStatsCard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("📈 Player Stats", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(mockChurnProbability, new GUIContent("Churn Probability"));

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("ℹ️ Mock churn prediction for testing player stats API.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawUsageInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("📖 Usage", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("1. Create this config: Assets → Create → BizSim → Games Services Mock Config",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("2. Name it 'DefaultGamesConfig' (will auto-load from Resources/)",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("3. Enter Play Mode - GamesServicesManager will use mock data",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("4. On Android device - Real Google Play Games will be used automatically",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
        }
    }
}
