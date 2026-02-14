// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine.Scripting;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Represents a leaderboard score entry.
    /// </summary>
    [Serializable, Preserve]
    public class GamesLeaderboardEntry
    {
        public string playerId;
        public string displayName;
        public long score;
        public string formattedScore;
        public long rank;
        public string scoreTag;
        public long timestampMillis;
        public string avatarUrl;
    }
}
