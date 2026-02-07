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

    public enum StatsErrorType
    {
        Unknown = 0,
        ApiNotAvailable = -1,
        UserNotAuthenticated = 1,
        NetworkError = 2,
        InternalError = 100
    }
}
