// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal class MockStatsProvider : IGamesStatsProvider
    {
        private readonly GamesServicesConfig.MockSettings _mock;

        public event Action<GamesPlayerStats> OnStatsLoaded;
        public event Action<GamesStatsError> OnStatsError;

        public MockStatsProvider(GamesServicesConfig.MockSettings mock)
        {
            _mock = mock;
            BizSimGamesLogger.Info("MockStatsProvider initialized");
        }

        public async Task<GamesPlayerStats> LoadPlayerStatsAsync(bool forceReload = false, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);

            if (_mock != null && _mock.mockSimulateErrors)
            {
                var error = new GamesStatsError(GamesErrorCodes.NetworkError, "Simulated network error");
                OnStatsError?.Invoke(error);
                throw new GamesStatsException(error);
            }

            var stats = new GamesPlayerStats
            {
                avgSessionLengthMinutes = 15.5f,
                daysSinceLastPlayed = 0,
                numberOfPurchases = 3,
                numberOfSessions = 42,
                sessionPercentile = 0.75f,
                spendPercentile = 0.60f,
                churnProbability = _mock?.mockChurnProbability ?? 0.15f,
                highSpenderProbability = 0.30f
            };

            BizSimGamesLogger.Info("[MOCK] Player stats loaded");
            OnStatsLoaded?.Invoke(stats);
            return stats;
        }
    }
}
