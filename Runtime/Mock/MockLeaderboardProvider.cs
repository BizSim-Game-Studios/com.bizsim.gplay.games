// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal class MockLeaderboardProvider : IGamesLeaderboardProvider
    {
        private readonly GamesServicesConfig.MockSettings _mock;
        private Dictionary<string, List<GamesLeaderboardEntry>> _mockScores;

        public event Action<string, long> OnScoreSubmitted;
        public event Action<string, List<GamesLeaderboardEntry>> OnScoresLoaded;
        public event Action<GamesLeaderboardError> OnLeaderboardError;

        public MockLeaderboardProvider(GamesServicesConfig.MockSettings mock)
        {
            _mock = mock;
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

        private void ThrowIfSimulatingErrors(string leaderboardId = null)
        {
            if (_mock == null || !_mock.mockSimulateErrors) return;

            var error = new GamesLeaderboardError(GamesErrorCodes.NetworkError, "Simulated network error", leaderboardId);
            OnLeaderboardError?.Invoke(error);
            throw new GamesLeaderboardException(error);
        }

        public async Task SubmitScoreAsync(string leaderboardId, long score, string scoreTag = null, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);
            ThrowIfSimulatingErrors(leaderboardId);

            if (!_mockScores.ContainsKey(leaderboardId))
                _mockScores[leaderboardId] = new List<GamesLeaderboardEntry>();

            var entries = _mockScores[leaderboardId];
            string playerId = _mock?.mockPlayerId ?? "mock_local";
            string displayName = _mock?.mockDisplayName ?? "You";

            var existing = entries.Find(e => e.playerId == playerId);
            if (existing != null)
            {
                if (score > existing.score)
                    existing.score = score;
            }
            else
            {
                entries.Add(new GamesLeaderboardEntry
                {
                    playerId = playerId,
                    displayName = displayName,
                    score = score,
                    rank = 0
                });
            }

            entries.Sort((a, b) => b.score.CompareTo(a.score));
            for (int i = 0; i < entries.Count; i++)
                entries[i].rank = i + 1;

            BizSimGamesLogger.Info($"[MOCK] Score submitted: {score} to {leaderboardId}");
            OnScoreSubmitted?.Invoke(leaderboardId, score);
        }

        public async Task ShowLeaderboardUIAsync(string leaderboardId, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            ThrowIfSimulatingErrors(leaderboardId);

            BizSimGamesLogger.Info($"[MOCK] Leaderboard UI shown: {leaderboardId}");
        }

        public async Task ShowAllLeaderboardsUIAsync(CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            ThrowIfSimulatingErrors();

            BizSimGamesLogger.Info("[MOCK] All leaderboards UI shown");
        }

        public async Task<List<GamesLeaderboardEntry>> LoadTopScoresAsync(string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            ThrowIfSimulatingErrors(leaderboardId);

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
