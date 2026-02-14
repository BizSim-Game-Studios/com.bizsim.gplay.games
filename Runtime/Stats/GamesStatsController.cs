// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class GamesStatsController : JniBridgeBase, IGamesStatsProvider
    {
        private StatsCallbackProxy _callbackProxy;
        private TaskCompletionSource<GamesPlayerStats> _loadTcs;

        public event Action<GamesPlayerStats> OnStatsLoaded;
        public event Action<GamesStatsError> OnStatsError;

        protected override string JavaClassName => JniConstants.StatsBridge;

        protected override AndroidJavaProxy CreateCallbackProxy()
        {
            _callbackProxy = new StatsCallbackProxy(this);
            return _callbackProxy;
        }

        public GamesStatsController()
        {
            InitializeBridge();
        }

        public async Task<GamesPlayerStats> LoadPlayerStatsAsync(bool forceReload = false, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _loadTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("loadPlayerStats", forceReload);
                return await tcs.Task;
            }
        }

        internal void OnStatsLoadedFromJava(string statsJson)
        {
            try
            {
                var stats = JsonUtility.FromJson<GamesPlayerStats>(statsJson);
                OnStatsLoaded?.Invoke(stats);
                _loadTcs?.TrySetResult(stats);
            }
            catch (Exception ex)
            {
                _loadTcs?.TrySetException(ex);
            }
        }

        internal void OnStatsErrorFromJava(int errorCode, string errorMessage)
        {
            var error = new GamesStatsError(errorCode, errorMessage);
            OnStatsError?.Invoke(error);
            _loadTcs?.TrySetException(new GamesStatsException(error));
        }

        protected override void OnDispose()
        {
            _loadTcs?.TrySetCanceled();
            _callbackProxy = null;
        }
    }
}
