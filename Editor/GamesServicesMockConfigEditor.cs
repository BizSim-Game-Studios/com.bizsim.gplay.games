// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace BizSim.GPlay.Games.Editor
{
    [CustomEditor(typeof(GamesServicesMockConfig))]
    public class GamesServicesMockConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty authSucceeds;
        private SerializedProperty mockPlayerId;
        private SerializedProperty mockDisplayName;
        private SerializedProperty mockAuthErrorType;
        private SerializedProperty mockConsentGranted;
        private SerializedProperty mockEmail;
        private SerializedProperty mockEmailVerified;
        private SerializedProperty mockFullName;
        private SerializedProperty mockGivenName;
        private SerializedProperty mockFamilyName;
        private SerializedProperty mockPictureUrl;
        private SerializedProperty mockLocale;
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
            mockConsentGranted = serializedObject.FindProperty("mockConsentGranted");
            mockEmail = serializedObject.FindProperty("mockEmail");
            mockEmailVerified = serializedObject.FindProperty("mockEmailVerified");
            mockFullName = serializedObject.FindProperty("mockFullName");
            mockGivenName = serializedObject.FindProperty("mockGivenName");
            mockFamilyName = serializedObject.FindProperty("mockFamilyName");
            mockPictureUrl = serializedObject.FindProperty("mockPictureUrl");
            mockLocale = serializedObject.FindProperty("mockLocale");
            authDelaySeconds = serializedObject.FindProperty("authDelaySeconds");
            mockUnlockedCount = serializedObject.FindProperty("mockUnlockedCount");
            mockScore = serializedObject.FindProperty("mockScore");
            mockChurnProbability = serializedObject.FindProperty("mockChurnProbability");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var config = (GamesServicesMockConfig)target;

            DrawPackageHeader();
            EditorGUILayout.Space(10);

            DrawAuthenticationCard(config);
            EditorGUILayout.Space(5);

            DrawProfileCard();
            EditorGUILayout.Space(5);

            DrawAchievementsCard();
            EditorGUILayout.Space(5);

            DrawLeaderboardsCard();
            EditorGUILayout.Space(5);

            DrawPlayerStatsCard();
            EditorGUILayout.Space(10);

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

            GUILayout.Label("Authentication", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(authSucceeds, new GUIContent("Auth Succeeds"));
            EditorGUILayout.Space(3);

            if (authSucceeds.boolValue)
            {
                GUI.color = new Color(0.8f, 1f, 0.8f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = Color.white;

                EditorGUILayout.LabelField("Success Scenario", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(mockPlayerId, new GUIContent("Player ID"));
                EditorGUILayout.PropertyField(mockDisplayName, new GUIContent("Display Name"));

                EditorGUILayout.EndVertical();
            }
            else
            {
                GUI.color = new Color(1f, 0.8f, 0.8f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = Color.white;

                EditorGUILayout.LabelField("Error Scenario", EditorStyles.miniLabel);
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

        private void DrawProfileCard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Profile (ID Token Claims)", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            EditorGUILayout.HelpBox(
                "Simulates the decoded JWT ID Token payload returned after " +
                "server-side auth code exchange. Claims are scope-dependent:\n" +
                "  EMAIL scope: email, emailVerified\n" +
                "  PROFILE scope: name, picture, locale\n" +
                "  OPEN_ID scope: sub (always = Player ID)",
                MessageType.Info);

            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(mockConsentGranted, new GUIContent("Consent Granted"));

            if (mockConsentGranted.boolValue)
            {
                GUI.color = new Color(0.8f, 1f, 0.8f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = Color.white;

                EditorGUILayout.LabelField("EMAIL scope claims", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(mockEmail, new GUIContent("Email"));
                EditorGUILayout.PropertyField(mockEmailVerified, new GUIContent("Email Verified"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("PROFILE scope claims", EditorStyles.miniLabel);
                EditorGUILayout.PropertyField(mockFullName, new GUIContent("Full Name"));
                EditorGUILayout.PropertyField(mockGivenName, new GUIContent("Given Name"));
                EditorGUILayout.PropertyField(mockFamilyName, new GUIContent("Family Name"));
                EditorGUILayout.PropertyField(mockPictureUrl, new GUIContent("Picture URL"));
                EditorGUILayout.PropertyField(mockLocale, new GUIContent("Locale"));

                EditorGUILayout.EndVertical();
            }
            else
            {
                GUI.color = new Color(1f, 0.9f, 0.7f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = Color.white;

                EditorGUILayout.LabelField(
                    "Consent declined: auth code will still be returned, " +
                    "but grantedScopes will be empty and IdTokenClaims will be null.",
                    EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAchievementsCard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Achievements", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(mockUnlockedCount, new GUIContent("Unlocked Count"));

            EditorGUILayout.EndVertical();
        }

        private void DrawLeaderboardsCard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Leaderboards", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(mockScore, new GUIContent("Mock Score"));

            EditorGUILayout.EndVertical();
        }

        private void DrawPlayerStatsCard()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Player Stats", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.PropertyField(mockChurnProbability, new GUIContent("Churn Probability"));

            EditorGUILayout.EndVertical();
        }

        private void DrawUsageInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Usage", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField("1. Create this config: Assets > Create > BizSim > Games Services Mock Config",
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
