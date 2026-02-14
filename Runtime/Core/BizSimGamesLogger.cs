// Copyright (c) BizSim Game Studios. All rights reserved.
// https://www.bizsim.com

using Debug = UnityEngine.Debug;

namespace BizSim.GPlay.Games
{
    public enum LogLevel
    {
        Verbose = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Silent = 4
    }

    internal static class BizSimGamesLogger
    {
        private const string Prefix = "[PlayGames]";

        internal static LogLevel MinLevel { get; set; } = LogLevel.Verbose;

        internal static bool ForceDebug { get; set; }

#if DEBUG
        private static readonly bool IsDebugBuild = true;
#else
        private static readonly bool IsDebugBuild = false;
#endif

        internal static void Verbose(string message)
        {
            if (!IsDebugBuild && !ForceDebug) return;
            if (MinLevel <= LogLevel.Verbose)
                Debug.Log($"{Prefix} [V] {message}");
        }

        internal static void Info(string message)
        {
            if (!IsDebugBuild && !ForceDebug) return;
            if (MinLevel <= LogLevel.Info)
                Debug.Log($"{Prefix} {message}");
        }

        internal static void Warning(string message)
        {
            if (MinLevel <= LogLevel.Warning)
                Debug.LogWarning($"{Prefix} {message}");
        }

        internal static void Error(string message)
        {
            if (MinLevel <= LogLevel.Error)
                Debug.LogError($"{Prefix} {message}");
        }
    }
}
