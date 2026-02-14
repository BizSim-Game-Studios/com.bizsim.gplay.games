// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace BizSim.GPlay.Games
{
    internal class GamesEventsController : JniBridgeBase, IGamesEventsProvider
    {
        private const float FLUSH_INTERVAL_SECONDS = 5f;

        private EventsCallbackProxy _callbackProxy;

        private readonly Dictionary<string, int> _pendingIncrements = new();
        private float _lastFlushTime;

        private TaskCompletionSource<GamesEvent[]> _loadAllTcs;
        private TaskCompletionSource<GamesEvent> _loadOneTcs;

        public event Action<GamesEventsError> OnEventsError;

        protected override string JavaClassName => JniConstants.EventsBridge;

        protected override AndroidJavaProxy CreateCallbackProxy()
        {
            _callbackProxy = new EventsCallbackProxy(this);
            return _callbackProxy;
        }

        public GamesEventsController()
        {
            InitializeBridge();
        }

        public Task IncrementEventAsync(string eventId, int steps = 1, CancellationToken ct = default)
        {
            if (!UnityMainThreadDispatcher.IsMainThread)
            {
                var tcs = new TaskCompletionSource<bool>();
                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    try
                    {
                        IncrementOnMainThread(eventId, steps, ct);
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
                return tcs.Task;
            }

            IncrementOnMainThread(eventId, steps, ct);
            return Task.CompletedTask;
        }

        private void IncrementOnMainThread(string eventId, int steps, CancellationToken ct)
        {
            if (!_pendingIncrements.ContainsKey(eventId))
                _pendingIncrements[eventId] = 0;
            _pendingIncrements[eventId] += steps;

            if (Time.realtimeSinceStartup - _lastFlushTime >= FLUSH_INTERVAL_SECONDS)
                FlushPendingIncrements(ct);
        }

        internal Task FlushPendingIncrements(CancellationToken ct = default)
        {
            if (_pendingIncrements.Count == 0)
                return Task.CompletedTask;

            var batch = new Dictionary<string, int>(_pendingIncrements);
            _pendingIncrements.Clear();
            _lastFlushTime = Time.realtimeSinceStartup;

            foreach (var kvp in batch)
            {
                try
                {
                    CallBridge("incrementEvent", kvp.Key, kvp.Value);
                }
                catch (Exception e)
                {
                    BizSimGamesLogger.Error($"[Events] Failed to increment event '{kvp.Key}': {e.Message}");
                    OnEventsError?.Invoke(new GamesEventsError
                    {
                        eventId = kvp.Key,
                        errorCode = GamesErrorCodes.ApiNotAvailable,
                        message = e.Message
                    });
                }
            }

            return Task.CompletedTask;
        }

        public async Task<GamesEvent[]> LoadEventsAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await FlushPendingIncrements(ct);

            var tcs = TcsGuard.Replace(ref _loadAllTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("loadEvents");
                return await tcs.Task;
            }
        }

        public async Task<GamesEvent> LoadEventAsync(string eventId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _loadOneTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("loadEvent", eventId);
                return await tcs.Task;
            }
        }

        [Serializable, Preserve]
        private class EventArrayWrapper : IArrayWrapper<GamesEvent>
        {
            public GamesEvent[] items;
            public GamesEvent[] Items => items;
        }

        internal void OnEventsLoadedFromJava(string eventsJson)
        {
            try
            {
                var events = JsonArrayParser.Parse<EventArrayWrapper, GamesEvent>(eventsJson);
                _loadAllTcs?.TrySetResult(events);
            }
            catch (Exception ex)
            {
                _loadAllTcs?.TrySetException(ex);
            }
        }

        internal void OnEventLoadedFromJava(string eventJson)
        {
            try
            {
                var ev = JsonUtility.FromJson<GamesEvent>(eventJson);
                _loadOneTcs?.TrySetResult(ev);
            }
            catch (Exception ex)
            {
                _loadOneTcs?.TrySetException(ex);
            }
        }

        internal void OnEventsErrorFromJava(int errorCode, string message)
        {
            var error = new GamesEventsError
            {
                errorCode = errorCode,
                message = message
            };
            OnEventsError?.Invoke(error);

            var exception = new GamesEventsException(error);
            _loadAllTcs?.TrySetException(exception);
            _loadOneTcs?.TrySetException(exception);
        }

        protected override void OnDispose()
        {
            FlushPendingIncrements();

            _loadAllTcs?.TrySetCanceled();
            _loadOneTcs?.TrySetCanceled();
            _callbackProxy = null;
        }
    }
}
