// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BizSim.GPlay.Games
{
    internal class GamesCloudSaveController : JniBridgeBase, IGamesCloudSaveProvider
    {
        private int ConflictTimeoutSeconds
        {
            get
            {
                var config = GamesServicesManager.Config;
                return config != null ? config.conflictTimeoutSeconds : 60;
            }
        }

        private CloudSaveCallbackProxy _callbackProxy;

        private TaskCompletionSource<SnapshotHandle> _openTcs;
        private TaskCompletionSource<byte[]> _readTcs;
        private TaskCompletionSource<bool> _commitTcs;
        private TaskCompletionSource<bool> _deleteTcs;
        private TaskCompletionSource<string> _showUITcs;
        private TaskCompletionSource<ConflictResolution> _conflictTcs;

        private readonly Dictionary<string, Texture2D> _coverImageCache = new();

        public event Action<SnapshotHandle> OnSnapshotOpened;
        public event Action<string> OnSnapshotCommitted;
        public event Action<SavedGameConflict> OnConflictDetected;
        public event Action<GamesCloudSaveError> OnCloudSaveError;

        protected override string JavaClassName => JniConstants.CloudSaveBridge;

        protected override AndroidJavaProxy CreateCallbackProxy()
        {
            _callbackProxy = new CloudSaveCallbackProxy(this);
            return _callbackProxy;
        }

        public GamesCloudSaveController()
        {
            InitializeBridge();
        }

        public async Task<SnapshotHandle> OpenSnapshotAsync(string filename, bool createIfNotFound = true, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _openTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("openSnapshot", filename, createIfNotFound);
                return await tcs.Task.WithJniTimeout(tcs, ct: ct);
            }
        }

        public async Task<byte[]> ReadSnapshotAsync(SnapshotHandle handle, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _readTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("readSnapshot", handle.nativeHandle);
                return await tcs.Task;
            }
        }

        public async Task CommitSnapshotAsync(SnapshotHandle handle, byte[] data, string description = null,
            long playedTimeMillis = 0, byte[] coverImage = null, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var config = GamesServicesManager.Config;
            if (config != null && config.requireCloudSaveMetadata)
            {
                if (string.IsNullOrEmpty(description))
                    BizSimGamesLogger.Warning("[CloudSave] requireCloudSaveMetadata is true but description is empty.");
                if (playedTimeMillis <= 0)
                    BizSimGamesLogger.Warning("[CloudSave] requireCloudSaveMetadata is true but playedTimeMillis is 0.");
            }

            var tcs = TcsGuard.Replace(ref _commitTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("commitSnapshot", handle.nativeHandle, data,
                    description ?? "", playedTimeMillis, coverImage);
                await tcs.Task.WithJniTimeout(tcs, ct: ct);
            }
        }

        public async Task DeleteSnapshotAsync(string filename, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _deleteTcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("deleteSnapshot", filename);
                await tcs.Task;
            }
        }

        public async Task<string> ShowSavedGamesUIAsync(string title = "Saved Games", bool allowAddButton = false,
            bool allowDelete = true, int maxSnapshots = 5, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var tcs = TcsGuard.Replace(ref _showUITcs);

            using (ct.Register(() => tcs.TrySetCanceled()))
            {
                CallBridge("showSavedGamesUI", title, allowAddButton, allowDelete, maxSnapshots);
                return await tcs.Task;
            }
        }

        public async Task SaveAsync(string filename, byte[] data, string description = null, CancellationToken ct = default)
        {
            var config = GamesServicesManager.Config;
            if (config != null && config.requireCloudSaveMetadata)
                BizSimGamesLogger.Warning("[CloudSave] Using SaveAsync without metadata. " +
                    "For Sidekick Tier 2 compliance, use the SaveAsync(filename, data, SaveGameMetadata) overload.");

            var handle = await OpenSnapshotAsync(filename, true, ct);

            if (handle.hasConflict)
                handle = await HandleConflictWithTimeout(ct);

            await CommitSnapshotAsync(handle, data, description, 0, null, ct);
        }

        public async Task SaveAsync(string filename, byte[] data, SaveGameMetadata metadata, CancellationToken ct = default)
        {
            ValidateMetadata(metadata);

            var handle = await OpenSnapshotAsync(filename, true, ct);

            if (handle.hasConflict)
                handle = await HandleConflictWithTimeout(ct);

            await CommitSnapshotAsync(handle, data, metadata?.description,
                metadata?.playedTimeMillis ?? 0, metadata?.coverImage, ct);
        }

        public async Task<Texture2D> DownloadCoverImageAsync(string coverImageUri, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(coverImageUri))
                return null;

            if (_coverImageCache.TryGetValue(coverImageUri, out var cached) && cached != null)
                return cached;

            using var request = UnityWebRequestTexture.GetTexture(coverImageUri);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                BizSimGamesLogger.Error($"[CloudSave] Cover image download failed: {request.error}");
                return null;
            }

            var texture = DownloadHandlerTexture.GetContent(request);
            _coverImageCache[coverImageUri] = texture;
            return texture;
        }

        public void ReleaseCoverImage(Texture2D texture)
        {
            if (texture == null) return;

            string keyToRemove = null;
            foreach (var kvp in _coverImageCache)
            {
                if (kvp.Value == texture)
                {
                    keyToRemove = kvp.Key;
                    break;
                }
            }

            if (keyToRemove != null)
                _coverImageCache.Remove(keyToRemove);

            UnityEngine.Object.Destroy(texture);
        }

        public void ReleaseAllCoverImages()
        {
            foreach (var kvp in _coverImageCache)
            {
                if (kvp.Value != null)
                    UnityEngine.Object.Destroy(kvp.Value);
            }
            _coverImageCache.Clear();
        }

        private void ValidateMetadata(SaveGameMetadata metadata)
        {
            var config = GamesServicesManager.Config;
            if (config == null || !config.requireCloudSaveMetadata)
                return;

            if (string.IsNullOrEmpty(metadata?.description))
                BizSimGamesLogger.Warning("[CloudSave] Sidekick Tier 2 requires save description.");

            if (metadata == null || metadata.playedTimeMillis <= 0)
                BizSimGamesLogger.Warning("[CloudSave] Sidekick Tier 2 requires playedTimeMillis > 0.");

            if (metadata?.coverImage == null || metadata.coverImage.Length == 0)
                BizSimGamesLogger.Warning("[CloudSave] Sidekick Tier 2 recommends cover image.");

            if (metadata?.coverImage != null && metadata.coverImage.Length > 800 * 1024)
                BizSimGamesLogger.Error("[CloudSave] Cover image exceeds 800KB hard limit (" +
                    (metadata.coverImage.Length / 1024) + "KB). Commit will fail. " +
                    "Resize to 640x360 or lower.");
            else if (metadata?.coverImage != null && metadata.coverImage.Length > 512 * 1024)
                BizSimGamesLogger.Warning("[CloudSave] Cover image is " +
                    (metadata.coverImage.Length / 1024) + "KB. Recommended max 512KB for safety margin " +
                    "(Google limit: 800KB). Consider resizing to 640x360.");
        }

        private SavedGameConflict _lastConflict;

        private async Task<SnapshotHandle> HandleConflictWithTimeout(CancellationToken ct)
        {
            _conflictTcs = new TaskCompletionSource<ConflictResolution>();

            int timeoutSeconds = ConflictTimeoutSeconds;
            ConflictResolution resolution;

            if (timeoutSeconds <= 0)
            {
                resolution = ResolveByTimestamp();
            }
            else
            {
                try
                {
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), ct);
                    var completedTask = await Task.WhenAny(_conflictTcs.Task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        resolution = ResolveByTimestamp();
                        BizSimGamesLogger.Warning($"Conflict resolution timeout ({timeoutSeconds}s) - auto-resolved: {resolution}");
                    }
                    else
                    {
                        resolution = await _conflictTcs.Task;
                    }
                }
                catch (OperationCanceledException)
                {
                    resolution = ResolveByTimestamp();
                    BizSimGamesLogger.Warning($"Conflict resolution cancelled - auto-resolved: {resolution}");
                }
            }

            var tcs = TcsGuard.Replace(ref _openTcs);

            CallBridge("resolveConflict", resolution.ToString(), "");

            var resolvedHandle = await tcs.Task;
            return resolvedHandle;
        }

        private ConflictResolution ResolveByTimestamp()
        {
            if (_lastConflict == null)
                return ConflictResolution.UseServer;

            long localTime = _lastConflict.localSnapshot?.lastModifiedTimestamp ?? 0;
            long serverTime = _lastConflict.serverSnapshot?.lastModifiedTimestamp ?? 0;

            if (localTime > serverTime)
            {
                BizSimGamesLogger.Info($"[CloudSave] Auto-resolve: UseLocal (local={localTime} > server={serverTime})");
                return ConflictResolution.UseLocal;
            }

            BizSimGamesLogger.Info($"[CloudSave] Auto-resolve: UseServer (server={serverTime} >= local={localTime})");
            return ConflictResolution.UseServer;
        }

        public async Task<byte[]> LoadAsync(string filename, CancellationToken ct = default)
        {
            try
            {
                var handle = await OpenSnapshotAsync(filename, false, ct);
                return await ReadSnapshotAsync(handle, ct);
            }
            catch (GamesCloudSaveException ex) when (ex.Error.Type == CloudSaveErrorType.SnapshotNotFound)
            {
                BizSimGamesLogger.Info($"[CloudSave] Snapshot '{filename}' not found, returning null");
                return null;
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
                        await Task.CompletedTask;
                    }
                };

                _lastConflict = conflict;
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

            var exception = new GamesCloudSaveException(error);
            _openTcs?.TrySetException(exception);
            _readTcs?.TrySetException(exception);
            _commitTcs?.TrySetException(exception);
            _deleteTcs?.TrySetException(exception);
            _showUITcs?.TrySetException(exception);
        }

        protected override void OnDispose()
        {
            _openTcs?.TrySetCanceled();
            _readTcs?.TrySetCanceled();
            _commitTcs?.TrySetCanceled();
            _deleteTcs?.TrySetCanceled();
            _showUITcs?.TrySetCanceled();

            ReleaseAllCoverImages();
            _callbackProxy = null;

            try { Bridge?.Call("shutdown"); }
            catch (System.Exception) { }
        }
    }
}
