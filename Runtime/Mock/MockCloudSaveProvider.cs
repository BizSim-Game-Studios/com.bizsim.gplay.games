// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal class MockCloudSaveProvider : IGamesCloudSaveProvider
    {
        private Dictionary<string, byte[]> _mockSnapshots = new Dictionary<string, byte[]>();

        public event Action<SnapshotHandle> OnSnapshotOpened;
        public event Action<string> OnSnapshotCommitted;

#pragma warning disable CS0067 // Events never used (mock doesn't simulate conflicts/errors)
        public event Action<SavedGameConflict> OnConflictDetected;
        public event Action<GamesCloudSaveError> OnCloudSaveError;
#pragma warning restore CS0067

        public MockCloudSaveProvider(GamesServicesConfig.MockSettings mock)
        {
            BizSimGamesLogger.Info("MockCloudSaveProvider initialized");
        }

        public async Task<SnapshotHandle> OpenSnapshotAsync(string filename, bool createIfNotFound = true, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);
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
            return _mockSnapshots.ContainsKey(handle.filename) ? _mockSnapshots[handle.filename] : null;
        }

        public async Task CommitSnapshotAsync(SnapshotHandle handle, byte[] data, string description = null,
            long playedTimeMillis = 0, byte[] coverImage = null, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            _mockSnapshots[handle.filename] = data;
            BizSimGamesLogger.Info($"[MOCK] Snapshot committed: {handle.filename} ({data.Length} bytes)");
            OnSnapshotCommitted?.Invoke(handle.filename);
        }

        public async Task DeleteSnapshotAsync(string filename, CancellationToken ct = default)
        {
            await Task.Delay(200, ct);
            _mockSnapshots.Remove(filename);
            BizSimGamesLogger.Info($"[MOCK] Snapshot deleted: {filename}");
        }

        public async Task<string> ShowSavedGamesUIAsync(string title = "Saved Games", bool allowAddButton = false,
            bool allowDelete = true, int maxSnapshots = 5, CancellationToken ct = default)
        {
            await Task.Delay(300, ct);
            BizSimGamesLogger.Info("[MOCK] Saved games UI shown");
            return null; // Cancelled
        }

        public async Task SaveAsync(string filename, byte[] data, string description = null, CancellationToken ct = default)
        {
            var handle = await OpenSnapshotAsync(filename, true, ct);
            await CommitSnapshotAsync(handle, data, description, 0, null, ct);
        }

        public async Task<byte[]> LoadAsync(string filename, CancellationToken ct = default)
        {
            var handle = await OpenSnapshotAsync(filename, false, ct);
            return await ReadSnapshotAsync(handle, ct);
        }
    }
}
