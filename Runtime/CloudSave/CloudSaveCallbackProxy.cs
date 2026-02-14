// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class CloudSaveCallbackProxy : AndroidJavaProxy
    {
        private readonly GamesCloudSaveController _controller;

        public CloudSaveCallbackProxy(GamesCloudSaveController controller)
            : base(JniConstants.CloudSaveCallback)
        {
            _controller = controller;
        }

        void onSnapshotOpened(string filename, string snapshotJson, bool hasConflict)
        {
            BizSimGamesLogger.Info($"Snapshot opened: {filename} (conflict: {hasConflict})");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSnapshotOpenedFromJava(filename, snapshotJson, hasConflict));
        }

        void onSnapshotRead(string filename, byte[] data)
        {
            BizSimGamesLogger.Info($"Snapshot read: {filename} ({data?.Length ?? 0} bytes)");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSnapshotReadFromJava(filename, data));
        }

        void onSnapshotCommitted(string filename)
        {
            BizSimGamesLogger.Info($"Snapshot committed: {filename}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSnapshotCommittedFromJava(filename));
        }

        void onSnapshotDeleted(string filename)
        {
            BizSimGamesLogger.Info($"Snapshot deleted: {filename}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSnapshotDeletedFromJava(filename));
        }

        void onSavedGamesUIResult(string selectedFilename)
        {
            BizSimGamesLogger.Info($"Saved games UI result: {selectedFilename}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSavedGamesUIResultFromJava(selectedFilename));
        }

        void onConflictDetected(string localSnapshotJson, string serverSnapshotJson, byte[] localData, byte[] serverData)
        {
            BizSimGamesLogger.Warning("Conflict detected");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnConflictDetectedFromJava(localSnapshotJson, serverSnapshotJson, localData, serverData));
        }

        void onCloudSaveError(int errorCode, string errorMessage, string filename)
        {
            BizSimGamesLogger.Error($"Cloud save error: {errorCode} - {errorMessage}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnCloudSaveErrorFromJava(errorCode, errorMessage, filename));
        }
    }
}
