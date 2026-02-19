// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.leaderboards;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;

import androidx.activity.ComponentActivity;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;

import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.LeaderboardsClient;
import com.google.android.gms.games.leaderboard.LeaderboardScore;
import com.google.android.gms.games.leaderboard.LeaderboardScoreBuffer;
import com.google.android.gms.games.Player;

import org.json.JSONArray;
import org.json.JSONObject;

public class LeaderboardBridge {
    private static final String TAG = "BizSimGames.Leaderboards";

    private final Activity activity;
    private final LeaderboardsClient leaderboardsClient;
    private final ActivityResultLauncher<Intent> leaderboardLauncher;
    private final ActivityResultLauncher<Intent> allLeaderboardsLauncher;
    private ILeaderboardCallback callback;

    public LeaderboardBridge(Activity activity) {
        this.activity = activity;
        this.leaderboardsClient = PlayGames.getLeaderboardsClient(activity);

        this.leaderboardLauncher = ((ComponentActivity) activity)
                .getActivityResultRegistry()
                .register(
                        "bizsim_leaderboard",
                        new ActivityResultContracts.StartActivityForResult(),
                        result -> {
                            if (callback != null) callback.onLeaderboardUIClosed();
                        }
                );

        this.allLeaderboardsLauncher = ((ComponentActivity) activity)
                .getActivityResultRegistry()
                .register(
                        "bizsim_all_leaderboards",
                        new ActivityResultContracts.StartActivityForResult(),
                        result -> {
                            if (callback != null) callback.onLeaderboardUIClosed();
                        }
                );

        Log.d(TAG, "LeaderboardBridge initialized with ActivityResultLauncher");
    }

    public void setCallback(ILeaderboardCallback callback) {
        this.callback = callback;
    }

    public void submitScore(String leaderboardId, long score, String scoreTag) {
        Log.d(TAG, "Submitting score: " + score + " to " + leaderboardId);

        if (scoreTag != null && !scoreTag.isEmpty()) {
            leaderboardsClient.submitScore(leaderboardId, score, scoreTag);
        } else {
            leaderboardsClient.submitScore(leaderboardId, score);
        }

        if (callback != null) {
            callback.onScoreSubmitted(leaderboardId, score);
        }
    }

    public void showLeaderboardUI(String leaderboardId) {
        leaderboardsClient.getLeaderboardIntent(leaderboardId)
                .addOnSuccessListener(activity, leaderboardLauncher::launch)
                .addOnFailureListener(activity, e -> sendError(100, e.getMessage(), leaderboardId));
    }

    public void showAllLeaderboardsUI() {
        leaderboardsClient.getAllLeaderboardsIntent()
                .addOnSuccessListener(activity, allLeaderboardsLauncher::launch)
                .addOnFailureListener(activity, e -> sendError(100, e.getMessage(), null));
    }

    public void loadTopScores(String leaderboardId, int timeSpan, int collection, int maxResults) {
        leaderboardsClient.loadTopScores(leaderboardId, timeSpan, collection, maxResults)
                .addOnSuccessListener(activity, annotatedData -> {
                    LeaderboardsClient.LeaderboardScores leaderboardScores = annotatedData.get();
                    try {
                        String json = serializeScores(leaderboardScores.getScores());
                        if (callback != null) {
                            callback.onScoresLoaded(leaderboardId, json);
                        }
                    } catch (Exception e) {
                        sendError(100, e.getMessage(), leaderboardId);
                    } finally {
                        leaderboardScores.release();
                    }
                })
                .addOnFailureListener(activity, e -> sendError(100, e.getMessage(), leaderboardId));
    }

    public void loadPlayerCenteredScores(String leaderboardId, int timeSpan, int collection, int maxResults) {
        leaderboardsClient.loadPlayerCenteredScores(leaderboardId, timeSpan, collection, maxResults)
                .addOnSuccessListener(activity, annotatedData -> {
                    LeaderboardsClient.LeaderboardScores leaderboardScores = annotatedData.get();
                    try {
                        String json = serializeScores(leaderboardScores.getScores());
                        if (callback != null) {
                            callback.onScoresLoaded(leaderboardId, json);
                        }
                    } catch (Exception e) {
                        sendError(100, e.getMessage(), leaderboardId);
                    } finally {
                        leaderboardScores.release();
                    }
                })
                .addOnFailureListener(activity, e -> sendError(100, e.getMessage(), leaderboardId));
    }

    private String serializeScores(LeaderboardScoreBuffer buffer) throws Exception {
        JSONArray array = new JSONArray();

        for (int i = 0; i < buffer.getCount(); i++) {
            LeaderboardScore score = buffer.get(i);
            JSONObject obj = new JSONObject();
            Player holder = score.getScoreHolder();
            obj.put("playerId", holder != null ? holder.getPlayerId() : "");
            obj.put("displayName", holder != null ? holder.getDisplayName() : "");
            obj.put("score", score.getRawScore());
            obj.put("formattedScore", score.getDisplayScore());
            obj.put("rank", score.getRank());
            obj.put("scoreTag", score.getScoreTag() != null ? score.getScoreTag() : "");
            obj.put("timestampMillis", score.getTimestampMillis());
            obj.put("avatarUrl", (holder != null && holder.getHiResImageUri() != null) ?
                holder.getHiResImageUri().toString() : "");
            array.put(obj);
        }

        return array.toString();
    }

    public void shutdown() {
        leaderboardLauncher.unregister();
        allLeaderboardsLauncher.unregister();
        callback = null;
    }

    private void sendError(int errorCode, String errorMessage, String leaderboardId) {
        if (callback != null) {
            callback.onLeaderboardError(errorCode, errorMessage, leaderboardId);
        }
    }
}
