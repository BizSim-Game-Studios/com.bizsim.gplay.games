// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Mock configuration for Editor testing.
    /// Create via: Assets → Create → BizSim → Games Services Mock Config
    /// Will be created as "DefaultGamesConfig" and auto-loaded from Resources/ folder.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultGamesConfig", menuName = "BizSim/Games Services Mock Config", order = 100)]
    public class GamesServicesMockConfig : ScriptableObject
    {
        [Header("Authentication")]
        [Tooltip("Should mock authentication succeed?")]
        public bool authSucceeds = true;

        [Tooltip("Mock player ID (if auth succeeds)")]
        public string mockPlayerId = "mock_player_12345";

        [Tooltip("Mock player display name")]
        public string mockDisplayName = "Test Player";

        [Tooltip("Mock auth error type (if auth fails) - dropdown for easier testing")]
        public AuthErrorType mockAuthErrorType = AuthErrorType.UserCancelled;

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

        /// <summary>
        /// Get error code for the selected error type.
        /// </summary>
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

        /// <summary>
        /// Get human-readable error message for the selected error type.
        /// </summary>
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
