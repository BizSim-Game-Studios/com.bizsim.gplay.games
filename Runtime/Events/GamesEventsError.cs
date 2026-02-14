// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    [Serializable]
    public class GamesEventsError
    {
        public string eventId;
        public int errorCode;
        public string message;

        public EventsErrorType Type => errorCode switch
        {
            -1 => EventsErrorType.ApiNotAvailable,
            1 => EventsErrorType.UserNotAuthenticated,
            2 => EventsErrorType.NetworkError,
            3 => EventsErrorType.EventNotFound,
            100 => EventsErrorType.InternalError,
            _ => EventsErrorType.Unknown
        };

        public override string ToString() =>
            $"[EventsError {errorCode}] {Type}: {message}" +
            (string.IsNullOrEmpty(eventId) ? "" : $" (Event: {eventId})");
    }

    /// <summary>
    /// Events error codes mapped from Google Play Games EventsClient.
    /// </summary>
    public enum EventsErrorType
    {
        /// <summary>Unclassified error not matching any known code.</summary>
        Unknown = 0,

        /// <summary>Google Play Games API is not available on this device (code -1).</summary>
        ApiNotAvailable = -1,

        /// <summary>User is not signed in. Call GamesServicesManager.Auth.SignInAsync() first (code 1).</summary>
        UserNotAuthenticated = 1,

        /// <summary>Network error — device is offline or Google servers unreachable (code 2).</summary>
        NetworkError = 2,

        /// <summary>Event ID does not exist in Play Console configuration (code 3).</summary>
        EventNotFound = 3,

        /// <summary>Internal error in Google Play Games SDK (code 100).</summary>
        InternalError = 100
    }

    public class GamesEventsException : GamesException
    {
        public GamesEventsError Error { get; }

        public GamesEventsException(GamesEventsError error)
            : base(error?.errorCode ?? 0, $"Events operation failed: {error}")
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
