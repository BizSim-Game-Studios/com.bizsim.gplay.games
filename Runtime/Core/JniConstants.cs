// Copyright (c) BizSim Game Studios. All rights reserved.

namespace BizSim.GPlay.Games
{
    internal static class JniConstants
    {
        internal const string UnityPlayer = "com.unity3d.player.UnityPlayer";

        internal const string AuthBridge = "com.bizsim.gplay.games.AuthBridge";
        internal const string AchievementBridge = "com.bizsim.gplay.games.achievements.AchievementBridge";
        internal const string LeaderboardBridge = "com.bizsim.gplay.games.leaderboards.LeaderboardBridge";
        internal const string CloudSaveBridge = "com.bizsim.gplay.games.cloudsave.CloudSaveBridge";
        internal const string StatsBridge = "com.bizsim.gplay.games.stats.StatsBridge";
        internal const string EventsBridge = "com.bizsim.gplay.games.events.EventsBridge";

        internal const string AuthCallback = "com.bizsim.gplay.games.callbacks.IAuthCallback";
        internal const string AchievementCallback = "com.bizsim.gplay.games.achievements.IAchievementCallback";
        internal const string LeaderboardCallback = "com.bizsim.gplay.games.leaderboards.ILeaderboardCallback";
        internal const string CloudSaveCallback = "com.bizsim.gplay.games.cloudsave.ICloudSaveCallback";
        internal const string StatsCallback = "com.bizsim.gplay.games.stats.IStatsCallback";
        internal const string EventsCallback = "com.bizsim.gplay.games.events.IEventsCallback";
    }
}
