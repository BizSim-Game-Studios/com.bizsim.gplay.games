// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class LeaderboardCallbackProxy : AndroidJavaProxy
    {
        private readonly GamesLeaderboardController _controller;

        public LeaderboardCallbackProxy(GamesLeaderboardController controller)
            : base("com.bizsim.gplay.games.leaderboards.ILeaderboardCallback")
        {
            _controller = controller;
        }

        void onScoreSubmitted(string leaderboardId, long score)
        {
            BizSimGamesLogger.Info($"Score submitted: {score} to {leaderboardId}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnScoreSubmittedFromJava(leaderboardId, score));
        }

        void onScoresLoaded(string leaderboardId, string scoresJson)
        {
            BizSimGamesLogger.Info($"Scores loaded for {leaderboardId}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnScoresLoadedFromJava(leaderboardId, scoresJson));
        }

        void onLeaderboardUIClosed()
        {
            BizSimGamesLogger.Info("Leaderboard UI closed");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnLeaderboardUIClosedFromJava());
        }

        void onLeaderboardError(int errorCode, string errorMessage, string leaderboardId)
        {
            BizSimGamesLogger.Error($"Leaderboard error: {errorCode} - {errorMessage}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnLeaderboardErrorFromJava(errorCode, errorMessage, leaderboardId));
        }
    }
}
