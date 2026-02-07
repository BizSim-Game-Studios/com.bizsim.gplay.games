// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Represents an error that occurred during an achievement operation.
    /// </summary>
    [Serializable]
    public class GamesAchievementError
    {
        /// <summary>
        /// Error code from Google Play Games (raw integer).
        /// </summary>
        public int errorCode;

        /// <summary>
        /// Human-readable error message.
        /// </summary>
        public string errorMessage;

        /// <summary>
        /// Achievement ID that caused the error (if applicable).
        /// </summary>
        public string achievementId;

        /// <summary>
        /// Type-safe error classification.
        /// </summary>
        public AchievementErrorType Type => errorCode switch
        {
            -1 => AchievementErrorType.ApiNotAvailable,
            1 => AchievementErrorType.UserNotAuthenticated,
            2 => AchievementErrorType.NetworkError,
            3 => AchievementErrorType.AchievementNotFound,
            4 => AchievementErrorType.InvalidSteps,
            5 => AchievementErrorType.AlreadyUnlocked,
            100 => AchievementErrorType.InternalError,
            _ => AchievementErrorType.Unknown
        };

        public GamesAchievementError(int code, string message, string achievementId = null)
        {
            this.errorCode = code;
            this.errorMessage = message;
            this.achievementId = achievementId;
        }

        public override string ToString()
        {
            string achievementInfo = string.IsNullOrEmpty(achievementId) ? "" : $" (Achievement: {achievementId})";
            return $"[AchievementError {errorCode}] {Type}: {errorMessage}{achievementInfo}";
        }
    }

    /// <summary>
    /// Type-safe classification of achievement errors.
    /// </summary>
    public enum AchievementErrorType
    {
        /// <summary>
        /// Unknown error type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Google Play Games API is not available on this device.
        /// </summary>
        ApiNotAvailable = -1,

        /// <summary>
        /// User is not authenticated with Google Play Games.
        /// </summary>
        UserNotAuthenticated = 1,

        /// <summary>
        /// Network connection error.
        /// </summary>
        NetworkError = 2,

        /// <summary>
        /// Achievement ID does not exist in Play Console configuration.
        /// </summary>
        AchievementNotFound = 3,

        /// <summary>
        /// Invalid step count for incremental achievement (must be > 0).
        /// </summary>
        InvalidSteps = 4,

        /// <summary>
        /// Achievement is already unlocked (redundant unlock attempt).
        /// </summary>
        AlreadyUnlocked = 5,

        /// <summary>
        /// Internal error in Google Play Games SDK.
        /// </summary>
        InternalError = 100
    }
}
