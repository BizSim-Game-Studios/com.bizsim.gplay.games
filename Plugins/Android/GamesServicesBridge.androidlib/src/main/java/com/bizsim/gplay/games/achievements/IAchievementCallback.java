// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.achievements;

/**
 * Callback interface for achievement operations.
 * Implemented in C# via AndroidJavaProxy (ProGuard-safe).
 */
public interface IAchievementCallback {
    /**
     * Called when an achievement is successfully unlocked.
     * @param achievementId The unlocked achievement ID
     */
    void onAchievementUnlocked(String achievementId);

    /**
     * Called when an incremental achievement is successfully incremented.
     * @param achievementId The achievement ID
     * @param currentSteps Current progress steps
     * @param totalSteps Total steps required
     */
    void onAchievementIncremented(String achievementId, int currentSteps, int totalSteps);

    /**
     * Called when a hidden achievement is successfully revealed.
     * @param achievementId The revealed achievement ID
     */
    void onAchievementRevealed(String achievementId);

    /**
     * Called when achievements are successfully loaded.
     * @param achievementsJson JSON array of achievement data
     */
    void onAchievementsLoaded(String achievementsJson);

    /**
     * Called when the achievements UI is closed.
     */
    void onAchievementsUIClosed();

    /**
     * Called when an achievement operation fails.
     * @param errorCode Error code from Google Play Games
     * @param errorMessage Human-readable error message
     * @param achievementId Achievement ID that caused the error (null if general error)
     */
    void onAchievementError(int errorCode, String errorMessage, String achievementId);
}
