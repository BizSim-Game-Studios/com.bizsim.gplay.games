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
            previous?.TrySetCanceled();
            return field;
        }
    }
}
