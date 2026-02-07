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

    public enum LeaderboardErrorType
    {
        Unknown = 0,
        ApiNotAvailable = -1,
        UserNotAuthenticated = 1,
        NetworkError = 2,
        LeaderboardNotFound = 3,
        InternalError = 100
    }
}
