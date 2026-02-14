// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal class MockCloudSaveProvider : IGamesCloudSaveProvider
    {
        private readonly GamesServicesConfig.MockSettings _mock;
        private Dictionary<string, byte[]> _mockSnapshots = new Dictionary<string, byte[]>();

        public event Action<SnapshotHandle> OnSnapshotOpened;
        public event Action<string> OnSnapshotCommitted;
        public event Action<SavedGameConflict> OnConflictDetected;
        public event Action<GamesCloudSaveError> OnCloudSaveError;

        public MockCloudSaveProvider(GamesServicesConfig.MockSettings mock)
        {
            _mock = mock;
            BizSimGamesLogger.Info("MockCloudSaveProvider initialized");
        }

        private void ThrowIfSimulatingErrors(string filename = null)
        {
            if (_mock == null || !_mock.mockSimulateErrors) return;

            var error = new GamesCloudSaveError(GamesErrorCodes.NetworkError, "Simulated network error", filename);
            OnCloudSaveError?.Invoke(error);
            throw new GamesCloudSaveException(error);
        }

        public async Task<SnapshotHandle> OpenSnapshotAsync(string filename, bool createIfNotFound = true, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);
            ThrowIfSimulatingErrors(filename);

            if (!createIfNotFound && !_mockSnapshots.ContainsKey(filename))
            {
                var error = new GamesCloudSaveError(3, "Snapshot not found", filename);
                OnCloudSaveError?.Invoke(error);
                throw new GamesCloudSaveException(error);
            }

            var handle = new SnapshotHandle
            {
                filename = filename,
                nativeHandle = "mock:" + filename,
                hasConflict = false,
                lastModifiedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            OnSnapshotOpened?.Invoke(handle);
            return handle;
        }

        public async Task<byte[]> ReadSnapshotAsync(SnapshotHandle handle, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);
            ThrowIfSimulatingErrors(handle.filename);
            return _mockSnapshots.ContainsKey(handle.filename) ? _mockSnapshots[handle.filename] : null;
        }

        public async Task CommitSnapshotAsync(SnapshotHandle handle, byte[] data, string description = null,
            long playedTimeMillis = 0, byte[] coverImage = null, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            ThrowIfSimulatingErrors(handle.filename);

            _mockSnapshots[handle.filename] = data;
            BizSimGamesLogger.Info($"[MOCK] Snapshot committed: {handle.filename} ({data.Length} bytes)");
            OnSnapshotCommitted?.Invoke(handle.filename);
        }

        public async Task DeleteSnapshotAsync(string filename, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);
            ThrowIfSimulatingErrors(filename);

            _mockSnapshots.Remove(filename);
            BizSimGamesLogger.Info($"[MOCK] Snapshot deleted: {filename}");
        }

        public async Task<string> ShowSavedGamesUIAsync(string title = "Saved Games", bool allowAddButton = false,
            bool allowDelete = true, int maxSnapshots = 5, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            ThrowIfSimulatingErrors();

            BizSimGamesLogger.Info("[MOCK] Saved games UI shown");
            return null;
        }

        public async Task SaveAsync(string filename, byte[] data, string description = null, CancellationToken ct = default)
        {
            var handle = await OpenSnapshotAsync(filename, true, ct);
            FireConflictIfExists(handle, data);
            await CommitSnapshotAsync(handle, data, description, 0, null, ct);
        }

        public async Task SaveAsync(string filename, byte[] data, SaveGameMetadata metadata, CancellationToken ct = default)
        {
            var handle = await OpenSnapshotAsync(filename, true, ct);
            FireConflictIfExists(handle, data);
            await CommitSnapshotAsync(handle, data, metadata?.description,
                metadata?.playedTimeMillis ?? 0, metadata?.coverImage, ct);
        }

        private void FireConflictIfExists(SnapshotHandle handle, byte[] incomingData)
        {
            if (_mock == null || !_mock.mockSimulateConflict)
                return;

            if (!_mockSnapshots.TryGetValue(handle.filename, out var existingData))
                return;

            BizSimGamesLogger.Info($"[MOCK] Conflict detected for '{handle.filename}'");

            var conflict = new SavedGameConflict
            {
                localSnapshot = handle,
                serverSnapshot = new SnapshotHandle
                {
                    filename = handle.filename,
                    nativeHandle = "mock:server:" + handle.filename,
                    lastModifiedTimestamp = handle.lastModifiedTimestamp - 1000
                },
                localData = incomingData,
                serverData = existingData
            };
            OnConflictDetected?.Invoke(conflict);
        }

        public Task<Texture2D> DownloadCoverImageAsync(string coverImageUri, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(coverImageUri))
                return Task.FromResult<Texture2D>(null);

            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.gray);
            tex.Apply();
            return Task.FromResult(tex);
        }

        public void ReleaseCoverImage(Texture2D texture)
        {
            if (texture != null)
                UnityEngine.Object.Destroy(texture);
        }

        public void ReleaseAllCoverImages()
        {
            BizSimGamesLogger.Info("[MOCK] All cover images released");
        }

        public async Task<byte[]> LoadAsync(string filename, CancellationToken ct = default)
        {
            if (!_mockSnapshots.ContainsKey(filename))
            {
                BizSimGamesLogger.Info($"[MOCK] Snapshot '{filename}' not found, returning null");
                return null;
            }

            var handle = await OpenSnapshotAsync(filename, false, ct);
            return await ReadSnapshotAsync(handle, ct);
        }
    }
}
