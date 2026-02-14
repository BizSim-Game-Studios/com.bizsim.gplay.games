# Copyright (c) BizSim Game Studios. All rights reserved.

# ============================================================
# BizSim Google Play Games Bridge - ProGuard/R8 Keep Rules
# ============================================================

# === Bridge Classes ===
-keep class com.bizsim.gplay.games.** { *; }

# === Callback Interfaces (CRITICAL for AndroidJavaProxy) ===
# AndroidJavaProxy matches methods by NAME - obfuscation breaks this
-keep interface com.bizsim.gplay.games.callbacks.** { *; }

# Keep callback interface methods (must match C# proxy method names exactly)
-keepclassmembers interface com.bizsim.gplay.games.callbacks.IAuthCallback {
    void onAuthSuccess(java.lang.String, java.lang.String, java.lang.String);
    void onAuthFailure(int, java.lang.String);
    void onServerSideAccessSuccess(java.lang.String);
    void onServerSideAccessFailure(int, java.lang.String);
    void onServerSideAccessWithScopesSuccess(java.lang.String, java.lang.String);
    void onServerSideAccessWithScopesFailure(int, java.lang.String);
}

-keepclassmembers interface com.bizsim.gplay.games.achievements.IAchievementCallback {
    void onAchievementUnlocked(java.lang.String);
    void onAchievementIncremented(java.lang.String, int, int);
    void onAchievementRevealed(java.lang.String);
    void onAchievementsLoaded(java.lang.String);
    void onAchievementsUIClosed();
    void onAchievementError(int, java.lang.String, java.lang.String);
}

-keepclassmembers interface com.bizsim.gplay.games.leaderboards.ILeaderboardCallback {
    void onScoreSubmitted(java.lang.String, long);
    void onScoresLoaded(java.lang.String, java.lang.String);
    void onLeaderboardUIClosed();
    void onLeaderboardError(int, java.lang.String, java.lang.String);
}

-keepclassmembers interface com.bizsim.gplay.games.cloudsave.ICloudSaveCallback {
    void onSnapshotOpened(java.lang.String, java.lang.String, boolean);
    void onSnapshotRead(java.lang.String, byte[]);
    void onSnapshotCommitted(java.lang.String);
    void onSnapshotDeleted(java.lang.String);
    void onSavedGamesUIResult(java.lang.String);
    void onConflictDetected(java.lang.String, java.lang.String, byte[], byte[]);
    void onCloudSaveError(int, java.lang.String, java.lang.String);
}

-keepclassmembers interface com.bizsim.gplay.games.stats.IStatsCallback {
    void onStatsLoaded(java.lang.String);
    void onStatsError(int, java.lang.String);
}

-keepclassmembers interface com.bizsim.gplay.games.events.IEventsCallback {
    void onEventsLoaded(java.lang.String);
    void onEventLoaded(java.lang.String);
    void onEventsError(int, java.lang.String);
}

# === Google Play Games SDK v2 ===
-keep class com.google.android.gms.games.** { *; }
-keep interface com.google.android.gms.games.** { *; }

# Keep Player class (returned in callbacks)
-keep class com.google.android.gms.games.Player {
    public java.lang.String getPlayerId();
    public java.lang.String getDisplayName();
    public android.net.Uri getHiResImageUri();
}

# Keep client classes
-keep class com.google.android.gms.games.GamesSignInClient { *; }
-keep class com.google.android.gms.games.PlayersClient { *; }
-keep class com.google.android.gms.games.AchievementsClient { *; }
-keep class com.google.android.gms.games.LeaderboardsClient { *; }
-keep class com.google.android.gms.games.LeaderboardsClient$LeaderboardScores { *; }
-keep class com.google.android.gms.games.SnapshotsClient { *; }
-keep class com.google.android.gms.games.SnapshotsClient$DataOrConflict { *; }
-keep class com.google.android.gms.games.SnapshotsClient$SnapshotConflict { *; }
-keep class com.google.android.gms.games.PlayerStatsClient { *; }
-keep class com.google.android.gms.games.EventsClient { *; }

# Keep common wrapper classes
-keep class com.google.android.gms.games.AnnotatedData { *; }
-keep class com.google.android.gms.games.AuthenticationResult { *; }
-keep class com.google.android.gms.games.gamessignin.AuthResponse { *; }
-keep class com.google.android.gms.games.gamessignin.AuthScope { *; }
-keep class com.google.android.gms.games.PlayGames { *; }
-keep class com.google.android.gms.games.PlayGamesSdk { *; }

# Keep Achievement classes
-keep class com.google.android.gms.games.achievement.Achievement {
    public java.lang.String getAchievementId();
    public java.lang.String getName();
    public java.lang.String getDescription();
    public int getState();
    public int getType();
    public int getCurrentSteps();
    public int getTotalSteps();
    public int getXpValue();
    public long getLastUpdatedTimestamp();
    public android.net.Uri getRevealedImageUri();
    public android.net.Uri getUnlockedImageUri();
}
-keep class com.google.android.gms.games.achievement.AchievementBuffer { *; }

# Keep Leaderboard classes
-keep class com.google.android.gms.games.leaderboard.LeaderboardScore {
    public long getRank();
    public java.lang.String getDisplayRank();
    public long getRawScore();
    public java.lang.String getDisplayScore();
    public java.lang.String getScoreTag();
    public long getTimestampMillis();
    public com.google.android.gms.games.Player getScoreHolder();
}
-keep class com.google.android.gms.games.leaderboard.LeaderboardScoreBuffer { *; }

# Keep Snapshot classes
-keep class com.google.android.gms.games.snapshot.Snapshot { *; }
-keep class com.google.android.gms.games.snapshot.SnapshotMetadata { *; }
-keep class com.google.android.gms.games.snapshot.SnapshotContents { *; }

# Keep PlayerStats (in stats subpackage)
-keep class com.google.android.gms.games.stats.PlayerStats { *; }
-keep class com.google.android.gms.games.stats.PlayerStatsBuffer { *; }

# Keep Event classes
-keep class com.google.android.gms.games.event.Event { *; }
-keep class com.google.android.gms.games.event.EventBuffer { *; }

# Keep Task classes (async operations)
-keep class com.google.android.gms.tasks.Task { *; }
-keep class com.google.android.gms.tasks.OnSuccessListener { *; }
-keep class com.google.android.gms.tasks.OnFailureListener { *; }

# === Strip Debug Logs in Release Builds ===
# R8 removes these calls (and their string arguments) when minifyEnabled=true.
# Only verbose and debug are stripped — info/warn/error preserved for diagnostics.
-assumenosideeffects class android.util.Log {
    public static int v(...);
    public static int d(...);
}

# === Don't Warn ===
-dontwarn com.bizsim.gplay.games.**
-dontwarn com.google.android.gms.games.**
