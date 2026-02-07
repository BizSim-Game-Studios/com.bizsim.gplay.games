// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal class MockLeaderboardProvider : IGamesLeaderboardProvider
    {
        private Dictionary<string, List<GamesLeaderboardEntry>> _mockScores;

        public event Action<string, long> OnScoreSubmitted;
        public event Action<string, List<GamesLeaderboardEntry>> OnScoresLoaded;

#pragma warning disable CS0067 // Event is never used (mock provider doesn't simulate errors)
        public event Action<GamesLeaderboardError> OnLeaderboardError;
#pragma warning restore CS0067

        public MockLeaderboardProvider(GamesServicesMockConfig config)
        {
            InitializeMockData();
            BizSimGamesLogger.Info("MockLeaderboardProvider initialized");
        }

        private void InitializeMockData()
        {
            _mockScores = new Dictionary<string, List<GamesLeaderboardEntry>>
            {
                ["leaderboard_high_score"] = new List<GamesLeaderboardEntry>
                {
                    new GamesLeaderboardEntry { playerId = "mock1", displayName = "Player 1", score = 1000, rank = 1 },
                    new GamesLeaderboardEntry { playerId = "mock2", displayName = "Player 2", score = 900, rank = 2 },
                    new GamesLeaderboardEntry { playerId = "mock3", displayName = "Player 3", score = 800, rank = 3 }
                }
            };
        }

        public async Task SubmitScoreAsync(string leaderboardId, long score, string scoreTag = null, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);
            BizSimGamesLogger.Info($"[MOCK] Score submitted: {score} to {leaderboardId}");
            OnScoreSubmitted?.Invoke(leaderboardId, score);
        }

        public async Task ShowLeaderboardUIAsync(string leaderboardId, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            BizSimGamesLogger.Info($"[MOCK] Leaderboard UI shown: {leaderboardId}");
        }

        public async Task ShowAllLeaderboardsUIAsync(CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            BizSimGamesLogger.Info("[MOCK] All leaderboards UI shown");
        }

        public async Task<List<GamesLeaderboardEntry>> LoadTopScoresAsync(string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            var scores = _mockScores.ContainsKey(leaderboardId) ? _mockScores[leaderboardId] : new List<GamesLeaderboardEntry>();
            OnScoresLoaded?.Invoke(leaderboardId, scores);
            return scores;
        }

        public Task<List<GamesLeaderboardEntry>> LoadPlayerCenteredScoresAsync(string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25, CancellationToken ct = default)
        {
            return LoadTopScoresAsync(leaderboardId, timeSpan, collection, maxResults, ct);
        }
    }
}
