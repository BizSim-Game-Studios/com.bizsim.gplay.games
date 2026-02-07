// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class GamesStatsController : IGamesStatsProvider
    {
        private AndroidJavaObject _statsBridge;
        private StatsCallbackProxy _callbackProxy;
        private TaskCompletionSource<GamesPlayerStats> _loadTcs;

        public event Action<GamesPlayerStats> OnStatsLoaded;
        public event Action<GamesStatsError> OnStatsError;

        public GamesStatsController()
        {
            InitializeBridge();
        }

        private void InitializeBridge()
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _statsBridge = new AndroidJavaObject("com.bizsim.gplay.games.stats.StatsBridge", activity);
                    _callbackProxy = new StatsCallbackProxy(this);
                    _statsBridge.Call("setCallback", _callbackProxy);
                    BizSimGamesLogger.Info("StatsBridge initialized");
                }
            }
            catch (Exception ex)
            {
                BizSimGamesLogger.Error($"Failed to initialize StatsBridge: {ex.Message}");
                throw;
            }
        }

        public async Task<GamesPlayerStats> LoadPlayerStatsAsync(bool forceReload = false, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _loadTcs = new TaskCompletionSource<GamesPlayerStats>();

            using (ct.Register(() => _loadTcs.TrySetCanceled()))
            {
                _statsBridge.Call("loadPlayerStats", forceReload);
                return await _loadTcs.Task;
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
            _loadTcs?.TrySetException(new Exception(error.ToString()));
        }
    }
}
