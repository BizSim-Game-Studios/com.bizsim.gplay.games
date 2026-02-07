// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Handle to an open snapshot (transaction context).
    /// Used for Read/Write/Commit operations.
    /// </summary>
    [Serializable]
    public class SnapshotHandle
    {
        /// <summary>
        /// Snapshot filename.
        /// </summary>
        public string filename;

        /// <summary>
        /// Whether this snapshot has a conflict that needs resolution.
        /// </summary>
        public bool hasConflict;

        /// <summary>
        /// Native snapshot reference (opaque handle for JNI).
        /// [SerializeField] required for JsonUtility deserialization of internal field.
        /// </summary>
        [SerializeField]
        internal string nativeHandle;

        /// <summary>
        /// Timestamp when snapshot was last modified (Unix milliseconds).
        /// </summary>
        public long lastModifiedTimestamp;

        /// <summary>
        /// Total played time in milliseconds.
        /// </summary>
        public long playedTimeMillis;

        /// <summary>
        /// Snapshot description.
        /// </summary>
        public string description;
    }
}
