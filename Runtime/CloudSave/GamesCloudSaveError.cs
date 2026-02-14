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

    /// <summary>
    /// Cloud save error codes mapped from Google Play Games SnapshotsClient.
    /// </summary>
    public enum CloudSaveErrorType
    {
        /// <summary>Unclassified error not matching any known code.</summary>
        Unknown = 0,

        /// <summary>Google Play Games API is not available on this device (code -1).</summary>
        ApiNotAvailable = -1,

        /// <summary>User is not signed in. Call GamesServicesManager.Auth.SignInAsync() first (code 1).</summary>
        UserNotAuthenticated = 1,

        /// <summary>Network error — device is offline or Google servers unreachable (code 2).</summary>
        NetworkError = 2,

        /// <summary>Snapshot filename does not exist and createIfNotFound was false (code 3).</summary>
        SnapshotNotFound = 3,

        /// <summary>Conflict resolution timed out — player did not choose within deadline (code 4).</summary>
        ConflictTimeout = 4,

        /// <summary>Snapshot data exceeds 3MB limit per save slot (code 5).</summary>
        DataTooLarge = 5,

        /// <summary>Internal error in Google Play Games SDK (code 100).</summary>
        InternalError = 100
    }

    public class GamesCloudSaveException : GamesException
    {
        public GamesCloudSaveError Error { get; }

        public GamesCloudSaveException(GamesCloudSaveError error)
            : base(error?.errorCode ?? 0, $"Cloud save failed: {error}")
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
