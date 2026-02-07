// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Marshals Java callback invocations from background threads to Unity's main thread.
    /// Java callbacks execute on arbitrary threads — Unity API calls MUST happen on main thread.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    internal class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();
        private static readonly object _lock = new object();
        private static bool _isQuitting;

        /// <summary>
        /// Enqueues an action to be executed on the Unity main thread.
        /// Thread-safe — can be called from Java callbacks on background threads.
        /// </summary>
        public static void Enqueue(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (_isQuitting) return;

            lock (_lock)
            {
                _executionQueue.Enqueue(action);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null || _isQuitting) return;

            _isQuitting = false;
            Application.quitting += () => _isQuitting = true;

            var go = new GameObject("[UnityMainThreadDispatcher]");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        private void Update()
        {
            lock (_lock)
            {
                while (_executionQueue.Count > 0)
                {
                    try
                    {
                        _executionQueue.Dequeue().Invoke();
                    }
                    catch (Exception e)
                    {
                        BizSimGamesLogger.Error($"Main thread callback error: {e.Message}");
                    }
                }
            }
        }
    }
}
