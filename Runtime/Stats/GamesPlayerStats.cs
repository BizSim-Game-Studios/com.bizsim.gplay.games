// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine.Scripting;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Player statistics from Google Play Games Services.
    /// Populated by PlayerStatsClient on the Java side.
    /// Field names are camelCase to match JSON from StatsBridge.java.
    /// </summary>
    [Serializable, Preserve]
    public class GamesPlayerStats
    {
        /// <summary>Average session length in minutes over the player's lifetime.</summary>
        public float avgSessionLengthMinutes;

        /// <summary>Number of days since the player last opened the app.</summary>
        public int daysSinceLastPlayed;

        /// <summary>Total number of in-app purchases the player has made.</summary>
        public int numberOfPurchases;

        /// <summary>Total number of sessions the player has had.</summary>
        public int numberOfSessions;

        /// <summary>Session length percentile (0.0–1.0) relative to all players.</summary>
        public float sessionPercentile;

        /// <summary>Spend percentile (0.0–1.0) relative to all players.</summary>
        public float spendPercentile;

        /// <summary>Probability (0.0–1.0) that this player will churn (stop playing).</summary>
        public float churnProbability;

        /// <summary>Probability (0.0–1.0) that this player is a high spender.</summary>
        public float highSpenderProbability;
    }
}
