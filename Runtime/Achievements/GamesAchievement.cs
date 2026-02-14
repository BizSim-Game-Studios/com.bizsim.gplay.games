// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine.Scripting;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Represents a Google Play Games achievement and its current state.
    /// </summary>
    [Serializable, Preserve]
    public class GamesAchievement
    {
        /// <summary>
        /// Achievement ID (from games-ids.xml).
        /// </summary>
        public string achievementId;

        /// <summary>
        /// Achievement name (localized).
        /// </summary>
        public string name;

        /// <summary>
        /// Achievement description (localized).
        /// </summary>
        public string description;

        /// <summary>
        /// Current state of the achievement.
        /// </summary>
        public AchievementState state;

        /// <summary>
        /// Achievement type (standard or incremental).
        /// </summary>
        public AchievementType type;

        /// <summary>
        /// Current steps (for incremental achievements).
        /// </summary>
        public int currentSteps;

        /// <summary>
        /// Total steps required to unlock (for incremental achievements).
        /// </summary>
        public int totalSteps;

        /// <summary>
        /// XP value awarded when unlocked.
        /// </summary>
        public int xpValue;

        /// <summary>
        /// Timestamp when achievement was unlocked (Unix milliseconds).
        /// 0 if not unlocked.
        /// </summary>
        public long unlockedTimestamp;

        /// <summary>
        /// URL to achievement icon (revealed state).
        /// </summary>
        public string revealedIconUrl;

        /// <summary>
        /// URL to achievement icon (unlocked state).
        /// </summary>
        public string unlockedIconUrl;

        /// <summary>
        /// Whether this achievement is unlocked.
        /// </summary>
        public bool IsUnlocked => state == AchievementState.Unlocked;

        /// <summary>
        /// Whether this achievement is revealed (visible to player).
        /// </summary>
        public bool IsRevealed => state != AchievementState.Hidden;

        /// <summary>
        /// Progress percentage for incremental achievements (0-100).
        /// </summary>
        public float ProgressPercentage
        {
            get
            {
                if (type != AchievementType.Incremental || totalSteps == 0)
                    return 0f;

                return (float)currentSteps / totalSteps * 100f;
            }
        }
    }

    /// <summary>
    /// Achievement state in Google Play Games.
    /// </summary>
    public enum AchievementState
    {
        /// <summary>
        /// Achievement exists but is not visible to the player.
        /// </summary>
        Hidden = 0,

        /// <summary>
        /// Achievement is visible to the player but not unlocked.
        /// </summary>
        Revealed = 1,

        /// <summary>
        /// Achievement has been unlocked.
        /// </summary>
        Unlocked = 2
    }

    /// <summary>
    /// Achievement type in Google Play Games.
    /// </summary>
    public enum AchievementType
    {
        /// <summary>
        /// Standard achievement (unlock in one action).
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Incremental achievement (requires multiple steps).
        /// </summary>
        Incremental = 1
    }
}
