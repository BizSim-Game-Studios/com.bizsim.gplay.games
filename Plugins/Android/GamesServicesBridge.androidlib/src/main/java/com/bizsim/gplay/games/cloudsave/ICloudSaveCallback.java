// Copyright (c) BizSim Game Studios. All rights reserved.

package com.bizsim.gplay.games.cloudsave;

public interface ICloudSaveCallback {
    void onSnapshotOpened(String filename, String snapshotJson, boolean hasConflict);
    void onSnapshotRead(String filename, byte[] data);
    void onSnapshotCommitted(String filename);
    void onSnapshotDeleted(String filename);
    void onSavedGamesUIResult(String selectedFilename);
    void onConflictDetected(String localSnapshotJson, String serverSnapshotJson, byte[] localData, byte[] serverData);
    void onCloudSaveError(int errorCode, String errorMessage, String filename);
}
