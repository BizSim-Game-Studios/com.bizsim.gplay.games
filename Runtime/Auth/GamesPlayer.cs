// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Google Play Games player profile data.
    /// </summary>
    [Serializable]
    public class GamesPlayer
    {
        /// <summary>Unique player ID (stable, never changes).</summary>
        public string PlayerId { get; }

        /// <summary>Player display name (can change).</summary>
        public string DisplayName { get; }

        /// <summary>Banner image URI (nullable).</summary>
        public string BannerImageUri { get; }

        /// <summary>High-resolution avatar image URI (nullable).</summary>
        public string HiResImageUri { get; }

        public GamesPlayer(string playerId, string displayName, string bannerImageUri = null, string hiResImageUri = null)
        {
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
            DisplayName = displayName ?? "Unknown Player";
            BannerImageUri = bannerImageUri;
            HiResImageUri = hiResImageUri;
        }

        public override string ToString() => $"{DisplayName} ({PlayerId})";
    }
}
