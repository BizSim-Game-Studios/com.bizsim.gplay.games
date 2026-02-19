// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.achievements;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;

import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;

import com.google.android.gms.games.AnnotatedData;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.AchievementsClient;
import com.google.android.gms.games.achievement.Achievement;
import com.google.android.gms.games.achievement.AchievementBuffer;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

public class AchievementBridge {
    private static final String TAG = "BizSimGames.Achievements";

    private final Activity activity;
    private final AchievementsClient achievementsClient;
    private final ActivityResultLauncher<Intent> achievementsLauncher;
    private IAchievementCallback callback;

    public AchievementBridge(Activity activity) {
        this.activity = activity;
        this.achievementsClient = PlayGames.getAchievementsClient(activity);

        this.achievementsLauncher = ((ComponentActivity) activity)
                .getActivityResultRegistry()
                .register(
                        "bizsim_achievements",
                        new ActivityResultContracts.StartActivityForResult(),
                        result -> {
                            if (callback != null) callback.onAchievementsUIClosed();
                        }
                );

        Log.d(TAG, "AchievementBridge initialized with ActivityResultLauncher");
    }

    public void setCallback(IAchievementCallback callback) {
        this.callback = callback;
        Log.d(TAG, "Callback registered");
    }

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
            loadAchievementSteps(achievementId);
        } catch (Exception e) {
            Log.e(TAG, "Failed to increment achievement: " + achievementId, e);
            sendError(100, "Failed to increment: " + e.getMessage(), achievementId);
        }
    }

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

    public void showAchievementsUI() {
        Log.d(TAG, "Showing achievements UI");

        achievementsClient.getAchievementsIntent()
                .addOnSuccessListener(activity, achievementsLauncher::launch)
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to get achievements intent", e);
                    sendError(100, "Failed to show UI: " + e.getMessage(), null);
                });
    }

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
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to load achievement steps for: " + achievementId, e);
                    sendError(100, "Failed to load steps: " + e.getMessage(), achievementId);
                });
    }

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

    public void shutdown() {
        achievementsLauncher.unregister();
        callback = null;
    }

    private void sendError(int errorCode, String errorMessage, String achievementId) {
        if (callback != null) {
            callback.onAchievementError(errorCode, errorMessage, achievementId);
        }
    }
}
