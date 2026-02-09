# Consumer ProGuard/R8 rules for com.bizsim.gplay.games
# These rules are automatically applied to the consuming app's minification pass.

# BizSim Bridge Classes (JNI bridge invoked via AndroidJavaClass from Unity C#)
-keep class com.bizsim.gplay.games.** { *; }

# Callback Interfaces (CRITICAL for AndroidJavaProxy — method name matching)
-keep interface com.bizsim.gplay.games.callbacks.** { *; }

-keepclassmembers interface com.bizsim.gplay.games.callbacks.IAuthCallback {
    void onAuthSuccess(java.lang.String, java.lang.String, java.lang.String);
    void onAuthFailure(int, java.lang.String);
    void onServerSideAccessSuccess(java.lang.String);
    void onServerSideAccessFailure(int, java.lang.String);
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

# Google Play Games SDK v2 (accessed via JNI from bridge classes)
-keep class com.google.android.gms.games.** { *; }
-keep interface com.google.android.gms.games.** { *; }

# Task classes (async operations)
-keep class com.google.android.gms.tasks.Task { *; }
-keep class com.google.android.gms.tasks.OnSuccessListener { *; }
-keep class com.google.android.gms.tasks.OnFailureListener { *; }

-dontwarn com.bizsim.gplay.games.**
-dontwarn com.google.android.gms.games.**
