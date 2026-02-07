// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.leaderboards;

public interface ILeaderboardCallback {
    void onScoreSubmitted(String leaderboardId, long score);
    void onScoresLoaded(String leaderboardId, String scoresJson);
    void onLeaderboardUIClosed();
    void onLeaderboardError(int errorCode, String errorMessage, String leaderboardId);
}
