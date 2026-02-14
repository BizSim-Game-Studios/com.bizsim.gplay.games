// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Authentication error types returned by Google Play Games Services.
    /// </summary>
    public enum AuthErrorType
    {
        /// <summary>User cancelled sign-in dialog.</summary>
        UserCancelled,

        /// <summary>Network error - unable to reach Google servers.</summary>
        NoConnection,

        /// <summary>User not signed in - game should show "Sign In" button.</summary>
        SignInRequired,

        /// <summary>Google Play Services error.</summary>
        SignInFailed,

        /// <summary>Operation timed out.</summary>
        Timeout,

        /// <summary>Unclassified error.</summary>
        Unknown
    }

    /// <summary>
    /// Authentication error returned by Google Play Games Services.
    /// Field names are camelCase to match JSON from Java bridge (JsonUtility requirement).
    /// </summary>
    [Serializable]
    public class GamesAuthError
    {
        // Field names MUST be camelCase to match JSON keys from AuthBridge.java
        // JsonUtility.FromJson is case-sensitive — PascalCase fields will fail to deserialize
        public int errorCode;
        public string errorMessage;
        public bool isRetryable;

        /// <summary>Type-safe error code for readable error handling.</summary>
        public AuthErrorType Type => errorCode switch
        {
            1 => AuthErrorType.UserCancelled,
            2 => AuthErrorType.NoConnection,
            3 => AuthErrorType.SignInRequired,
            4 => AuthErrorType.SignInFailed,
            -1 => AuthErrorType.Timeout,
            _ => AuthErrorType.Unknown
        };

        /// <summary>Human-readable error message.</summary>
        public string Message => errorMessage ?? "Unknown error";

        public override string ToString() => $"{Type} ({errorCode}): {Message}";
    }

    /// <summary>
    /// Exception thrown by authentication operations.
    /// </summary>
    public class GamesAuthException : GamesException
    {
        public GamesAuthError Error { get; }

        public GamesAuthException(GamesAuthError error)
            : base(error?.errorCode ?? 0, $"Authentication failed: {error}")
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
