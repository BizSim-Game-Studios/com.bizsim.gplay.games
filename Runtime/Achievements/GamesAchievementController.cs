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
    internal class GamesAchievementController : JniBridgeBase, IGamesAchievementProvider
    {
        private const string CACHE_PREFIX = "BizSimGames_Achievement_";
        private const int CACHE_LIFETIME_HOURS = 24;

        private AchievementCallbackProxy _callbackProxy;

        private readonly Dictionary<string, TaskCompletionSource<bool>> _pendingUnlocks = new();
        private readonly Dictionary<string, TaskCompletionSource<bool>> _pendingIncrements = new();
        private readonly Dictionary<string, TaskCompletionSource<bool>> _pendingReveals = new();
        private TaskCompletionSource<bool> _showUITcs;
        private TaskCompletionSource<List<GamesAchievement>> _loadTcs;
        private TaskCompletionSource<bool> _unlockMultipleTcs;

        private Dictionary<string, GamesAchievement> _achievementCache;
        private DateTime _cacheTimestamp;

        public event Action<string> OnAchievementUnlocked;
        public event Action<string, int> OnAchievementIncremented;
        public event Action<string> OnAchievementRevealed;
        public event Action<GamesAchievementError> OnAchievementError;

        protected override string JavaClassName => JniConstants.AchievementBridge;

        protected override AndroidJavaProxy CreateCallbackProxy()
        {
            _callbackProxy = new AchievementCallbackProxy(this);
            return _callbackProxy;
        }

        public GamesAchievementController()
        {
            _achievementCache = new Dictionary<string, GamesAchievement>();
            _cacheTimestamp = DateTime.MinValue;
            InitializeBridge();
        }

        #region Public API

        public async Task UnlockAchievementAsync(string achievementId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(achievementId))
                throw new ArgumentException("Achievement ID cannot be null or empty", nameof(achievementId));

            if (IsAchievementUnlockedInCache(achievementId))
            {
                BizSimGamesLogger.Info($"Achievement {achievementId} already unlocked (cached)");
                return;
            }

            var tcs = new TaskCompletionSource<bool>();
            lock (_pendingUnlocks)
            {
                if (_pendingUnlocks.TryGetValue(achievementId, out var existing))
                    existing.TrySetCanceled();
                _pendingUnlocks[achievementId] = tcs;
            }

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Unlocking achievement: {achievementId}");
                CallBridge("unlockAchievement", achievementId);
                await tcs.Task;
            }
        }

        public async Task IncrementAchievementAsync(string achievementId, int steps, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(achievementId))
                throw new ArgumentException("Achievement ID cannot be null or empty", nameof(achievementId));

            if (steps <= 0)
                throw new ArgumentException("Steps must be greater than 0", nameof(steps));

            var tcs = new TaskCompletionSource<bool>();
            lock (_pendingIncrements)
            {
                if (_pendingIncrements.TryGetValue(achievementId, out var existing))
                    existing.TrySetCanceled();
                _pendingIncrements[achievementId] = tcs;
            }

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Incrementing achievement: {achievementId} by {steps}");
                CallBridge("incrementAchievement", achievementId, steps);
                await tcs.Task;
            }
        }

        public async Task RevealAchievementAsync(string achievementId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(achievementId))
                throw new ArgumentException("Achievement ID cannot be null or empty", nameof(achievementId));

            var tcs = new TaskCompletionSource<bool>();
            lock (_pendingReveals)
            {
                if (_pendingReveals.TryGetValue(achievementId, out var existing))
                    existing.TrySetCanceled();
                _pendingReveals[achievementId] = tcs;
            }

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Revealing achievement: {achievementId}");
                CallBridge("revealAchievement", achievementId);
                await tcs.Task;
            }
        }

        public async Task ShowAchievementsUIAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var tcs = TcsGuard.Replace(ref _showUITcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info("Showing achievements UI");
                CallBridge("showAchievementsUI");
                await tcs.Task;
            }
        }

        public async Task<List<GamesAchievement>> LoadAchievementsAsync(bool forceReload = false, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (!forceReload && IsCacheValid())
            {
                BizSimGamesLogger.Info("Returning cached achievements");
                return _achievementCache.Values.ToList();
            }

            var tcs = TcsGuard.Replace(ref _loadTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Loading achievements (forceReload: {forceReload})");
                CallBridge("loadAchievements", forceReload);
                return await tcs.Task;
            }
        }

        public async Task UnlockMultipleAsync(List<string> achievementIds, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (achievementIds == null || achievementIds.Count == 0)
                throw new ArgumentException("Achievement IDs list cannot be null or empty", nameof(achievementIds));

            var tcs = TcsGuard.Replace(ref _unlockMultipleTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Unlocking {achievementIds.Count} achievements in batch");
                string json = "[\"" + string.Join("\",\"", achievementIds) + "\"]";
                CallBridge("unlockMultiple", json);
                await tcs.Task;
            }
        }

        #endregion

        #region Callback Handlers (called from Java via proxy)

        internal void OnAchievementUnlockedFromJava(string achievementId)
        {
            if (_achievementCache.ContainsKey(achievementId))
            {
                _achievementCache[achievementId].state = AchievementState.Unlocked;
                _achievementCache[achievementId].unlockedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            PlayerPrefs.SetString(CACHE_PREFIX + achievementId, "unlocked");
            PlayerPrefs.Save();

            OnAchievementUnlocked?.Invoke(achievementId);

            ResolvePending(_pendingUnlocks, achievementId);
            _unlockMultipleTcs?.TrySetResult(true);
        }

        internal void OnAchievementIncrementedFromJava(string achievementId, int currentSteps, int totalSteps)
        {
            if (_achievementCache.ContainsKey(achievementId))
            {
                _achievementCache[achievementId].currentSteps = currentSteps;
                _achievementCache[achievementId].totalSteps = totalSteps;

                if (currentSteps >= totalSteps)
                    _achievementCache[achievementId].state = AchievementState.Unlocked;
            }

            OnAchievementIncremented?.Invoke(achievementId, currentSteps);
            ResolvePending(_pendingIncrements, achievementId);
        }

        internal void OnAchievementRevealedFromJava(string achievementId)
        {
            if (_achievementCache.ContainsKey(achievementId))
                _achievementCache[achievementId].state = AchievementState.Revealed;

            OnAchievementRevealed?.Invoke(achievementId);
            ResolvePending(_pendingReveals, achievementId);
        }

        internal void OnAchievementsLoadedFromJava(string achievementsJson)
        {
            try
            {
                var achievements = ParseAchievementsJson(achievementsJson);

                _achievementCache.Clear();
                foreach (var achievement in achievements)
                    _achievementCache[achievement.achievementId] = achievement;
                _cacheTimestamp = DateTime.UtcNow;

                BizSimGamesLogger.Info($"Loaded {achievements.Count} achievements");
                _loadTcs?.TrySetResult(achievements);
            }
            catch (Exception ex)
            {
                BizSimGamesLogger.Error($"Failed to parse achievements JSON: {ex.Message}");
                _loadTcs?.TrySetException(ex);
            }
        }

        internal void OnAchievementsUIClosedFromJava()
        {
            _showUITcs?.TrySetResult(true);
        }

        internal void OnAchievementErrorFromJava(int errorCode, string errorMessage, string achievementId)
        {
            var error = new GamesAchievementError(errorCode, errorMessage, achievementId);
            OnAchievementError?.Invoke(error);

            var exception = new GamesAchievementException(error);

            if (!string.IsNullOrEmpty(achievementId))
            {
                FailPending(_pendingUnlocks, achievementId, exception);
                FailPending(_pendingIncrements, achievementId, exception);
                FailPending(_pendingReveals, achievementId, exception);
            }
            else
            {
                FailAllPending(_pendingUnlocks, exception);
                FailAllPending(_pendingIncrements, exception);
                FailAllPending(_pendingReveals, exception);
            }

            _showUITcs?.TrySetException(exception);
            _loadTcs?.TrySetException(exception);
            _unlockMultipleTcs?.TrySetException(exception);
        }

        #endregion

        #region Pending Operation Helpers

        private static void ResolvePending(Dictionary<string, TaskCompletionSource<bool>> dict, string id)
        {
            TaskCompletionSource<bool> tcs;
            lock (dict)
            {
                if (!dict.TryGetValue(id, out tcs))
                    return;
                dict.Remove(id);
            }
            tcs.TrySetResult(true);
        }

        private static void FailPending(Dictionary<string, TaskCompletionSource<bool>> dict, string id, Exception ex)
        {
            TaskCompletionSource<bool> tcs;
            lock (dict)
            {
                if (!dict.TryGetValue(id, out tcs))
                    return;
                dict.Remove(id);
            }
            tcs.TrySetException(ex);
        }

        private static void FailAllPending(Dictionary<string, TaskCompletionSource<bool>> dict, Exception ex)
        {
            List<TaskCompletionSource<bool>> pending;
            lock (dict)
            {
                pending = new List<TaskCompletionSource<bool>>(dict.Values);
                dict.Clear();
            }
            foreach (var tcs in pending)
                tcs.TrySetException(ex);
        }

        private static void CancelAllPending(Dictionary<string, TaskCompletionSource<bool>> dict)
        {
            List<TaskCompletionSource<bool>> pending;
            lock (dict)
            {
                pending = new List<TaskCompletionSource<bool>>(dict.Values);
                dict.Clear();
            }
            foreach (var tcs in pending)
                tcs.TrySetCanceled();
        }

        #endregion

        #region Helper Methods

        private bool IsAchievementUnlockedInCache(string achievementId)
        {
            if (_achievementCache.ContainsKey(achievementId))
                return _achievementCache[achievementId].state == AchievementState.Unlocked;

            if (PlayerPrefs.HasKey(CACHE_PREFIX + achievementId))
                return PlayerPrefs.GetString(CACHE_PREFIX + achievementId) == "unlocked";

            return false;
        }

        private bool IsCacheValid()
        {
            if (_achievementCache.Count == 0)
                return false;

            var age = DateTime.UtcNow - _cacheTimestamp;
            return age.TotalHours < CACHE_LIFETIME_HOURS;
        }

        [Serializable, Preserve]
        private class AchievementArrayWrapper : IArrayWrapper<GamesAchievement>
        {
            public GamesAchievement[] items;
            public GamesAchievement[] Items => items;
        }

        private List<GamesAchievement> ParseAchievementsJson(string json)
        {
            try
            {
                var items = JsonArrayParser.Parse<AchievementArrayWrapper, GamesAchievement>(json);
                return new List<GamesAchievement>(items);
            }
            catch (Exception ex)
            {
                BizSimGamesLogger.Error($"JSON parse error: {ex.Message}");
                throw;
            }
        }

        #endregion

        protected override void OnDispose()
        {
            CancelAllPending(_pendingUnlocks);
            CancelAllPending(_pendingIncrements);
            CancelAllPending(_pendingReveals);
            _showUITcs?.TrySetCanceled();
            _loadTcs?.TrySetCanceled();
            _unlockMultipleTcs?.TrySetCanceled();

            _achievementCache?.Clear();
            _callbackProxy = null;
        }
    }
}
