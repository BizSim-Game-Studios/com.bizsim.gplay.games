// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal class MockEventsProvider : IGamesEventsProvider
    {
        private readonly GamesServicesConfig.MockSettings _mock;
        private readonly Dictionary<string, long> _eventCounters = new();

        public event Action<GamesEventsError> OnEventsError;

        public MockEventsProvider(GamesServicesConfig.MockSettings mock)
        {
            _mock = mock;
            BizSimGamesLogger.Info("MockEventsProvider initialized");
        }

        private void ThrowIfSimulatingErrors(string eventId = null)
        {
            if (_mock == null || !_mock.mockSimulateErrors) return;

            var error = new GamesEventsError
            {
                errorCode = GamesErrorCodes.NetworkError,
                message = "Simulated network error",
                eventId = eventId
            };
            OnEventsError?.Invoke(error);
            throw new GamesEventsException(error);
        }

        public async Task IncrementEventAsync(string eventId, int steps = 1, CancellationToken ct = default)
        {
            await Task.Delay(50, ct);
            ThrowIfSimulatingErrors(eventId);

            if (!_eventCounters.ContainsKey(eventId))
                _eventCounters[eventId] = 0;
            _eventCounters[eventId] += steps;

            BizSimGamesLogger.Info($"[MOCK] Event incremented: {eventId} += {steps} (total: {_eventCounters[eventId]})");
        }

        public async Task<GamesEvent[]> LoadEventsAsync(CancellationToken ct = default)
        {
            await Task.Delay(200, ct);
            ThrowIfSimulatingErrors();

            var events = new List<GamesEvent>();
            foreach (var kvp in _eventCounters)
            {
                events.Add(new GamesEvent
                {
                    eventId = kvp.Key,
                    name = kvp.Key,
                    description = "Mock event",
                    value = kvp.Value,
                    isVisible = true
                });
            }

            return events.ToArray();
        }

        public async Task<GamesEvent> LoadEventAsync(string eventId, CancellationToken ct = default)
        {
            await Task.Delay(100, ct);
            ThrowIfSimulatingErrors(eventId);

            return new GamesEvent
            {
                eventId = eventId,
                name = eventId,
                description = "Mock event",
                value = _eventCounters.ContainsKey(eventId) ? _eventCounters[eventId] : 0,
                isVisible = true
            };
        }
    }
}
