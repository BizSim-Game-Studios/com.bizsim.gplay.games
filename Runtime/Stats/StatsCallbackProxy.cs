// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class StatsCallbackProxy : AndroidJavaProxy
    {
        private readonly GamesStatsController _controller;

        public StatsCallbackProxy(GamesStatsController controller)
            : base("com.bizsim.gplay.games.stats.IStatsCallback")
        {
            _controller = controller;
        }

        void onStatsLoaded(string statsJson)
        {
            BizSimGamesLogger.Info("Stats loaded");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnStatsLoadedFromJava(statsJson));
        }

        void onStatsError(int errorCode, string errorMessage)
        {
            BizSimGamesLogger.Error($"Stats error: {errorCode} - {errorMessage}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnStatsErrorFromJava(errorCode, errorMessage));
        }
    }
}
