// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    [Serializable]
    public class GamesCloudSaveError
    {
        public int errorCode;
        public string errorMessage;
        public string filename;

        public CloudSaveErrorType Type => errorCode switch
        {
            -1 => CloudSaveErrorType.ApiNotAvailable,
            1 => CloudSaveErrorType.UserNotAuthenticated,
            2 => CloudSaveErrorType.NetworkError,
            3 => CloudSaveErrorType.SnapshotNotFound,
            4 => CloudSaveErrorType.ConflictTimeout,
            5 => CloudSaveErrorType.DataTooLarge,
            100 => CloudSaveErrorType.InternalError,
            _ => CloudSaveErrorType.Unknown
        };

        public GamesCloudSaveError(int code, string message, string filename = null)
        {
            this.errorCode = code;
            this.errorMessage = message;
            this.filename = filename;
        }

        public override string ToString() =>
            $"[CloudSaveError {errorCode}] {Type}: {errorMessage}" +
            (string.IsNullOrEmpty(filename) ? "" : $" (File: {filename})");
    }

    public enum CloudSaveErrorType
    {
        Unknown = 0,
        ApiNotAvailable = -1,
        UserNotAuthenticated = 1,
        NetworkError = 2,
        SnapshotNotFound = 3,
        ConflictTimeout = 4,
        DataTooLarge = 5,
        InternalError = 100
    }
}
