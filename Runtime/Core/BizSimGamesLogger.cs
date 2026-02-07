// Copyright (c) BizSim Game Studios. All rights reserved.
// https://www.bizsim.com

using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Log severity levels for <see cref="BizSimGamesLogger"/>.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>All messages including verbose traces.</summary>
        Verbose = 0,

        /// <summary>Informational messages and above.</summary>
        Info = 1,

        /// <summary>Warnings and errors only.</summary>
        Warning = 2,

        /// <summary>Errors only.</summary>
        Error = 3,

        /// <summary>No logging at all — completely silent.</summary>
        Silent = 4
    }

    /// <summary>
    /// Zero-allocation logger for release builds with configurable log level.
    /// Methods marked with [Conditional("DEBUG")] are stripped in release builds.
    /// </summary>
    internal static class BizSimGamesLogger
    {
        private const string Prefix = "[PlayGames]";

        /// <summary>Minimum log level. Defaults to Verbose.</summary>
        internal static LogLevel MinLevel { get; set; } = LogLevel.Verbose;

        [Conditional("DEBUG")]
        internal static void Verbose(string message)
        {
            if (MinLevel <= LogLevel.Verbose)
                Debug.Log($"{Prefix} [V] {message}");
        }

        [Conditional("DEBUG")]
        internal static void Info(string message)
        {
            if (MinLevel <= LogLevel.Info)
                Debug.Log($"{Prefix} {message}");
        }

        [Conditional("DEBUG")]
        internal static void Warning(string message)
        {
            if (MinLevel <= LogLevel.Warning)
                Debug.LogWarning($"{Prefix} {message}");
        }

        /// <summary>
        /// Logs an error. NOT conditional — survives compiler stripping in release builds.
        /// </summary>
        internal static void Error(string message)
        {
            if (MinLevel <= LogLevel.Error)
                Debug.LogError($"{Prefix} {message}");
        }
    }
}
