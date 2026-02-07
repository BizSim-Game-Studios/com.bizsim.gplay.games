// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    public interface IGamesStatsProvider
    {
        Task<GamesPlayerStats> LoadPlayerStatsAsync(bool forceReload = false, CancellationToken ct = default);
        event Action<GamesPlayerStats> OnStatsLoaded;
        event Action<GamesStatsError> OnStatsError;
    }
}
