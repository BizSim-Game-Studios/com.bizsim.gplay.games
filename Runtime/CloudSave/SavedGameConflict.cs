// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Represents a conflict between local and cloud saves.
    /// Game must choose a resolution strategy via ResolveAsync.
    /// </summary>
    public class SavedGameConflict
    {
        /// <summary>
        /// Local snapshot (current device data).
        /// </summary>
        public SnapshotHandle localSnapshot;

        /// <summary>
        /// Server snapshot (cloud data).
        /// </summary>
        public SnapshotHandle serverSnapshot;

        /// <summary>
        /// Local save data (byte array).
        /// </summary>
        public byte[] localData;

        /// <summary>
        /// Server save data (byte array).
        /// </summary>
        public byte[] serverData;

        /// <summary>
        /// Resolves the conflict by choosing a resolution strategy.
        /// Game must call this from OnConflictDetected event handler.
        /// </summary>
        public Func<ConflictResolution, Task> ResolveAsync { get; internal set; }
    }

    /// <summary>
    /// Conflict resolution strategy.
    /// </summary>
    public enum ConflictResolution
    {
        /// <summary>
        /// Use local device data (discard cloud data).
        /// </summary>
        UseLocal = 0,

        /// <summary>
        /// Use cloud data (discard local changes).
        /// </summary>
        UseServer = 1,

        /// <summary>
        /// Merge both (game-specific logic required).
        /// Not implemented in this version - falls back to UseLocal.
        /// </summary>
        UseManual = 2
    }
}
