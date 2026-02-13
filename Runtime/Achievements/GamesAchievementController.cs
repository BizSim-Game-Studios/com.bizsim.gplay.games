// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Android implementation of achievements provider using Google Play Games SDK.
    /// </summary>
    internal class GamesAchievementController : IGamesAchievementProvider
    {
        private const string CACHE_PREFIX = "BizSimGames_Achievement_";
        private const int CACHE_LIFETIME_HOURS = 24;

        private AndroidJavaObject _achievementBridge;
        private AchievementCallbackProxy _callbackProxy;

        // TaskCompletionSource for async operations
        private TaskCompletionSource<bool> _unlockTcs;
        private TaskCompletionSource<bool> _incrementTcs;
        private TaskCompletionSource<bool> _revealTcs;
        private TaskCompletionSource<bool> _showUITcs;
        private TaskCompletionSource<List<GamesAchievement>> _loadTcs;
        private TaskCompletionSource<bool> _unlockMultipleTcs;

        // Local cache
        private Dictionary<string, GamesAchievement> _achievementCache;
        private DateTime _cacheTimestamp;

        // Events
        public event Action<string> OnAchievementUnlocked;
        public event Action<string, int> OnAchievementIncremented;
        public event Action<string> OnAchievementRevealed;
        public event Action<GamesAchievementError> OnAchievementError;

        public GamesAchievementController()
        {
            InitializeBridge();
        }

        private void InitializeBridge()
        {
            try
            {
                BizSimGamesLogger.Info("Initializing AchievementBridge...");

                // Get Unity activity
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // Create bridge
                    _achievementBridge = new AndroidJavaObject(
                        "com.bizsim.gplay.games.achievements.AchievementBridge",
                        activity);

                    // Create and register callback proxy
                    _callbackProxy = new AchievementCallbackProxy(this);
                    _achievementBridge.Call("setCallback", _callbackProxy);

                    BizSimGamesLogger.Info("AchievementBridge initialized successfully");
                }

                // Initialize cache
                _achievementCache = new Dictionary<string, GamesAchievement>();
                _cacheTimestamp = DateTime.MinValue;
            }
            catch (Exception ex)
            {
                BizSimGamesLogger.Error($"Failed to initialize AchievementBridge: {ex.Message}");
                throw;
            }
        }

        #region Public API

        public async Task UnlockAchievementAsync(string achievementId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(achievementId))
            {
                throw new ArgumentException("Achievement ID cannot be null or empty", nameof(achievementId));
            }

            // Check cache - avoid redundant unlock calls
            if (IsAchievementUnlockedInCache(achievementId))
            {
                BizSimGamesLogger.Info($"Achievement {achievementId} already unlocked (cached)");
                return;
            }

            _unlockTcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _unlockTcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Unlocking achievement: {achievementId}");
                _achievementBridge.Call("unlockAchievement", achievementId);

                await _unlockTcs.Task;
            }
        }

        public async Task IncrementAchievementAsync(string achievementId, int steps, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(achievementId))
            {
                throw new ArgumentException("Achievement ID cannot be null or empty", nameof(achievementId));
            }

            if (steps <= 0)
            {
                throw new ArgumentException("Steps must be greater than 0", nameof(steps));
            }

            _incrementTcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _incrementTcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Incrementing achievement: {achievementId} by {steps}");
                _achievementBridge.Call("incrementAchievement", achievementId, steps);

                await _incrementTcs.Task;
            }
        }

        public async Task RevealAchievementAsync(string achievementId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(achievementId))
            {
                throw new ArgumentException("Achievement ID cannot be null or empty", nameof(achievementId));
            }

            _revealTcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _revealTcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Revealing achievement: {achievementId}");
                _achievementBridge.Call("revealAchievement", achievementId);

                await _revealTcs.Task;
            }
        }

        public async Task ShowAchievementsUIAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            _showUITcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _showUITcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info("Showing achievements UI");
                _achievementBridge.Call("showAchievementsUI");

                await _showUITcs.Task;
            }
        }

        public async Task<List<GamesAchievement>> LoadAchievementsAsync(bool forceReload = false, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            // Check cache
            if (!forceReload && IsCacheValid())
            {
                BizSimGamesLogger.Info("Returning cached achievements");
                return _achievementCache.Values.ToList();
            }

            _loadTcs = new TaskCompletionSource<List<GamesAchievement>>();

            using (ct.Register(() => _loadTcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Loading achievements (forceReload: {forceReload})");
                _achievementBridge.Call("loadAchievements", forceReload);

                return await _loadTcs.Task;
            }
        }

        public async Task UnlockMultipleAsync(List<string> achievementIds, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (achievementIds == null || achievementIds.Count == 0)
            {
                throw new ArgumentException("Achievement IDs list cannot be null or empty", nameof(achievementIds));
            }

            _unlockMultipleTcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _unlockMultipleTcs.TrySetCanceled()))
            {
                BizSimGamesLogger.Info($"Unlocking {achievementIds.Count} achievements in batch");

                // Serialize to JSON array
                string json = "[\"" + string.Join("\",\"", achievementIds) + "\"]";
                _achievementBridge.Call("unlockMultiple", json);

                await _unlockMultipleTcs.Task;
            }
        }

        #endregion

        #region Callback Handlers (called from Java via proxy)

        internal void OnAchievementUnlockedFromJava(string achievementId)
        {
            // Update cache
            if (_achievementCache.ContainsKey(achievementId))
            {
                _achievementCache[achievementId].state = AchievementState.Unlocked;
                _achievementCache[achievementId].unlockedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            // Save to PlayerPrefs cache
            PlayerPrefs.SetString(CACHE_PREFIX + achievementId, "unlocked");
            PlayerPrefs.Save();

            // Fire event
            OnAchievementUnlocked?.Invoke(achievementId);

            // Complete task
            _unlockTcs?.TrySetResult(true);
            _unlockMultipleTcs?.TrySetResult(true);  // Batch unlock
        }

        internal void OnAchievementIncrementedFromJava(string achievementId, int currentSteps, int totalSteps)
        {
            // Update cache
            if (_achievementCache.ContainsKey(achievementId))
            {
                _achievementCache[achievementId].currentSteps = currentSteps;
                _achievementCache[achievementId].totalSteps = totalSteps;

                if (currentSteps >= totalSteps)
                {
                    _achievementCache[achievementId].state = AchievementState.Unlocked;
                }
            }

            // Fire event
            OnAchievementIncremented?.Invoke(achievementId, currentSteps);

            // Complete task
            _incrementTcs?.TrySetResult(true);
        }

        internal void OnAchievementRevealedFromJava(string achievementId)
        {
            // Update cache
            if (_achievementCache.ContainsKey(achievementId))
            {
                _achievementCache[achievementId].state = AchievementState.Revealed;
            }

            // Fire event
            OnAchievementRevealed?.Invoke(achievementId);

            // Complete task
            _revealTcs?.TrySetResult(true);
        }

        internal void OnAchievementsLoadedFromJava(string achievementsJson)
        {
            try
            {
                var achievements = ParseAchievementsJson(achievementsJson);

                // Update cache
                _achievementCache.Clear();
                foreach (var achievement in achievements)
                {
                    _achievementCache[achievement.achievementId] = achievement;
                }
                _cacheTimestamp = DateTime.UtcNow;

                BizSimGamesLogger.Info($"Loaded {achievements.Count} achievements");

                // Complete task
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

            // Fire event
            OnAchievementError?.Invoke(error);

            // Complete tasks with exception
            var exception = new Exception($"Achievement error {errorCode}: {errorMessage}");
            _unlockTcs?.TrySetException(exception);
            _incrementTcs?.TrySetException(exception);
            _revealTcs?.TrySetException(exception);
            _showUITcs?.TrySetException(exception);
            _loadTcs?.TrySetException(exception);
            _unlockMultipleTcs?.TrySetException(exception);
        }

        #endregion

        #region Helper Methods

        private bool IsAchievementUnlockedInCache(string achievementId)
        {
            // Check PlayerPrefs cache
            if (PlayerPrefs.HasKey(CACHE_PREFIX + achievementId))
            {
                return PlayerPrefs.GetString(CACHE_PREFIX + achievementId) == "unlocked";
            }

            // Check memory cache
            if (_achievementCache.ContainsKey(achievementId))
            {
                return _achievementCache[achievementId].state == AchievementState.Unlocked;
            }

            return false;
        }

        private bool IsCacheValid()
        {
            if (_achievementCache.Count == 0)
                return false;

            var age = DateTime.UtcNow - _cacheTimestamp;
            return age.TotalHours < CACHE_LIFETIME_HOURS;
        }

        [Serializable]
        private class AchievementArrayWrapper
        {
            public GamesAchievement[] items;
        }

        private List<GamesAchievement> ParseAchievementsJson(string json)
        {
            try
            {
                var wrappedJson = "{\"items\":" + json + "}";
                var wrapper = JsonUtility.FromJson<AchievementArrayWrapper>(wrappedJson);
                return wrapper?.items != null
                    ? new List<GamesAchievement>(wrapper.items)
                    : new List<GamesAchievement>();
            }
            catch (Exception ex)
            {
                BizSimGamesLogger.Error($"JSON parse error: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}
