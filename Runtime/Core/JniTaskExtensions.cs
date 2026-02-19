// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal static class JniTaskExtensions
    {
        private const int FallbackTimeoutMs = 30000;

        private static int ResolveTimeoutMs()
        {
            var config = GamesServicesManager.Config;
            return config != null ? config.jniTimeoutSeconds * 1000 : FallbackTimeoutMs;
        }

        internal static async Task<T> WithJniTimeout<T>(
            this Task<T> task,
            TaskCompletionSource<T> tcs,
            int timeoutMs = 0,
            CancellationToken ct = default)
        {
            if (timeoutMs <= 0)
                timeoutMs = ResolveTimeoutMs();

            var startTime = System.Diagnostics.Stopwatch.StartNew();
            BizSimGamesLogger.Info($"[JniTimeout] Timer started: {timeoutMs}ms, appFocused={UnityEngine.Application.isFocused}");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var timeoutTask = Task.Delay(timeoutMs, timeoutCts.Token);

            var completedTask = await Task.WhenAny(task, timeoutTask);

            startTime.Stop();

            if (completedTask == timeoutTask)
            {
                BizSimGamesLogger.Error($"[JniTimeout] TIMED OUT after {startTime.ElapsedMilliseconds}ms (limit={timeoutMs}ms), appFocused={UnityEngine.Application.isFocused}");
                tcs.TrySetException(new TimeoutException(
                    $"JNI operation timed out after {timeoutMs}ms (elapsed={startTime.ElapsedMilliseconds}ms, focused={UnityEngine.Application.isFocused})"));
                throw new TimeoutException($"JNI operation timed out after {timeoutMs}ms");
            }

            timeoutCts.Cancel();
            BizSimGamesLogger.Info($"[JniTimeout] Completed in {startTime.ElapsedMilliseconds}ms");
            return await task;
        }
    }
}
