// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    internal abstract class JniBridgeBase : IDisposable
    {
        private bool _disposed;

        protected AndroidJavaObject Bridge { get; private set; }

        protected abstract string JavaClassName { get; }

        protected abstract AndroidJavaProxy CreateCallbackProxy();

        protected void InitializeBridge()
        {
            try
            {
                string shortName = JavaClassName.Substring(JavaClassName.LastIndexOf('.') + 1);
                BizSimGamesLogger.Info($"Initializing {shortName}...");

                using (var unityPlayer = new AndroidJavaClass(JniConstants.UnityPlayer))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    Bridge = new AndroidJavaObject(JavaClassName, activity);
                    Bridge.Call("setCallback", CreateCallbackProxy());
                    BizSimGamesLogger.Info($"{shortName} initialized successfully");
                }
            }
            catch (Exception ex)
            {
                string shortName = JavaClassName.Substring(JavaClassName.LastIndexOf('.') + 1);
                BizSimGamesLogger.Error($"Failed to initialize {shortName}: {ex}");
                throw new GamesNativeBridgeException(JavaClassName, ex);
            }
        }

        protected void CallBridge(string method, params object[] args)
        {
            Bridge.Call(method, args);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            OnDispose();

            #if UNITY_ANDROID && !UNITY_EDITOR
            Bridge?.Dispose();
            Bridge = null;
            #endif
        }

        protected abstract void OnDispose();
    }
}
