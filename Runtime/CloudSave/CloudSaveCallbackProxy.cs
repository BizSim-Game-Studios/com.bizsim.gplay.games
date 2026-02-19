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
            BizSimGamesLogger.Info($"[CloudSave][JNI→Unity] onSnapshotOpened: filename='{filename}', hasConflict={hasConflict}, json={snapshotJson?.Length ?? 0} chars");
            BizSimGamesLogger.Info($"[CloudSave][JNI→Unity] onSnapshotOpened raw json: {snapshotJson}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSnapshotOpenedFromJava(filename, snapshotJson, hasConflict));
        }

        void onSnapshotRead(string filename, byte[] data)
        {
            BizSimGamesLogger.Info($"[CloudSave][JNI→Unity] onSnapshotRead: filename='{filename}', dataSize={data?.Length ?? 0} bytes");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSnapshotReadFromJava(filename, data));
        }

        void onSnapshotCommitted(string filename)
        {
            BizSimGamesLogger.Info($"[CloudSave][JNI→Unity] onSnapshotCommitted: filename='{filename}'");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSnapshotCommittedFromJava(filename));
        }

        void onSnapshotDeleted(string filename)
        {
            BizSimGamesLogger.Info($"[CloudSave][JNI→Unity] onSnapshotDeleted: filename='{filename}'");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSnapshotDeletedFromJava(filename));
        }

        void onSavedGamesUIResult(string selectedFilename)
        {
            BizSimGamesLogger.Info($"[CloudSave][JNI→Unity] onSavedGamesUIResult: selectedFilename='{selectedFilename ?? "(null)"}'");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnSavedGamesUIResultFromJava(selectedFilename));
        }

        void onConflictDetected(string localSnapshotJson, string serverSnapshotJson, byte[] localData, byte[] serverData)
        {
            BizSimGamesLogger.Warning($"[CloudSave][JNI→Unity] onConflictDetected: localJson={localSnapshotJson?.Length ?? 0} chars, serverJson={serverSnapshotJson?.Length ?? 0} chars, localData={localData?.Length ?? 0} bytes, serverData={serverData?.Length ?? 0} bytes");
            BizSimGamesLogger.Info($"[CloudSave][JNI→Unity] conflict local: {localSnapshotJson}");
            BizSimGamesLogger.Info($"[CloudSave][JNI→Unity] conflict server: {serverSnapshotJson}");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnConflictDetectedFromJava(localSnapshotJson, serverSnapshotJson, localData, serverData));
        }

        void onCloudSaveError(int errorCode, string errorMessage, string filename)
        {
            BizSimGamesLogger.Error($"[CloudSave][JNI→Unity] onCloudSaveError: code={errorCode}, message='{errorMessage}', filename='{filename}'");
            UnityMainThreadDispatcher.Enqueue(() => _controller.OnCloudSaveErrorFromJava(errorCode, errorMessage, filename));
        }
    }
}
