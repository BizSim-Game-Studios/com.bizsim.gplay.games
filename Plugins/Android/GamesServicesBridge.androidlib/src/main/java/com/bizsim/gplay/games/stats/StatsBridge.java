// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.stats;

import android.app.Activity;
import android.util.Log;

import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.PlayerStatsClient;
import com.google.android.gms.games.stats.PlayerStats;

import org.json.JSONObject;

/**
 * JNI bridge for Google Play Games Player Stats (PGS v2).
 *
 * PGS v2 notes:
 * - Use PlayGames.getPlayerStatsClient() instead of PlayersClient
 * - PlayerStats is in com.google.android.gms.games.stats package
 * - loadPlayerStats() returns Task<AnnotatedData<PlayerStats>>
 * - churnProbability/highSpenderProbability are deprecated (return UNSET_VALUE)
 */
public class StatsBridge {
    private static final String TAG = "BizSimGames.Stats";

    private final Activity activity;
    private final PlayerStatsClient playerStatsClient;
    private IStatsCallback callback;

    public StatsBridge(Activity activity) {
        this.activity = activity;
        this.playerStatsClient = PlayGames.getPlayerStatsClient(activity);
        Log.d(TAG, "StatsBridge initialized");
    }

    public void setCallback(IStatsCallback callback) {
        this.callback = callback;
    }

    public void loadPlayerStats(boolean forceReload) {
        Log.d(TAG, "Loading player stats (forceReload: " + forceReload + ")");

        playerStatsClient.loadPlayerStats(forceReload)
                .addOnSuccessListener(activity, annotatedData -> {
                    try {
                        PlayerStats stats = annotatedData.get();
                        if (stats == null) {
                            sendError(100, "Stats data is null");
                            return;
                        }

                        String json = serializeStats(stats);
                        if (callback != null) {
                            callback.onStatsLoaded(json);
                        }
                    } catch (Exception e) {
                        Log.e(TAG, "Failed to load stats", e);
                        sendError(100, e.getMessage());
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to load stats", e);
                    sendError(100, e.getMessage());
                });
    }

    private String serializeStats(PlayerStats stats) throws Exception {
        JSONObject obj = new JSONObject();

        obj.put("avgSessionLengthMinutes", stats.getAverageSessionLength());
        obj.put("daysSinceLastPlayed", stats.getDaysSinceLastPlayed());
        obj.put("numberOfPurchases", stats.getNumberOfPurchases());
        obj.put("numberOfSessions", stats.getNumberOfSessions());
        obj.put("sessionPercentile", stats.getSessionPercentile());
        obj.put("spendPercentile", stats.getSpendPercentile());
        // Deprecated in v2 (always return UNSET_VALUE) but included for compatibility
        obj.put("churnProbability", stats.getChurnProbability());
        obj.put("highSpenderProbability", stats.getHighSpenderProbability());

        return obj.toString();
    }

    private void sendError(int errorCode, String errorMessage) {
        if (callback != null) {
            callback.onStatsError(errorCode, errorMessage);
        }
    }
}
