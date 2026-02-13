// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal class MockStatsProvider : IGamesStatsProvider
    {
        public event Action<GamesPlayerStats> OnStatsLoaded;

#pragma warning disable CS0067 // Event never used (mock doesn't simulate errors)
        public event Action<GamesStatsError> OnStatsError;
#pragma warning restore CS0067

        public MockStatsProvider(GamesServicesConfig.MockSettings mock)
        {
            BizSimGamesLogger.Info("MockStatsProvider initialized");
        }

        public async Task<GamesPlayerStats> LoadPlayerStatsAsync(bool forceReload = false, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);

            var stats = new GamesPlayerStats
            {
                avgSessionLengthMinutes = 15.5f,
                daysSinceLastPlayed = 0,
                numberOfPurchases = 3,
                numberOfSessions = 42,
                sessionPercentile = 0.75f,
                spendPercentile = 0.60f,
                churnProbability = 0.15f,
                highSpenderProbability = 0.30f
            };

            BizSimGamesLogger.Info("[MOCK] Player stats loaded");
            OnStatsLoaded?.Invoke(stats);
            return stats;
        }
    }
}
