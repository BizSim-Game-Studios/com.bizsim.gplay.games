// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// AndroidJavaProxy implementation for achievement callbacks.
    /// Routes callbacks from Java to C# on the main Unity thread.
    /// </summary>
    internal class AchievementCallbackProxy : AndroidJavaProxy
    {
        private readonly GamesAchievementController _controller;

        public AchievementCallbackProxy(GamesAchievementController controller)
            : base("com.bizsim.gplay.games.achievements.IAchievementCallback")
        {
            _controller = controller;
        }

        // Note: These methods are called from Java on background thread
        // Must marshal to Unity main thread via UnityMainThreadDispatcher

        void onAchievementUnlocked(string achievementId)
        {
            BizSimGamesLogger.Info($"Achievement unlocked callback: {achievementId}");
            UnityMainThreadDispatcher.Enqueue(() => {
                _controller.OnAchievementUnlockedFromJava(achievementId);
            });
        }

        void onAchievementIncremented(string achievementId, int currentSteps, int totalSteps)
        {
            BizSimGamesLogger.Info($"Achievement incremented callback: {achievementId} ({currentSteps}/{totalSteps})");
            UnityMainThreadDispatcher.Enqueue(() => {
                _controller.OnAchievementIncrementedFromJava(achievementId, currentSteps, totalSteps);
            });
        }

        void onAchievementRevealed(string achievementId)
        {
            BizSimGamesLogger.Info($"Achievement revealed callback: {achievementId}");
            UnityMainThreadDispatcher.Enqueue(() => {
                _controller.OnAchievementRevealedFromJava(achievementId);
            });
        }

        void onAchievementsLoaded(string achievementsJson)
        {
            BizSimGamesLogger.Info($"Achievements loaded callback (JSON length: {achievementsJson?.Length ?? 0})");
            UnityMainThreadDispatcher.Enqueue(() => {
                _controller.OnAchievementsLoadedFromJava(achievementsJson);
            });
        }

        void onAchievementsUIClosed()
        {
            BizSimGamesLogger.Info("Achievements UI closed callback");
            UnityMainThreadDispatcher.Enqueue(() => {
                _controller.OnAchievementsUIClosedFromJava();
            });
        }

        void onAchievementError(int errorCode, string errorMessage, string achievementId)
        {
            BizSimGamesLogger.Error($"Achievement error callback: {errorCode} - {errorMessage} (Achievement: {achievementId})");
            UnityMainThreadDispatcher.Enqueue(() => {
                _controller.OnAchievementErrorFromJava(errorCode, errorMessage, achievementId);
            });
        }
    }
}
