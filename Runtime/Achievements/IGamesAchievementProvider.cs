// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Provider interface for Google Play Games Achievements.
    /// Supports unlock, increment, reveal, and UI display operations.
    /// </summary>
    public interface IGamesAchievementProvider
    {
        /// <summary>
        /// Unlocks an achievement immediately.
        /// </summary>
        /// <param name="achievementId">The achievement ID from games-ids.xml</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when achievement is unlocked</returns>
        Task UnlockAchievementAsync(string achievementId, CancellationToken ct = default);

        /// <summary>
        /// Increments an incremental achievement by the specified number of steps.
        /// </summary>
        /// <param name="achievementId">The achievement ID from games-ids.xml</param>
        /// <param name="steps">Number of steps to increment (must be > 0)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when increment is processed</returns>
        Task IncrementAchievementAsync(string achievementId, int steps, CancellationToken ct = default);

        /// <summary>
        /// Reveals a hidden achievement (makes it visible to the player).
        /// </summary>
        /// <param name="achievementId">The achievement ID from games-ids.xml</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when achievement is revealed</returns>
        Task RevealAchievementAsync(string achievementId, CancellationToken ct = default);

        /// <summary>
        /// Shows the native Google Play Games achievements UI.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when UI is closed</returns>
        Task ShowAchievementsUIAsync(CancellationToken ct = default);

        /// <summary>
        /// Loads all achievements for the current player.
        /// </summary>
        /// <param name="forceReload">If true, bypasses cache and fetches fresh data from server</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of achievements with current state</returns>
        Task<List<GamesAchievement>> LoadAchievementsAsync(bool forceReload = false, CancellationToken ct = default);

        /// <summary>
        /// Unlocks multiple achievements in a single batch operation.
        /// More efficient than multiple UnlockAchievementAsync calls.
        /// </summary>
        /// <param name="achievementIds">List of achievement IDs to unlock</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when all unlocks are processed</returns>
        Task UnlockMultipleAsync(List<string> achievementIds, CancellationToken ct = default);

        /// <summary>
        /// Event fired when an achievement is successfully unlocked.
        /// </summary>
        event Action<string> OnAchievementUnlocked;

        /// <summary>
        /// Event fired when an incremental achievement is updated.
        /// </summary>
        event Action<string, int> OnAchievementIncremented;

        /// <summary>
        /// Event fired when a hidden achievement is revealed.
        /// </summary>
        event Action<string> OnAchievementRevealed;

        /// <summary>
        /// Event fired when an achievement operation fails.
        /// </summary>
        event Action<GamesAchievementError> OnAchievementError;
    }
}
