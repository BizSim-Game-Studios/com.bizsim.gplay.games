// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace BizSim.GPlay.Games
{
    internal class GamesLeaderboardController : JniBridgeBase, IGamesLeaderboardProvider
    {
        private LeaderboardCallbackProxy _callbackProxy;

        private TaskCompletionSource<bool> _submitTcs;
        private TaskCompletionSource<bool> _showUITcs;
        private TaskCompletionSource<List<GamesLeaderboardEntry>> _loadTcs;

        public event Action<string, long> OnScoreSubmitted;
        public event Action<string, List<GamesLeaderboardEntry>> OnScoresLoaded;
        public event Action<GamesLeaderboardError> OnLeaderboardError;

        protected override string JavaClassName => JniConstants.LeaderboardBridge;

        protected override AndroidJavaProxy CreateCallbackProxy()
        {
            _callbackProxy = new LeaderboardCallbackProxy(this);
            return _callbackProxy;
        }

        public GamesLeaderboardController()
        {
            InitializeBridge();
        }

        public async Task SubmitScoreAsync(string leaderboardId, long score, string scoreTag = null, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _submitTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("submitScore", leaderboardId, score, scoreTag ?? "");
                await tcs.Task;
            }
        }

        public async Task ShowLeaderboardUIAsync(string leaderboardId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _showUITcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("showLeaderboardUI", leaderboardId);
                await tcs.Task;
            }
        }

        public async Task ShowAllLeaderboardsUIAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _showUITcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("showAllLeaderboardsUI");
                await tcs.Task;
            }
        }

        public async Task<List<GamesLeaderboardEntry>> LoadTopScoresAsync(string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _loadTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("loadTopScores", leaderboardId, (int)timeSpan, (int)collection, maxResults);
                return await tcs.Task;
            }
        }

        public async Task<List<GamesLeaderboardEntry>> LoadPlayerCenteredScoresAsync(string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _loadTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("loadPlayerCenteredScores", leaderboardId, (int)timeSpan, (int)collection, maxResults);
                return await tcs.Task;
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
                var items = JsonArrayParser.Parse<LeaderboardScoresList, GamesLeaderboardEntry>(scoresJson);
                var scores = items.ToList();
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

            var exception = new GamesLeaderboardException(error);
            _submitTcs?.TrySetException(exception);
            _showUITcs?.TrySetException(exception);
            _loadTcs?.TrySetException(exception);
        }

        [Serializable, Preserve]
        private class LeaderboardScoresList : IArrayWrapper<GamesLeaderboardEntry>
        {
            public GamesLeaderboardEntry[] items;
            public GamesLeaderboardEntry[] Items => items;
        }

        protected override void OnDispose()
        {
            _submitTcs?.TrySetCanceled();
            _showUITcs?.TrySetCanceled();
            _loadTcs?.TrySetCanceled();
            _callbackProxy = null;
        }
    }
}
