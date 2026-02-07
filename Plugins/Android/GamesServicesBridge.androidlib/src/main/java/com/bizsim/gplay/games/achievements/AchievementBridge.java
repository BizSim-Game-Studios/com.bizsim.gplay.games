// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.achievements;

import android.app.Activity;
import android.util.Log;

import com.google.android.gms.games.AnnotatedData;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.AchievementsClient;
import com.google.android.gms.games.achievement.Achievement;
import com.google.android.gms.games.achievement.AchievementBuffer;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

/**
 * JNI bridge for Google Play Games Achievements (PGS v2).
 * Handles unlock, increment, reveal, load, and UI operations.
 *
 * PGS v2 notes:
 * - unlock(), reveal(), increment() are fire-and-forget (return void)
 * - load() returns Task<AnnotatedData<AchievementBuffer>>
 */
public class AchievementBridge {
    private static final String TAG = "BizSimGames.Achievements";

    private final Activity activity;
    private final AchievementsClient achievementsClient;
    private IAchievementCallback callback;

    public AchievementBridge(Activity activity) {
        this.activity = activity;
        this.achievementsClient = PlayGames.getAchievementsClient(activity);
        Log.d(TAG, "AchievementBridge initialized");
    }

    /**
     * Sets the callback for achievement events.
     * @param callback C# callback proxy (AndroidJavaProxy)
     */
    public void setCallback(IAchievementCallback callback) {
        this.callback = callback;
        Log.d(TAG, "Callback registered");
    }

    /**
     * Unlocks an achievement immediately.
     * PGS v2: Fire-and-forget — no Task returned, callback fires immediately.
     * @param achievementId Achievement ID from games-ids.xml
     */
    public void unlockAchievement(final String achievementId) {
        if (achievementId == null || achievementId.isEmpty()) {
            sendError(-1, "Achievement ID cannot be null or empty", null);
            return;
        }

        Log.d(TAG, "Unlocking achievement: " + achievementId);

        try {
            achievementsClient.unlock(achievementId);
            Log.d(TAG, "Achievement unlock sent: " + achievementId);
            if (callback != null) {
                callback.onAchievementUnlocked(achievementId);
            }
        } catch (Exception e) {
            Log.e(TAG, "Failed to unlock achievement: " + achievementId, e);
            sendError(100, "Failed to unlock: " + e.getMessage(), achievementId);
        }
    }

    /**
     * Increments an incremental achievement.
     * PGS v2: Fire-and-forget — no Task returned.
     * @param achievementId Achievement ID from games-ids.xml
     * @param steps Number of steps to increment (must be > 0)
     */
    public void incrementAchievement(final String achievementId, int steps) {
        if (achievementId == null || achievementId.isEmpty()) {
            sendError(-1, "Achievement ID cannot be null or empty", null);
            return;
        }

        if (steps <= 0) {
            sendError(4, "Steps must be greater than 0", achievementId);
            return;
        }

        Log.d(TAG, "Incrementing achievement: " + achievementId + " by " + steps);

        try {
            achievementsClient.increment(achievementId, steps);
            Log.d(TAG, "Achievement increment sent: " + achievementId);

            // Load achievement to get current/total steps
            loadAchievementSteps(achievementId);
        } catch (Exception e) {
            Log.e(TAG, "Failed to increment achievement: " + achievementId, e);
            sendError(100, "Failed to increment: " + e.getMessage(), achievementId);
        }
    }

    /**
     * Reveals a hidden achievement (makes it visible to player).
     * PGS v2: Fire-and-forget — no Task returned.
     * @param achievementId Achievement ID from games-ids.xml
     */
    public void revealAchievement(final String achievementId) {
        if (achievementId == null || achievementId.isEmpty()) {
            sendError(-1, "Achievement ID cannot be null or empty", null);
            return;
        }

        Log.d(TAG, "Revealing achievement: " + achievementId);

        try {
            achievementsClient.reveal(achievementId);
            Log.d(TAG, "Achievement reveal sent: " + achievementId);
            if (callback != null) {
                callback.onAchievementRevealed(achievementId);
            }
        } catch (Exception e) {
            Log.e(TAG, "Failed to reveal achievement: " + achievementId, e);
            sendError(100, "Failed to reveal: " + e.getMessage(), achievementId);
        }
    }

    /**
     * Shows the native Google Play Games achievements UI.
     */
    public void showAchievementsUI() {
        Log.d(TAG, "Showing achievements UI");

        achievementsClient.getAchievementsIntent()
                .addOnSuccessListener(activity, intent -> {
                    try {
                        activity.startActivityForResult(intent, 9001);
                        Log.d(TAG, "Achievements UI opened");
                        // NOTE: Callback fires when UI launches, not when closed.
                        // Proper close detection requires onActivityResult handling.
                        if (callback != null) {
                            callback.onAchievementsUIClosed();
                        }
                    } catch (Exception e) {
                        Log.e(TAG, "Failed to start achievements UI", e);
                        sendError(100, "Failed to show UI: " + e.getMessage(), null);
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to get achievements intent", e);
                    sendError(100, "Failed to show UI: " + e.getMessage(), null);
                });
    }

    /**
     * Loads all achievements for the current player.
     * PGS v2: Returns Task<AnnotatedData<AchievementBuffer>>.
     * @param forceReload If true, bypasses cache and fetches from server
     */
    public void loadAchievements(boolean forceReload) {
        Log.d(TAG, "Loading achievements (forceReload: " + forceReload + ")");

        achievementsClient.load(forceReload)
                .addOnSuccessListener(activity, annotatedData -> {
                    AchievementBuffer achievementBuffer = annotatedData.get();
                    try {
                        Log.d(TAG, "Achievements loaded: " + achievementBuffer.getCount());
                        String json = serializeAchievements(achievementBuffer);

                        if (callback != null) {
                            callback.onAchievementsLoaded(json);
                        }
                    } catch (Exception e) {
                        Log.e(TAG, "Failed to serialize achievements", e);
                        sendError(100, "Failed to serialize: " + e.getMessage(), null);
                    } finally {
                        achievementBuffer.release();
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to load achievements", e);
                    sendError(100, "Failed to load: " + e.getMessage(), null);
                });
    }

    /**
     * Unlocks multiple achievements in a batch.
     * PGS v2: All fire-and-forget.
     * @param achievementIds JSON array of achievement IDs
     */
    public void unlockMultiple(String achievementIds) {
        try {
            JSONArray idsArray = new JSONArray(achievementIds);
            List<String> ids = new ArrayList<>();

            for (int i = 0; i < idsArray.length(); i++) {
                ids.add(idsArray.getString(i));
            }

            Log.d(TAG, "Unlocking multiple achievements: " + ids.size());

            for (String id : ids) {
                achievementsClient.unlock(id);
            }

            // Fire callback after all unlocks are dispatched (fire-and-forget)
            if (callback != null) {
                for (String id : ids) {
                    callback.onAchievementUnlocked(id);
                }
            }
        } catch (Exception e) {
            Log.e(TAG, "Failed to parse achievement IDs", e);
            sendError(100, "Invalid achievement IDs: " + e.getMessage(), null);
        }
    }

    /**
     * Helper: Loads steps for an incremental achievement after increment.
     * PGS v2: load() returns Task<AnnotatedData<AchievementBuffer>>.
     */
    private void loadAchievementSteps(final String achievementId) {
        achievementsClient.load(false)
                .addOnSuccessListener(activity, annotatedData -> {
                    AchievementBuffer achievementBuffer = annotatedData.get();
                    try {
                        for (int i = 0; i < achievementBuffer.getCount(); i++) {
                            Achievement achievement = achievementBuffer.get(i);
                            if (achievement.getAchievementId().equals(achievementId)) {
                                int currentSteps = achievement.getCurrentSteps();
                                int totalSteps = achievement.getTotalSteps();

                                if (callback != null) {
                                    callback.onAchievementIncremented(achievementId, currentSteps, totalSteps);
                                }
                                break;
                            }
                        }
                    } catch (Exception e) {
                        Log.e(TAG, "Failed to load achievement steps", e);
                    } finally {
                        achievementBuffer.release();
                    }
                });
    }

    /**
     * Serializes AchievementBuffer to JSON for C# consumption.
     */
    private String serializeAchievements(AchievementBuffer buffer) throws Exception {
        JSONArray array = new JSONArray();

        for (int i = 0; i < buffer.getCount(); i++) {
            Achievement achievement = buffer.get(i);
            JSONObject obj = new JSONObject();

            obj.put("achievementId", achievement.getAchievementId());
            obj.put("name", achievement.getName());
            obj.put("description", achievement.getDescription());
            obj.put("state", achievement.getState());
            obj.put("type", achievement.getType());

            // getCurrentSteps()/getTotalSteps() throw on non-incremental achievements
            if (achievement.getType() == Achievement.TYPE_INCREMENTAL) {
                obj.put("currentSteps", achievement.getCurrentSteps());
                obj.put("totalSteps", achievement.getTotalSteps());
            } else {
                obj.put("currentSteps", 0);
                obj.put("totalSteps", 0);
            }

            obj.put("xpValue", achievement.getXpValue());
            obj.put("unlockedTimestamp", achievement.getLastUpdatedTimestamp());
            obj.put("revealedIconUrl", achievement.getRevealedImageUri() != null ? achievement.getRevealedImageUri().toString() : "");
            obj.put("unlockedIconUrl", achievement.getUnlockedImageUri() != null ? achievement.getUnlockedImageUri().toString() : "");

            array.put(obj);
        }

        return array.toString();
    }

    /**
     * Sends error to C# callback.
     */
    private void sendError(int errorCode, String errorMessage, String achievementId) {
        if (callback != null) {
            callback.onAchievementError(errorCode, errorMessage, achievementId);
        }
    }
}
