// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class GamesCloudSaveController : IGamesCloudSaveProvider
    {
        private const int CONFLICT_TIMEOUT_SECONDS = 60;

        private AndroidJavaObject _cloudSaveBridge;
        private CloudSaveCallbackProxy _callbackProxy;

        private TaskCompletionSource<SnapshotHandle> _openTcs;
        private TaskCompletionSource<byte[]> _readTcs;
        private TaskCompletionSource<bool> _commitTcs;
        private TaskCompletionSource<bool> _deleteTcs;
        private TaskCompletionSource<string> _showUITcs;
        private TaskCompletionSource<ConflictResolution> _conflictTcs;

        public event Action<SnapshotHandle> OnSnapshotOpened;
        public event Action<string> OnSnapshotCommitted;
        public event Action<SavedGameConflict> OnConflictDetected;
        public event Action<GamesCloudSaveError> OnCloudSaveError;

        public GamesCloudSaveController()
        {
            InitializeBridge();
        }

        private void InitializeBridge()
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _cloudSaveBridge = new AndroidJavaObject(
                        "com.bizsim.gplay.games.cloudsave.CloudSaveBridge", activity);
                    _callbackProxy = new CloudSaveCallbackProxy(this);
                    _cloudSaveBridge.Call("setCallback", _callbackProxy);
                    BizSimGamesLogger.Info("CloudSaveBridge initialized");
                }
            }
            catch (Exception ex)
            {
                BizSimGamesLogger.Error($"Failed to initialize CloudSaveBridge: {ex.Message}");
                throw;
            }
        }

        public async Task<SnapshotHandle> OpenSnapshotAsync(string filename, bool createIfNotFound = true, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _openTcs = new TaskCompletionSource<SnapshotHandle>();

            using (ct.Register(() => _openTcs.TrySetCanceled()))
            {
                _cloudSaveBridge.Call("openSnapshot", filename, createIfNotFound);
                return await _openTcs.Task;
            }
        }

        public async Task<byte[]> ReadSnapshotAsync(SnapshotHandle handle, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _readTcs = new TaskCompletionSource<byte[]>();

            using (ct.Register(() => _readTcs.TrySetCanceled()))
            {
                _cloudSaveBridge.Call("readSnapshot", handle.nativeHandle);
                return await _readTcs.Task;
            }
        }

        /// <remarks>
        /// coverImage is accepted for API compatibility but currently ignored by the Java bridge.
        /// PGS v2 supports cover images via SnapshotMetadataChange.Builder.setCoverImage().
        /// </remarks>
        public async Task CommitSnapshotAsync(SnapshotHandle handle, byte[] data, string description = null,
            long playedTimeMillis = 0, byte[] coverImage = null, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _commitTcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _commitTcs.TrySetCanceled()))
            {
                _cloudSaveBridge.Call("commitSnapshot", handle.nativeHandle, data, description ?? "", playedTimeMillis);
                await _commitTcs.Task;
            }
        }

        public async Task DeleteSnapshotAsync(string filename, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _deleteTcs = new TaskCompletionSource<bool>();

            using (ct.Register(() => _deleteTcs.TrySetCanceled()))
            {
                _cloudSaveBridge.Call("deleteSnapshot", filename);
                await _deleteTcs.Task;
            }
        }

        public async Task<string> ShowSavedGamesUIAsync(string title = "Saved Games", bool allowAddButton = false,
            bool allowDelete = true, int maxSnapshots = 5, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _showUITcs = new TaskCompletionSource<string>();

            using (ct.Register(() => _showUITcs.TrySetCanceled()))
            {
                _cloudSaveBridge.Call("showSavedGamesUI", title, allowAddButton, allowDelete, maxSnapshots);
                return await _showUITcs.Task;
            }
        }

        public async Task SaveAsync(string filename, byte[] data, string description = null, CancellationToken ct = default)
        {
            var handle = await OpenSnapshotAsync(filename, true, ct);

            if (handle.hasConflict)
            {
                _conflictTcs = new TaskCompletionSource<ConflictResolution>();
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(CONFLICT_TIMEOUT_SECONDS));

                try
                {
                    // Manual timeout implementation (.NET Standard 2.1 compatible)
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(CONFLICT_TIMEOUT_SECONDS), cts.Token);
                    var completedTask = await Task.WhenAny(_conflictTcs.Task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        BizSimGamesLogger.Warning("Conflict resolution timeout - using local data");
                        // Timeout fallback: use local (safer)
                    }
                    else
                    {
                        var resolution = await _conflictTcs.Task;
                        // Resolution applied by conflict handler
                    }
                }
                catch (OperationCanceledException)
                {
                    BizSimGamesLogger.Warning("Conflict resolution cancelled");
                }
            }

            await CommitSnapshotAsync(handle, data, description, 0, null, ct);
        }

        public async Task<byte[]> LoadAsync(string filename, CancellationToken ct = default)
        {
            try
            {
                var handle = await OpenSnapshotAsync(filename, false, ct);
                return await ReadSnapshotAsync(handle, ct);
            }
            catch
            {
                return null; // Snapshot not found
            }
        }

        internal void OnSnapshotOpenedFromJava(string filename, string snapshotJson, bool hasConflict)
        {
            try
            {
                var handle = JsonUtility.FromJson<SnapshotHandle>(snapshotJson);
                handle.hasConflict = hasConflict;

                OnSnapshotOpened?.Invoke(handle);
                _openTcs?.TrySetResult(handle);
            }
            catch (Exception ex)
            {
                _openTcs?.TrySetException(ex);
            }
        }

        internal void OnSnapshotReadFromJava(string filename, byte[] data)
        {
            _readTcs?.TrySetResult(data);
        }

        internal void OnSnapshotCommittedFromJava(string filename)
        {
            OnSnapshotCommitted?.Invoke(filename);
            _commitTcs?.TrySetResult(true);
        }

        internal void OnSnapshotDeletedFromJava(string filename)
        {
            _deleteTcs?.TrySetResult(true);
        }

        internal void OnSavedGamesUIResultFromJava(string selectedFilename)
        {
            _showUITcs?.TrySetResult(selectedFilename);
        }

        internal void OnConflictDetectedFromJava(string localSnapshotJson, string serverSnapshotJson,
            byte[] localData, byte[] serverData)
        {
            try
            {
                var conflict = new SavedGameConflict
                {
                    localSnapshot = JsonUtility.FromJson<SnapshotHandle>(localSnapshotJson),
                    serverSnapshot = JsonUtility.FromJson<SnapshotHandle>(serverSnapshotJson),
                    localData = localData,
                    serverData = serverData,
                    ResolveAsync = async (resolution) =>
                    {
                        _conflictTcs?.TrySetResult(resolution);
                        _cloudSaveBridge.Call("resolveConflict", resolution.ToString(), "");
                        await Task.CompletedTask;
                    }
                };

                OnConflictDetected?.Invoke(conflict);
            }
            catch (Exception ex)
            {
                BizSimGamesLogger.Error($"Conflict handling error: {ex.Message}");
            }
        }

        internal void OnCloudSaveErrorFromJava(int errorCode, string errorMessage, string filename)
        {
            var error = new GamesCloudSaveError(errorCode, errorMessage, filename);
            OnCloudSaveError?.Invoke(error);

            var exception = new Exception(error.ToString());
            _openTcs?.TrySetException(exception);
            _readTcs?.TrySetException(exception);
            _commitTcs?.TrySetException(exception);
            _deleteTcs?.TrySetException(exception);
            _showUITcs?.TrySetException(exception);
        }
    }
}
