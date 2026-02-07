// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Google Play Games authentication provider.
    /// PGS v2 automatically attempts silent sign-in when game starts.
    /// </summary>
    public interface IGamesAuthProvider
    {
        /// <summary>Fired when player successfully authenticates (silent or manual).</summary>
        event Action<GamesPlayer> OnAuthenticationSuccess;

        /// <summary>Fired when authentication fails (user cancelled, network error, etc.).</summary>
        event Action<GamesAuthError> OnAuthenticationFailed;

        /// <summary>True if player is currently signed in to Google Play Games.</summary>
        bool IsAuthenticated { get; }

        /// <summary>Current authenticated player data (null if not authenticated).</summary>
        GamesPlayer CurrentPlayer { get; }

        /// <summary>
        /// Attempts to authenticate the player with Google Play Games.
        /// First call: Silent auth (no UI if previously signed in).
        /// Subsequent calls: Shows Google Play sign-in UI.
        /// </summary>
        Task<GamesPlayer> AuthenticateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Requests OAuth2 server-side access token for backend integration.
        /// Requires authentication first.
        /// </summary>
        Task<string> RequestServerSideAccessAsync(string serverClientId, bool forceRefresh = false, CancellationToken cancellationToken = default);
    }
}
