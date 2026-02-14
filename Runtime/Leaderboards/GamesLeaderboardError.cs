// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    [Serializable]
    public class GamesLeaderboardError
    {
        public int errorCode;
        public string errorMessage;
        public string leaderboardId;

        public LeaderboardErrorType Type => errorCode switch
        {
            -1 => LeaderboardErrorType.ApiNotAvailable,
            1 => LeaderboardErrorType.UserNotAuthenticated,
            2 => LeaderboardErrorType.NetworkError,
            3 => LeaderboardErrorType.LeaderboardNotFound,
            100 => LeaderboardErrorType.InternalError,
            _ => LeaderboardErrorType.Unknown
        };

        public GamesLeaderboardError(int code, string message, string leaderboardId = null)
        {
            this.errorCode = code;
            this.errorMessage = message;
            this.leaderboardId = leaderboardId;
        }

        public override string ToString() =>
            $"[LeaderboardError {errorCode}] {Type}: {errorMessage}" +
            (string.IsNullOrEmpty(leaderboardId) ? "" : $" (Leaderboard: {leaderboardId})");
    }

    /// <summary>
    /// Leaderboard error codes mapped from Google Play Games LeaderboardsClient.
    /// </summary>
    public enum LeaderboardErrorType
    {
        /// <summary>Unclassified error not matching any known code.</summary>
        Unknown = 0,

        /// <summary>Google Play Games API is not available on this device (code -1).</summary>
        ApiNotAvailable = -1,

        /// <summary>User is not signed in. Call GamesServicesManager.Auth.SignInAsync() first (code 1).</summary>
        UserNotAuthenticated = 1,

        /// <summary>Network error — device is offline or Google servers unreachable (code 2).</summary>
        NetworkError = 2,

        /// <summary>Leaderboard ID does not exist in Play Console configuration (code 3).</summary>
        LeaderboardNotFound = 3,

        /// <summary>Internal error in Google Play Games SDK (code 100).</summary>
        InternalError = 100
    }

    public class GamesLeaderboardException : GamesException
    {
        public GamesLeaderboardError Error { get; }

        public GamesLeaderboardException(GamesLeaderboardError error)
            : base(error?.errorCode ?? 0, $"Leaderboard operation failed: {error}")
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
