// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Provider interface for Google Play Games Cloud Save (Snapshots API).
    /// Uses transaction-based pattern: Open → Read/Write → Commit.
    /// </summary>
    public interface IGamesCloudSaveProvider
    {
        /// <summary>
        /// Opens a snapshot for reading or writing (transaction start).
        /// IMPORTANT: Must call CommitSnapshotAsync after writing to save changes.
        /// </summary>
        /// <param name="filename">Snapshot filename (unique save slot)</param>
        /// <param name="createIfNotFound">Create snapshot if it doesn't exist</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Snapshot handle for subsequent operations</returns>
        Task<SnapshotHandle> OpenSnapshotAsync(string filename, bool createIfNotFound = true, CancellationToken ct = default);

        /// <summary>
        /// Reads data from an open snapshot.
        /// </summary>
        /// <param name="handle">Snapshot handle from OpenSnapshotAsync</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Snapshot data as byte array</returns>
        Task<byte[]> ReadSnapshotAsync(SnapshotHandle handle, CancellationToken ct = default);

        /// <summary>
        /// Commits changes to a snapshot (transaction end).
        /// This writes data to Google Play cloud storage.
        ///
        /// ⚠️ GOOGLE PLAY QUALITY CHECKLIST REQUIREMENT (6.1):
        /// Saved games MUST include metadata:
        /// • Cover Image: Screenshot capturing game progress (helps player remember where they left off)
        /// • Description: Short text providing additional context
        /// • Timestamp: How long player has been playing this save (playedTimeMillis)
        ///
        /// Failure to provide metadata may result in app rejection or removal from Google Play.
        /// </summary>
        /// <param name="handle">Snapshot handle from OpenSnapshotAsync</param>
        /// <param name="data">Data to save (byte array)</param>
        /// <param name="description">REQUIRED: Save description (visible in Google Play UI)</param>
        /// <param name="playedTimeMillis">REQUIRED: Total played time in milliseconds</param>
        /// <param name="coverImage">REQUIRED: Cover image (PNG, max 800KB) - screenshot of game state</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when commit succeeds</returns>
        Task CommitSnapshotAsync(SnapshotHandle handle, byte[] data, string description = null,
            long playedTimeMillis = 0, byte[] coverImage = null, CancellationToken ct = default);

        /// <summary>
        /// Deletes a snapshot from cloud storage.
        /// </summary>
        /// <param name="filename">Snapshot filename to delete</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when deletion succeeds</returns>
        Task DeleteSnapshotAsync(string filename, CancellationToken ct = default);

        /// <summary>
        /// Shows the native Google Play Games saved games UI.
        /// Allows player to view/delete saves.
        /// </summary>
        /// <param name="title">UI title</param>
        /// <param name="allowAddButton">Allow creating new saves from UI</param>
        /// <param name="allowDelete">Allow deleting saves from UI</param>
        /// <param name="maxSnapshots">Maximum number of snapshots to display</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Selected snapshot filename (or null if cancelled)</returns>
        Task<string> ShowSavedGamesUIAsync(string title = "Saved Games", bool allowAddButton = false,
            bool allowDelete = true, int maxSnapshots = 5, CancellationToken ct = default);

        /// <summary>
        /// Convenience method: Save data in one call (Open → Write → Commit).
        /// Handles conflicts automatically with timeout protection.
        ///
        /// ⚠️ IMPORTANT: For full Google Play Quality Checklist compliance, use CommitSnapshotAsync
        /// with cover image and playedTimeMillis. This simplified method is for basic usage only.
        /// </summary>
        /// <param name="filename">Snapshot filename</param>
        /// <param name="data">Data to save</param>
        /// <param name="description">Save description (recommended for compliance)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when save succeeds</returns>
        Task SaveAsync(string filename, byte[] data, string description = null, CancellationToken ct = default);

        /// <summary>
        /// Convenience method: Save data with full metadata in one call.
        /// Validates metadata for Sidekick Tier 2 compliance when requireCloudSaveMetadata is enabled.
        /// </summary>
        Task SaveAsync(string filename, byte[] data, SaveGameMetadata metadata, CancellationToken ct = default);

        /// <summary>
        /// Downloads a cover image from Google servers using the URI from SnapshotHandle.coverImageUri.
        /// WARNING: The returned Texture2D uses unmanaged GPU memory that is NOT garbage collected.
        /// Caller MUST call UnityEngine.Object.Destroy(texture) when the texture is no longer needed,
        /// otherwise GPU memory will leak.
        /// </summary>
        Task<Texture2D> DownloadCoverImageAsync(string coverImageUri, CancellationToken ct = default);

        /// <summary>
        /// Releases a cover image texture from internal cache and destroys the GPU resource.
        /// Call this when a cover image is no longer displayed (e.g., scrolling off-screen in a save list).
        /// </summary>
        /// <param name="texture">The texture returned by DownloadCoverImageAsync</param>
        void ReleaseCoverImage(Texture2D texture);

        /// <summary>
        /// Releases all cached cover image textures and destroys their GPU resources.
        /// Call this when leaving the saved games UI to free all GPU memory at once.
        /// </summary>
        void ReleaseAllCoverImages();

        /// <summary>
        /// Convenience method: Load data in one call (Open → Read).
        /// </summary>
        /// <param name="filename">Snapshot filename</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Snapshot data (or null if not found)</returns>
        Task<byte[]> LoadAsync(string filename, CancellationToken ct = default);

        /// <summary>
        /// Event fired when a snapshot is successfully opened.
        /// </summary>
        event Action<SnapshotHandle> OnSnapshotOpened;

        /// <summary>
        /// Event fired when a snapshot is successfully committed.
        /// </summary>
        event Action<string> OnSnapshotCommitted;

        /// <summary>
        /// Event fired when a conflict is detected during save.
        /// Game must handle conflict resolution via SavedGameConflict.ResolveAsync.
        /// </summary>
        event Action<SavedGameConflict> OnConflictDetected;

        /// <summary>
        /// Event fired when a cloud save operation fails.
        /// </summary>
        event Action<GamesCloudSaveError> OnCloudSaveError;
    }
}
