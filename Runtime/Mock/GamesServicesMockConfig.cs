// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    [Obsolete("Use GamesServicesConfig (Create > BizSim > Google Play Games > Services Config) instead. Will be removed in v1.4.0.")]
    [CreateAssetMenu(fileName = "DefaultGamesConfig", menuName = "BizSim/Games Services Mock Config", order = 100)]
    public class GamesServicesMockConfig : ScriptableObject
    {
        [Header("Authentication")]
        [Tooltip("Should mock authentication succeed?")]
        public bool authSucceeds = true;

        [Tooltip("Mock player ID (if auth succeeds)")]
        public string mockPlayerId = "mock_player_12345";

        [Tooltip("Mock Play Games display name")]
        public string mockDisplayName = "Test Player";

        [Tooltip("Mock auth error type (if auth fails)")]
        public AuthErrorType mockAuthErrorType = AuthErrorType.UserCancelled;

        [Header("Profile (ID Token Claims)")]
        [Tooltip("Simulate user granting consent for requested scopes")]
        public bool mockConsentGranted = true;

        [Tooltip("Google Account email")]
        public string mockEmail = "testplayer@gmail.com";

        [Tooltip("Is the email verified?")]
        public bool mockEmailVerified = true;

        [Tooltip("Google Account full name")]
        public string mockFullName = "Test Player";

        [Tooltip("Given (first) name")]
        public string mockGivenName = "Test";

        [Tooltip("Family (last) name")]
        public string mockFamilyName = "Player";

        [Tooltip("Google Account profile picture URL")]
        public string mockPictureUrl = "https://lh3.googleusercontent.com/a/mock-avatar=s96-c";

        [Tooltip("Account locale (e.g., en, tr, de)")]
        public string mockLocale = "en";

        [Header("Simulation")]
        [Tooltip("Delay before auth completes (seconds)")]
        [Range(0f, 5f)]
        public float authDelaySeconds = 0.5f;

        [Header("Achievements")]
        [Tooltip("Mock achievements unlocked count")]
        public int mockUnlockedCount = 5;

        [Header("Leaderboards")]
        [Tooltip("Mock leaderboard score")]
        public long mockScore = 1000;

        [Header("Player Stats")]
        [Tooltip("Mock churn probability (0-1)")]
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
