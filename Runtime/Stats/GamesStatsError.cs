// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    [Serializable]
    public class GamesStatsError
    {
        public int errorCode;
        public string errorMessage;

        public StatsErrorType Type => errorCode switch
        {
            -1 => StatsErrorType.ApiNotAvailable,
            1 => StatsErrorType.UserNotAuthenticated,
            2 => StatsErrorType.NetworkError,
            100 => StatsErrorType.InternalError,
            _ => StatsErrorType.Unknown
        };

        public GamesStatsError(int code, string message)
        {
            errorCode = code;
            errorMessage = message;
        }

        public override string ToString() => $"[StatsError {errorCode}] {Type}: {errorMessage}";
    }

    /// <summary>
    /// Player stats error codes mapped from Google Play Games PlayerStatsClient.
    /// </summary>
    public enum StatsErrorType
    {
        /// <summary>Unclassified error not matching any known code.</summary>
        Unknown = 0,

        /// <summary>Google Play Games API is not available on this device (code -1).</summary>
        ApiNotAvailable = -1,

        /// <summary>User is not signed in. Call GamesServicesManager.Auth.SignInAsync() first (code 1).</summary>
        UserNotAuthenticated = 1,

        /// <summary>Network error — device is offline or Google servers unreachable (code 2).</summary>
        NetworkError = 2,

        /// <summary>Internal error in Google Play Games SDK (code 100).</summary>
        InternalError = 100
    }

    public class GamesStatsException : GamesException
    {
        public GamesStatsError Error { get; }

        public GamesStatsException(GamesStatsError error)
            : base(error?.errorCode ?? 0, $"Stats operation failed: {error}")
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
