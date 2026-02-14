// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    [DefaultExecutionOrder(-1000)]
    internal class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static volatile UnityMainThreadDispatcher _instance;
        private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();
        private static bool _isQuitting;
        private static int _mainThreadId;

        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        public static void Enqueue(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (_isQuitting) return;

            _executionQueue.Enqueue(action);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null || _isQuitting) return;

            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _isQuitting = false;
            Application.quitting += () => _isQuitting = true;

            var go = new GameObject("[UnityMainThreadDispatcher]");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        private void Update()
        {
            while (_executionQueue.TryDequeue(out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    BizSimGamesLogger.Error($"Main thread callback error: {e}");
                }
            }
        }
    }
}
