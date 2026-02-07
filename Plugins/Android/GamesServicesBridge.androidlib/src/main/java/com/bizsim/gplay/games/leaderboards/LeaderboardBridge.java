// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.leaderboards;

import android.app.Activity;
import android.util.Log;

import com.google.android.gms.games.AnnotatedData;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.LeaderboardsClient;
import com.google.android.gms.games.leaderboard.LeaderboardScore;
import com.google.android.gms.games.leaderboard.LeaderboardScoreBuffer;
import com.google.android.gms.games.leaderboard.LeaderboardVariant;

import org.json.JSONArray;
import org.json.JSONObject;

public class LeaderboardBridge {
    private static final String TAG = "BizSimGames.Leaderboards";

    private final Activity activity;
    private final LeaderboardsClient leaderboardsClient;
    private ILeaderboardCallback callback;

    public LeaderboardBridge(Activity activity) {
        this.activity = activity;
        this.leaderboardsClient = PlayGames.getLeaderboardsClient(activity);
        Log.d(TAG, "LeaderboardBridge initialized");
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
                .addOnSuccessListener(activity, intent -> {
                    activity.startActivityForResult(intent, 9002);
                    // NOTE: Fires on UI launch, not close. Proper close detection requires onActivityResult.
                    if (callback != null) {
                        callback.onLeaderboardUIClosed();
                    }
                })
                .addOnFailureListener(activity, e -> sendError(100, e.getMessage(), leaderboardId));
    }

    public void showAllLeaderboardsUI() {
        leaderboardsClient.getAllLeaderboardsIntent()
                .addOnSuccessListener(activity, intent -> {
                    activity.startActivityForResult(intent, 9003);
                    // NOTE: Fires on UI launch, not close. Proper close detection requires onActivityResult.
                    if (callback != null) {
                        callback.onLeaderboardUIClosed();
                    }
                })
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

    private String serializeScores(com.google.android.gms.games.leaderboard.LeaderboardScoreBuffer buffer) throws Exception {
        JSONArray array = new JSONArray();

        for (int i = 0; i < buffer.getCount(); i++) {
            LeaderboardScore score = buffer.get(i);
            JSONObject obj = new JSONObject();
            obj.put("playerId", score.getScoreHolder().getPlayerId());
            obj.put("displayName", score.getScoreHolder().getDisplayName());
            obj.put("score", score.getRawScore());
            obj.put("formattedScore", score.getDisplayScore());
            obj.put("rank", score.getRank());
            obj.put("scoreTag", score.getScoreTag() != null ? score.getScoreTag() : "");
            obj.put("timestampMillis", score.getTimestampMillis());
            obj.put("avatarUrl", score.getScoreHolder().getHiResImageUri() != null ?
                score.getScoreHolder().getHiResImageUri().toString() : "");
            array.put(obj);
        }

        return array.toString();
    }

    private void sendError(int errorCode, String errorMessage, String leaderboardId) {
        if (callback != null) {
            callback.onLeaderboardError(errorCode, errorMessage, leaderboardId);
        }
    }
}
