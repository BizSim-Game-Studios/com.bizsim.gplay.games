// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.cloudsave;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.util.Log;

import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.SnapshotsClient;
import com.google.android.gms.games.snapshot.Snapshot;
import com.google.android.gms.games.snapshot.SnapshotMetadata;
import com.google.android.gms.games.snapshot.SnapshotMetadataChange;

import org.json.JSONObject;

import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;


/**
 * JNI bridge for Google Play Games Cloud Save (PGS v2).
 *
 * PGS v2 notes:
 * - Conflict class is SnapshotsClient.SnapshotConflict (not snapshot.SnapshotConflict)
 * - open() returns Task<DataOrConflict<Snapshot>>
 */
public class CloudSaveBridge {
    private static final String TAG = "BizSimGames.CloudSave";
    private static final int CONFLICT_RESOLUTION_POLICY_MANUAL = -1;
    private static final int RC_SAVED_GAMES = 9004;

    private static ICloudSaveCallback pendingUICallback;

    private final Activity activity;
    private final SnapshotsClient snapshotsClient;
    private final ExecutorService ioExecutor = Executors.newSingleThreadExecutor();
    private ICloudSaveCallback callback;

    public CloudSaveBridge(Activity activity) {
        this.activity = activity;
        this.snapshotsClient = PlayGames.getSnapshotsClient(activity);
        Log.d(TAG, "CloudSaveBridge initialized");
    }

    public void setCallback(ICloudSaveCallback callback) {
        this.callback = callback;
    }

    public void openSnapshot(String filename, boolean createIfNotFound) {
        Log.d(TAG, "Opening snapshot: " + filename);

        snapshotsClient.open(filename, createIfNotFound, CONFLICT_RESOLUTION_POLICY_MANUAL)
                .addOnSuccessListener(activity, dataOrConflict -> {
                    if (dataOrConflict.isConflict()) {
                        Log.w(TAG, "Conflict detected for: " + filename);
                        handleConflict(dataOrConflict.getConflict());
                    } else {
                        Snapshot snapshot = dataOrConflict.getData();
                        try {
                            String snapshotJson = serializeSnapshot(snapshot);
                            if (callback != null) {
                                callback.onSnapshotOpened(filename, snapshotJson, false);
                            }
                        } catch (Exception e) {
                            sendError(100, "Failed to serialize snapshot: " + e.getMessage(), filename);
                        }
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to open snapshot: " + filename, e);
                    sendError(100, "Open failed: " + e.getMessage(), filename);
                });
    }

    public void readSnapshot(String nativeHandle) {
        Log.d(TAG, "Read snapshot: " + nativeHandle);

        String[] parts = nativeHandle.split(":");
        if (parts.length < 2) {
            sendError(100, "Invalid snapshot handle", null);
            return;
        }

        String filename = parts[1];
        snapshotsClient.open(filename, false, CONFLICT_RESOLUTION_POLICY_MANUAL)
                .addOnSuccessListener(activity, dataOrConflict -> {
                    if (dataOrConflict.isConflict()) {
                        handleConflict(dataOrConflict.getConflict());
                    } else {
                        Snapshot snapshot = dataOrConflict.getData();
                        ioExecutor.execute(() -> {
                            try {
                                byte[] data = snapshot.getSnapshotContents().readFully();
                                postToMainThread(() -> {
                                    if (callback != null) {
                                        callback.onSnapshotRead(filename, data);
                                    }
                                });
                            } catch (Exception e) {
                                postToMainThread(() ->
                                    sendError(100, "Read failed: " + e.getMessage(), filename));
                            }
                        });
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to open snapshot for read: " + filename, e);
                    sendError(100, "Read open failed: " + e.getMessage(), filename);
                });
    }

    public void commitSnapshot(String nativeHandle, byte[] data, String description, long playedTimeMillis, byte[] coverImage) {
        Log.d(TAG, "Commit snapshot: " + nativeHandle + " (" + data.length + " bytes)");

        String[] parts = nativeHandle.split(":");
        if (parts.length < 2) {
            sendError(100, "Invalid snapshot handle", null);
            return;
        }

        String filename = parts[1];
        snapshotsClient.open(filename, true, CONFLICT_RESOLUTION_POLICY_MANUAL)
                .addOnSuccessListener(activity, dataOrConflict -> {
                    if (dataOrConflict.isConflict()) {
                        handleConflict(dataOrConflict.getConflict());
                    } else {
                        Snapshot snapshot = dataOrConflict.getData();
                        ioExecutor.execute(() -> {
                            try {
                                snapshot.getSnapshotContents().writeBytes(data);

                                SnapshotMetadataChange.Builder metaBuilder = new SnapshotMetadataChange.Builder()
                                        .setPlayedTimeMillis(playedTimeMillis);

                                if (description != null && !description.isEmpty()) {
                                    metaBuilder.setDescription(description);
                                }

                                if (coverImage != null && coverImage.length > 0) {
                                    try {
                                        Bitmap bitmap = decodeCoverImageSafe(coverImage);
                                        if (bitmap != null) {
                                            metaBuilder.setCoverImage(bitmap);
                                        }
                                    } catch (OutOfMemoryError e) {
                                        Log.e(TAG,
                                            "Cover image decode OOM (" + coverImage.length + " bytes). " +
                                            "Use max 640x360 resolution. Save continues without cover image.", e);
                                    }
                                }

                                SnapshotMetadataChange metaChange = metaBuilder.build();

                                postToMainThread(() ->
                                    snapshotsClient.commitAndClose(snapshot, metaChange)
                                            .addOnSuccessListener(activity, metadata -> {
                                                Log.d(TAG, "Snapshot committed: " + filename);
                                                if (callback != null) {
                                                    callback.onSnapshotCommitted(filename);
                                                }
                                            })
                                            .addOnFailureListener(activity, e -> {
                                                sendError(100, "Commit failed: " + e.getMessage(), filename);
                                            }));

                            } catch (Exception e) {
                                postToMainThread(() ->
                                    sendError(100, "Write failed: " + e.getMessage(), filename));
                            }
                        });
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to open snapshot for commit: " + filename, e);
                    sendError(100, "Commit open failed: " + e.getMessage(), filename);
                });
    }

    public void deleteSnapshot(String filename) {
        Log.d(TAG, "Delete snapshot: " + filename);

        snapshotsClient.open(filename, false, CONFLICT_RESOLUTION_POLICY_MANUAL)
                .addOnSuccessListener(activity, dataOrConflict -> {
                    if (dataOrConflict.isConflict()) {
                        // Resolve conflict by picking server version, then retry delete
                        Log.w(TAG, "Conflict on delete open for: " + filename);
                        handleConflict(dataOrConflict.getConflict());
                    } else {
                        SnapshotMetadata metadata = dataOrConflict.getData().getMetadata();
                        snapshotsClient.delete(metadata)
                                .addOnSuccessListener(activity, deleteResult -> {
                                    Log.d(TAG, "Snapshot deleted: " + filename);
                                    if (callback != null) {
                                        callback.onSnapshotDeleted(filename);
                                    }
                                })
                                .addOnFailureListener(activity, e -> {
                                    sendError(100, "Delete failed: " + e.getMessage(), filename);
                                });
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to open snapshot for delete: " + filename, e);
                    sendError(100, "Delete open failed: " + e.getMessage(), filename);
                });
    }

    public void showSavedGamesUI(String title, boolean allowAddButton, boolean allowDelete, int maxSnapshots) {
        Log.d(TAG, "Show saved games UI");

        pendingUICallback = callback;

        snapshotsClient.getSelectSnapshotIntent(title, allowAddButton, allowDelete, maxSnapshots)
                .addOnSuccessListener(activity, intent -> {
                    activity.startActivityForResult(intent, RC_SAVED_GAMES);
                })
                .addOnFailureListener(activity, e -> {
                    pendingUICallback = null;
                    sendError(100, "UI failed: " + e.getMessage(), null);
                });
    }

    public static void handleActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode != RC_SAVED_GAMES || pendingUICallback == null) return;

        try {
            if (resultCode == Activity.RESULT_OK && data != null) {
                if (data.hasExtra(SnapshotsClient.EXTRA_SNAPSHOT_METADATA)) {
                    SnapshotMetadata metadata = data.getParcelableExtra(
                        SnapshotsClient.EXTRA_SNAPSHOT_METADATA);
                    if (metadata != null) {
                        pendingUICallback.onSavedGamesUIResult(metadata.getUniqueName());
                    } else {
                        pendingUICallback.onSavedGamesUIResult(null);
                    }
                } else if (data.hasExtra(SnapshotsClient.EXTRA_SNAPSHOT_NEW)) {
                    pendingUICallback.onSavedGamesUIResult("__NEW__");
                } else {
                    pendingUICallback.onSavedGamesUIResult(null);
                }
            } else {
                pendingUICallback.onSavedGamesUIResult(null);
            }
        } catch (Exception e) {
            Log.e(TAG, "handleActivityResult error", e);
            pendingUICallback.onSavedGamesUIResult(null);
        } finally {
            pendingUICallback = null;
        }
    }

    private volatile SnapshotsClient.SnapshotConflict lastConflict;

    /**
     * Handles snapshot conflict using PGS v2 SnapshotsClient.SnapshotConflict.
     * Heavy I/O (readFully) runs on a background thread to prevent ANR.
     * JNI callback is posted back to main thread for AndroidJavaProxy safety.
     */
    private void handleConflict(SnapshotsClient.SnapshotConflict conflict) {
        this.lastConflict = conflict;

        ioExecutor.execute(() -> {
            try {
                Snapshot conflictSnapshot = conflict.getConflictingSnapshot();
                Snapshot serverSnapshot = conflict.getSnapshot();

                String localJson = serializeSnapshot(conflictSnapshot);
                String serverJson = serializeSnapshot(serverSnapshot);

                byte[] localData = conflictSnapshot.getSnapshotContents().readFully();
                byte[] serverData = serverSnapshot.getSnapshotContents().readFully();

                postToMainThread(() -> {
                    if (callback != null) {
                        callback.onConflictDetected(localJson, serverJson, localData, serverData);
                    }
                });
            } catch (Exception e) {
                Log.e(TAG, "Failed to handle conflict", e);
                postToMainThread(() ->
                    sendError(100, "Conflict handling failed: " + e.getMessage(), null));
            }
        });
    }

    /**
     * Resolves a snapshot conflict.
     * PGS v2: Uses SnapshotsClient.resolveConflict(conflictId, snapshot).
     * @param resolution "Local" to keep local, "Server" to keep server version
     * @param nativeHandle snapshot native handle (unused, kept for C# compatibility)
     */
    public void resolveConflict(String resolution, String nativeHandle) {
        Log.d(TAG, "Resolve conflict: " + resolution);

        if (lastConflict == null) {
            sendError(100, "No conflict to resolve", null);
            return;
        }

        String conflictId = lastConflict.getConflictId();
        Snapshot resolvedSnapshot;

        if ("UseLocal".equalsIgnoreCase(resolution) || "Local".equalsIgnoreCase(resolution)) {
            resolvedSnapshot = lastConflict.getConflictingSnapshot();
        } else {
            resolvedSnapshot = lastConflict.getSnapshot();
        }

        snapshotsClient.resolveConflict(conflictId, resolvedSnapshot)
                .addOnSuccessListener(activity, dataOrConflict -> {
                    lastConflict = null;
                    if (dataOrConflict.isConflict()) {
                        Log.w(TAG, "Recursive conflict detected after resolution");
                        handleConflict(dataOrConflict.getConflict());
                    } else {
                        Snapshot snapshot = dataOrConflict.getData();
                        try {
                            String filename = snapshot.getMetadata().getUniqueName();
                            String snapshotJson = serializeSnapshot(snapshot);
                            if (callback != null) {
                                callback.onSnapshotOpened(filename, snapshotJson, false);
                            }
                        } catch (Exception e) {
                            sendError(100, "Post-resolve serialize failed: " + e.getMessage(), null);
                        }
                    }
                })
                .addOnFailureListener(activity, e -> {
                    Log.e(TAG, "Failed to resolve conflict", e);
                    lastConflict = null;
                    sendError(100, "Resolve failed: " + e.getMessage(), null);
                });
    }

    private static final int MAX_COVER_WIDTH = 640;
    private static final int MAX_COVER_HEIGHT = 360;

    private Bitmap decodeCoverImageSafe(byte[] coverImage) {
        BitmapFactory.Options boundsOptions = new BitmapFactory.Options();
        boundsOptions.inJustDecodeBounds = true;
        BitmapFactory.decodeByteArray(coverImage, 0, coverImage.length, boundsOptions);

        int width = boundsOptions.outWidth;
        int height = boundsOptions.outHeight;

        if (width <= 0 || height <= 0) {
            Log.e(TAG, "Cover image has invalid dimensions (" + width + "x" + height + ")");
            return null;
        }

        int inSampleSize = 1;
        if (width > MAX_COVER_WIDTH || height > MAX_COVER_HEIGHT) {
            int halfWidth = width / 2;
            int halfHeight = height / 2;
            while ((halfWidth / inSampleSize) >= MAX_COVER_WIDTH
                    && (halfHeight / inSampleSize) >= MAX_COVER_HEIGHT) {
                inSampleSize *= 2;
            }
            Log.w(TAG, "Cover image " + width + "x" + height +
                    " exceeds " + MAX_COVER_WIDTH + "x" + MAX_COVER_HEIGHT +
                    ", downsampling with inSampleSize=" + inSampleSize);
        }

        BitmapFactory.Options decodeOptions = new BitmapFactory.Options();
        decodeOptions.inSampleSize = inSampleSize;
        return BitmapFactory.decodeByteArray(coverImage, 0, coverImage.length, decodeOptions);
    }

    private String serializeSnapshot(Snapshot snapshot) throws Exception {
        SnapshotMetadata metadata = snapshot.getMetadata();
        JSONObject obj = new JSONObject();

        obj.put("filename", metadata.getUniqueName());
        obj.put("nativeHandle", "snapshot:" + metadata.getUniqueName());
        obj.put("lastModifiedTimestamp", metadata.getLastModifiedTimestamp());
        obj.put("playedTimeMillis", metadata.getPlayedTime());
        obj.put("description", metadata.getDescription());

        android.net.Uri coverUri = metadata.getCoverImageUri();
        if (coverUri != null) {
            obj.put("coverImageUri", coverUri.toString());
        }

        return obj.toString();
    }

    private void postToMainThread(Runnable r) {
        if (activity != null && !activity.isFinishing() && !activity.isDestroyed()) {
            activity.runOnUiThread(r);
        } else {
            Log.w(TAG, "Activity not available, dropping callback");
        }
    }

    private void sendError(int errorCode, String errorMessage, String filename) {
        if (callback != null) {
            callback.onCloudSaveError(errorCode, errorMessage, filename);
        }
    }

    public void shutdown() {
        ioExecutor.shutdownNow();
        lastConflict = null;
        callback = null;
    }
}
