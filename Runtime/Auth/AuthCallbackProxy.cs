// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// AndroidJavaProxy implementation of IAuthCallback (Java interface).
    /// Receives callbacks from AuthBridge.java on background threads.
    /// Marshals to Unity main thread via UnityMainThreadDispatcher.
    ///
    /// ProGuard-Safe: Uses interface name matching (not UnitySendMessage).
    /// Requires ProGuard keep rules for com.bizsim.gplay.games.callbacks.IAuthCallback.
    /// </summary>
    internal class AuthCallbackProxy : AndroidJavaProxy
    {
        private readonly GamesAuthController _controller;

        /// <summary>
        /// Creates proxy for Java callback interface.
        /// Interface name MUST match Java: "com.bizsim.gplay.games.callbacks.IAuthCallback"
        /// </summary>
        public AuthCallbackProxy(GamesAuthController controller)
            : base("com.bizsim.gplay.games.callbacks.IAuthCallback")
        {
            _controller = controller;
            BizSimGamesLogger.Verbose("AuthCallbackProxy created");
        }

        /// <summary>
        /// Called from Java (AuthBridge.java) on background thread when auth succeeds.
        /// Method name MUST match Java interface: "onAuthSuccess"
        /// ProGuard keep rules required for this method.
        /// </summary>
        void onAuthSuccess(string playerId, string displayName, string avatarUri)
        {
            BizSimGamesLogger.Verbose($"[Callback] onAuthSuccess: playerId={playerId}, name={displayName}");

            // Marshal to Unity main thread (Java callbacks run on background threads)
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                _controller.OnAuthSuccess(playerId, displayName, avatarUri);
            });
        }

        /// <summary>
        /// Called from Java (AuthBridge.java) on background thread when auth fails.
        /// Method name MUST match Java interface: "onAuthFailure"
        /// ProGuard keep rules required for this method.
        /// </summary>
        void onAuthFailure(int errorCode, string errorMessage)
        {
            BizSimGamesLogger.Verbose($"[Callback] onAuthFailure: code={errorCode}, message={errorMessage}");

            // Marshal to Unity main thread
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                _controller.OnAuthFailure(errorCode, errorMessage);
            });
        }

        /// <summary>
        /// Called from Java when server-side access auth code is retrieved.
        /// </summary>
        void onServerSideAccessSuccess(string serverAuthCode)
        {
            BizSimGamesLogger.Verbose($"[Callback] onServerSideAccessSuccess");

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                _controller.OnServerSideAccessSuccess(serverAuthCode);
            });
        }

        /// <summary>
        /// Called from Java when server-side access request fails.
        /// </summary>
        void onServerSideAccessFailure(int errorCode, string errorMessage)
        {
            BizSimGamesLogger.Verbose($"[Callback] onServerSideAccessFailure: code={errorCode}, message={errorMessage}");

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                _controller.OnServerSideAccessFailure(errorCode, errorMessage);
            });
        }

        void onServerSideAccessWithScopesSuccess(string authCode, string grantedScopesJson)
        {
            BizSimGamesLogger.Verbose("[Callback] onServerSideAccessWithScopesSuccess");

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                _controller.OnScopedAccessSuccess(authCode, grantedScopesJson);
            });
        }

        void onServerSideAccessWithScopesFailure(int errorCode, string errorMessage)
        {
            BizSimGamesLogger.Verbose($"[Callback] onServerSideAccessWithScopesFailure: code={errorCode}, message={errorMessage}");

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                _controller.OnScopedAccessFailure(errorCode, errorMessage);
            });
        }
    }
}
