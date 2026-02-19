// Copyright (c) BizSim Game Studios. All rights reserved.

using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal static class TcsGuard
    {
        internal static TaskCompletionSource<T> Replace<T>(ref TaskCompletionSource<T> field)
        {
            var previous = Interlocked.Exchange(ref field, new TaskCompletionSource<T>());
            if (previous != null)
            {
                bool wasCanceled = previous.TrySetCanceled();
                if (wasCanceled)
                    BizSimGamesLogger.Warning($"[TcsGuard] Previous TCS<{typeof(T).Name}> was still pending — canceled it");
            }
            return field;
        }
    }
}
