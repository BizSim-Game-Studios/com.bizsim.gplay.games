// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class GamesLeaderboardController : IGamesLeaderboardProvider
    {
        private AndroidJavaObject _leaderboardBridge;
        private LeaderboardCallbackProxy _callbackProxy;

        private TaskCompletionSource<bool> _submitTcs;
        private TaskCompletionSource<bool> _showUITcs;
        private TaskCompletionSource<List<GamesLeaderboardEntry>> _loadTcs;

        public event Action<string, long> OnScoreSubmitted;
        public event Action<string, List<GamesLeaderboardEntry>> OnScoresLoaded;
        public event Action<GamesLeaderboardError> OnLeaderboardError;

        public GamesLeaderboardController()
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
                    _leaderboardBridge = new AndroidJavaObject(
                        "com.bizsim.gplay.games.leaderboards.LeaderboardBridge", activity);
                    _callbackProxy = new LeaderboardCallbackProxy(this);
                    _leaderboardBridge.Call("setCallback", _callbackProxy);
                    BizSimGamesLogger.Info("LeaderboardBridge initialized");
                }
            }
            catch (Exception ex)
            {
                BizSimGamesLogger.Error($"Failed to initialize LeaderboardBridge: {ex.Message}");
                throw;
            }
        }

        public async Task SubmitScoreAsync(string leaderboardId, long score, string scoreTag = null, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _submitTcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _submitTcs.TrySetCanceled()))
            {
                _leaderboardBridge.Call("submitScore", leaderboardId, score, scoreTag ?? "");
                await _submitTcs.Task;
            }
        }

        public async Task ShowLeaderboardUIAsync(string leaderboardId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _showUITcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _showUITcs.TrySetCanceled()))
            {
                _leaderboardBridge.Call("showLeaderboardUI", leaderboardId);
                await _showUITcs.Task;
            }
        }

        public async Task ShowAllLeaderboardsUIAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _showUITcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _showUITcs.TrySetCanceled()))
            {
                _leaderboardBridge.Call("showAllLeaderboardsUI");
                await _showUITcs.Task;
            }
        }

        public async Task<List<GamesLeaderboardEntry>> LoadTopScoresAsync(string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _loadTcs = new TaskCompletionSource<List<GamesLeaderboardEntry>>();

            using (ct.Register(() => _loadTcs.TrySetCanceled()))
            {
                _leaderboardBridge.Call("loadTopScores", leaderboardId, (int)timeSpan, (int)collection, maxResults);
                return await _loadTcs.Task;
            }
        }

        public async Task<List<GamesLeaderboardEntry>> LoadPlayerCenteredScoresAsync(string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _loadTcs = new TaskCompletionSource<List<GamesLeaderboardEntry>>();

            using (ct.Register(() => _loadTcs.TrySetCanceled()))
            {
                _leaderboardBridge.Call("loadPlayerCenteredScores", leaderboardId, (int)timeSpan, (int)collection, maxResults);
                return await _loadTcs.Task;
            }
        }

        internal void OnScoreSubmittedFromJava(string leaderboardId, long score)
        {
            OnScoreSubmitted?.Invoke(leaderboardId, score);
            _submitTcs?.TrySetResult(true);
        }

        internal void OnScoresLoadedFromJava(string leaderboardId, string scoresJson)
        {
            try
            {
                var scores = JsonUtility.FromJson<LeaderboardScoresList>("{\"items\":" + scoresJson + "}").items.ToList();
                OnScoresLoaded?.Invoke(leaderboardId, scores);
                _loadTcs?.TrySetResult(scores);
            }
            catch (Exception ex)
            {
                _loadTcs?.TrySetException(ex);
            }
        }

        internal void OnLeaderboardUIClosedFromJava()
        {
            _showUITcs?.TrySetResult(true);
        }

        internal void OnLeaderboardErrorFromJava(int errorCode, string errorMessage, string leaderboardId)
        {
            var error = new GamesLeaderboardError(errorCode, errorMessage, leaderboardId);
            OnLeaderboardError?.Invoke(error);

            var exception = new Exception(error.ToString());
            _submitTcs?.TrySetException(exception);
            _showUITcs?.TrySetException(exception);
            _loadTcs?.TrySetException(exception);
        }

        [Serializable]
        private class LeaderboardScoresList
        {
            public GamesLeaderboardEntry[] items;
        }
    }
}
