// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    public interface IGamesEventsProvider
    {
        Task IncrementEventAsync(string eventId, int steps = 1, CancellationToken ct = default);
        Task<GamesEvent[]> LoadEventsAsync(CancellationToken ct = default);
        Task<GamesEvent> LoadEventAsync(string eventId, CancellationToken ct = default);
        event Action<GamesEventsError> OnEventsError;
    }
}
