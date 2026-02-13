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
    /// Mock implementation of achievements provider for Unity Editor testing.
    /// Uses GamesServicesMockConfig for test data.
    /// </summary>
    internal class MockAchievementProvider : IGamesAchievementProvider
    {
        private Dictionary<string, GamesAchievement> _achievements;
        private HashSet<string> _unlockedAchievements;

        public event Action<string> OnAchievementUnlocked;
        public event Action<string, int> OnAchievementIncremented;
        public event Action<string> OnAchievementRevealed;
        public event Action<GamesAchievementError> OnAchievementError;

        public MockAchievementProvider(GamesServicesConfig.MockSettings mock)
        {
            InitializeMockData();
            BizSimGamesLogger.Info("MockAchievementProvider initialized");
        }

        private void InitializeMockData()
        {
            _achievements = new Dictionary<string, GamesAchievement>();
            _unlockedAchievements = new HashSet<string>();

            // Create mock achievements
            var mockAchievements = new List<GamesAchievement>
            {
                new GamesAchievement
                {
                    achievementId = "achievement_first_trade",
                    name = "First Trade",
                    description = "Complete your first trade",
                    state = AchievementState.Revealed,
                    type = AchievementType.Standard,
                    xpValue = 10,
                    totalSteps = 1
                },
                new GamesAchievement
                {
                    achievementId = "achievement_ten_trades",
                    name = "Ten Trades",
                    description = "Complete 10 trades",
                    state = AchievementState.Hidden,
                    type = AchievementType.Incremental,
                    currentSteps = 0,
                    totalSteps = 10,
                    xpValue = 50
                },
                new GamesAchievement
                {
                    achievementId = "achievement_hundred_trades",
                    name = "Trade Master",
                    description = "Complete 100 trades",
                    state = AchievementState.Hidden,
                    type = AchievementType.Incremental,
                    currentSteps = 0,
                    totalSteps = 100,
                    xpValue = 100
                },
                new GamesAchievement
                {
                    achievementId = "achievement_scrap_circuit_specialist",
                    name = "Scrap Circuit Specialist",
                    description = "Unlock scrap circuit specialist achievement",
                    state = AchievementState.Revealed,
                    type = AchievementType.Standard,
                    xpValue = 25,
                    totalSteps = 1
                },
                new GamesAchievement
                {
                    achievementId = "achievement_amazing_profit",
                    name = "Amazing Profit",
                    description = "Make an amazing profit on a single trade",
                    state = AchievementState.Hidden,
                    type = AchievementType.Standard,
                    xpValue = 50,
                    totalSteps = 1
                }
            };

            foreach (var achievement in mockAchievements)
            {
                _achievements[achievement.achievementId] = achievement;
            }
        }

        public async Task UnlockAchievementAsync(string achievementId, CancellationToken ct = default)
        {
            await Task.Delay(200, ct); // Simulate network delay

            if (!_achievements.ContainsKey(achievementId))
            {
                var error = new GamesAchievementError(3, "Achievement not found", achievementId);
                OnAchievementError?.Invoke(error);
                throw new Exception(error.ToString());
            }

            var achievement = _achievements[achievementId];

            if (achievement.state == AchievementState.Unlocked)
            {
                BizSimGamesLogger.Warning($"Achievement {achievementId} already unlocked");
                return;
            }

            achievement.state = AchievementState.Unlocked;
            achievement.unlockedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _unlockedAchievements.Add(achievementId);

            BizSimGamesLogger.Info($"[MOCK] Achievement unlocked: {achievementId}");
            OnAchievementUnlocked?.Invoke(achievementId);
        }

        public async Task IncrementAchievementAsync(string achievementId, int steps, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);

            if (!_achievements.ContainsKey(achievementId))
            {
                var error = new GamesAchievementError(3, "Achievement not found", achievementId);
                OnAchievementError?.Invoke(error);
                throw new Exception(error.ToString());
            }

            var achievement = _achievements[achievementId];

            if (achievement.type != AchievementType.Incremental)
            {
                var error = new GamesAchievementError(4, "Achievement is not incremental", achievementId);
                OnAchievementError?.Invoke(error);
                throw new Exception(error.ToString());
            }

            achievement.currentSteps = Mathf.Min(achievement.currentSteps + steps, achievement.totalSteps);

            if (achievement.currentSteps >= achievement.totalSteps && achievement.state != AchievementState.Unlocked)
            {
                achievement.state = AchievementState.Unlocked;
                achievement.unlockedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _unlockedAchievements.Add(achievementId);
                OnAchievementUnlocked?.Invoke(achievementId);
            }

            BizSimGamesLogger.Info($"[MOCK] Achievement incremented: {achievementId} ({achievement.currentSteps}/{achievement.totalSteps})");
            OnAchievementIncremented?.Invoke(achievementId, achievement.currentSteps);
        }

        public async Task RevealAchievementAsync(string achievementId, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);

            if (!_achievements.ContainsKey(achievementId))
            {
                var error = new GamesAchievementError(3, "Achievement not found", achievementId);
                OnAchievementError?.Invoke(error);
                throw new Exception(error.ToString());
            }

            var achievement = _achievements[achievementId];

            if (achievement.state == AchievementState.Hidden)
            {
                achievement.state = AchievementState.Revealed;
                BizSimGamesLogger.Info($"[MOCK] Achievement revealed: {achievementId}");
                OnAchievementRevealed?.Invoke(achievementId);
            }
        }

        public async Task ShowAchievementsUIAsync(CancellationToken ct = default)
        {
            await Task.Delay(500, ct);
            BizSimGamesLogger.Info("[MOCK] Achievements UI shown (simulated)");
            Debug.Log("[MockAchievements] Native UI would show here on device");
        }

        public async Task<List<GamesAchievement>> LoadAchievementsAsync(bool forceReload = false, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);

            BizSimGamesLogger.Info($"[MOCK] Loading achievements (forceReload: {forceReload})");
            return _achievements.Values.ToList();
        }

        public async Task UnlockMultipleAsync(List<string> achievementIds, CancellationToken ct = default)
        {
            await Task.Delay(400, ct);

            foreach (var achievementId in achievementIds)
            {
                if (_achievements.ContainsKey(achievementId))
                {
                    var achievement = _achievements[achievementId];
                    if (achievement.state != AchievementState.Unlocked)
                    {
                        achievement.state = AchievementState.Unlocked;
                        achievement.unlockedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        _unlockedAchievements.Add(achievementId);
                        OnAchievementUnlocked?.Invoke(achievementId);
                    }
                }
            }

            BizSimGamesLogger.Info($"[MOCK] Unlocked {achievementIds.Count} achievements in batch");
        }
    }
}
