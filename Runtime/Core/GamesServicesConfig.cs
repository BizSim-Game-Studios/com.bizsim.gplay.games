// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    [CreateAssetMenu(
        fileName = "GamesServicesConfig",
        menuName = "BizSim/Google Play Games/Services Config")]
    public class GamesServicesConfig : ScriptableObject
    {
        [Header("Service Toggles")]
        public bool enableAuth = true;
        public bool enableAchievements = true;
        public bool enableLeaderboards = true;
        public bool enableCloudSave = true;
        public bool enableEvents = false;
        public bool enableStats = true;

        [Header("Sidekick")]
        [Tooltip("Enable Play Console Sidekick features. Requires achievements + cloud save at minimum.")]
        public bool sidekickReady = false;

        [Header("Quality Checklist")]
        public int expectedAchievementCount = 10;
        public bool requireCloudSaveMetadata = true;

        [Header("Editor Mock Settings")]
        [Tooltip("Used in Editor only. Ignored on device.")]
        public MockSettings editorMock = new MockSettings();

        [Serializable]
        public class MockSettings
        {
            [Header("Authentication")]
            public bool authSucceeds = true;
            public string mockPlayerId = "mock_player_12345";
            public string mockDisplayName = "Test Player";
            public AuthErrorType mockAuthErrorType = AuthErrorType.UserCancelled;

            [Header("Profile (ID Token Claims)")]
            public bool mockConsentGranted = true;
            public string mockEmail = "testplayer@gmail.com";
            public bool mockEmailVerified = true;
            public string mockFullName = "Test Player";
            public string mockGivenName = "Test";
            public string mockFamilyName = "Player";
            public string mockPictureUrl = "https://lh3.googleusercontent.com/a/mock-avatar=s96-c";
            public string mockLocale = "en";

            [Header("Simulation")]
            [Range(0f, 5f)]
            public float authDelaySeconds = 0.5f;

            [Header("Achievements")]
            public int mockUnlockedCount = 5;

            [Header("Leaderboards")]
            public long mockScore = 1000;

            [Header("Player Stats")]
            [Range(0f, 1f)]
            public float mockChurnProbability = 0.15f;

            public int GetAuthErrorCode()
            {
                return mockAuthErrorType switch
                {
                    AuthErrorType.UserCancelled => 1,
                    AuthErrorType.NoConnection => 2,
                    AuthErrorType.SignInRequired => 3,
                    AuthErrorType.SignInFailed => 4,
                    AuthErrorType.Timeout => -1,
                    AuthErrorType.Unknown => 0,
                    _ => 0
                };
            }

            public string GetAuthErrorMessage()
            {
                return mockAuthErrorType switch
                {
                    AuthErrorType.UserCancelled => "User cancelled sign-in",
                    AuthErrorType.NoConnection => "Network connection error - check your internet",
                    AuthErrorType.SignInRequired => "Sign in required to access this feature",
                    AuthErrorType.SignInFailed => "Google Play Games sign-in failed",
                    AuthErrorType.Timeout => "Authentication timed out - please try again",
                    AuthErrorType.Unknown => "An unknown error occurred",
                    _ => "Unknown error"
                };
            }
        }
    }
}
