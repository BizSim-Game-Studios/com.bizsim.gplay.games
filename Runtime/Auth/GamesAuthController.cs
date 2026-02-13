// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Android implementation of IGamesAuthProvider using JNI bridge to PGS v2 SDK.
    /// Uses AndroidJavaProxy for ProGuard-safe callbacks from Java.
    /// </summary>
    internal class GamesAuthController : IGamesAuthProvider
    {
        private AndroidJavaObject _authBridge;
        private AuthCallbackProxy _callbackProxy;
        private TaskCompletionSource<GamesPlayer> _authTaskSource;
        private TaskCompletionSource<string> _serverAccessTaskSource;
        private TaskCompletionSource<GamesAuthResponse> _scopedAccessTaskSource;
        private CancellationTokenSource _destroyTokenSource;

        private GamesPlayer _currentPlayer;
        private bool _isAuthenticated;

        public event Action<GamesPlayer> OnAuthenticationSuccess;
        public event Action<GamesAuthError> OnAuthenticationFailed;

        public bool IsAuthenticated => _isAuthenticated;
        public GamesPlayer CurrentPlayer => _currentPlayer;

        public GamesAuthController()
        {
            _destroyTokenSource = new CancellationTokenSource();
            InitializeJniBridge();
        }

        private void InitializeJniBridge()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                BizSimGamesLogger.Info("Initializing JNI bridge for authentication");

                // Get AuthBridge singleton from Java
                using (var bridgeClass = new AndroidJavaClass("com.bizsim.gplay.games.AuthBridge"))
                {
                    _authBridge = bridgeClass.CallStatic<AndroidJavaObject>("getInstance", GetUnityActivity());
                }

                // Create callback proxy (AndroidJavaProxy for ProGuard safety)
                _callbackProxy = new AuthCallbackProxy(this);
                _authBridge.Call("setCallback", _callbackProxy);

                BizSimGamesLogger.Info("JNI bridge initialized successfully");
            }
            catch (Exception e)
            {
                BizSimGamesLogger.Error($"JNI bridge initialization failed: {e.Message}");
            }
            #else
            BizSimGamesLogger.Warning("GamesAuthController created on non-Android platform - use MockAuthProvider instead");
            #endif
        }

        public async Task<GamesPlayer> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (_authBridge == null)
            {
                var error = new GamesAuthError
                {
                    errorCode = -100,
                    errorMessage = "JNI bridge not initialized",
                    isRetryable = false
                };
                throw new GamesAuthException(error);
            }

            // Create TaskCompletionSource for async operation
            _authTaskSource = new TaskCompletionSource<GamesPlayer>();

            // Link external cancellation + internal destroy token
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _destroyTokenSource.Token))
            using (linkedCts.Token.Register(() => _authTaskSource?.TrySetCanceled()))
            {
                try
                {
                    BizSimGamesLogger.Info("Calling signIn() on Java bridge");
                    _authBridge.Call("signIn");

                    return await _authTaskSource.Task;
                }
                catch (OperationCanceledException)
                {
                    BizSimGamesLogger.Warning("Authentication cancelled");
                    throw;
                }
            }
            #else
            await Task.CompletedTask;
            throw new GamesAuthException(new GamesAuthError
            {
                errorCode = -1,
                errorMessage = "Not available on this platform",
                isRetryable = false
            });
            #endif
        }

        public async Task<string> RequestServerSideAccessAsync(string serverClientId, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (!_isAuthenticated)
            {
                throw new GamesAuthException(new GamesAuthError
                {
                    errorCode = 3,
                    errorMessage = "Not authenticated - call AuthenticateAsync first",
                    isRetryable = false
                });
            }

            if (_authBridge == null)
            {
                throw new GamesAuthException(new GamesAuthError
                {
                    errorCode = -100,
                    errorMessage = "JNI bridge not initialized",
                    isRetryable = false
                });
            }

            _serverAccessTaskSource = new TaskCompletionSource<string>();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _destroyTokenSource.Token))
            using (linkedCts.Token.Register(() => _serverAccessTaskSource?.TrySetCanceled()))
            {
                try
                {
                    BizSimGamesLogger.Info($"Requesting server-side access (clientId={serverClientId}, forceRefresh={forceRefresh})");
                    _authBridge.Call("requestServerSideAccess", serverClientId, forceRefresh);

                    return await _serverAccessTaskSource.Task;
                }
                catch (OperationCanceledException)
                {
                    BizSimGamesLogger.Warning("Server-side access request cancelled");
                    throw;
                }
            }
            #else
            await Task.CompletedTask;
            throw new GamesAuthException(new GamesAuthError
            {
                errorCode = -1,
                errorMessage = "Not available on this platform",
                isRetryable = false
            });
            #endif
        }

        /// <summary>
        /// Called by AuthCallbackProxy when Java bridge reports auth success.
        /// Executes on Unity main thread (marshaled by UnityMainThreadDispatcher).
        /// </summary>
        internal void OnAuthSuccess(string playerId, string displayName, string avatarUri)
        {
            _currentPlayer = new GamesPlayer(playerId, displayName, null, avatarUri);
            _isAuthenticated = true;

            BizSimGamesLogger.Info($"Auth success: {_currentPlayer}");

            OnAuthenticationSuccess?.Invoke(_currentPlayer);
            _authTaskSource?.TrySetResult(_currentPlayer);
        }

        /// <summary>
        /// Called by AuthCallbackProxy when Java bridge reports auth failure.
        /// Executes on Unity main thread.
        /// </summary>
        internal void OnAuthFailure(int errorCode, string errorMessage)
        {
            _isAuthenticated = false;
            _currentPlayer = null;

            var error = new GamesAuthError
            {
                errorCode = errorCode,
                errorMessage = errorMessage,
                isRetryable = errorCode == 2 // NoConnection is retryable
            };

            BizSimGamesLogger.Warning($"Auth failure: {error}");

            OnAuthenticationFailed?.Invoke(error);
            _authTaskSource?.TrySetException(new GamesAuthException(error));
        }

        /// <summary>
        /// Called by AuthCallbackProxy when server-side access auth code is received.
        /// </summary>
        internal void OnServerSideAccessSuccess(string serverAuthCode)
        {
            BizSimGamesLogger.Info("Server-side access granted");
            _serverAccessTaskSource?.TrySetResult(serverAuthCode);
        }

        /// <summary>
        /// Called by AuthCallbackProxy when server-side access request fails.
        /// </summary>
        internal void OnServerSideAccessFailure(int errorCode, string errorMessage)
        {
            var error = new GamesAuthError
            {
                errorCode = errorCode,
                errorMessage = errorMessage,
                isRetryable = false
            };

            BizSimGamesLogger.Warning($"Server-side access failure: {error}");
            _serverAccessTaskSource?.TrySetException(new GamesAuthException(error));
        }

        public async Task<GamesAuthResponse> RequestServerSideAccessWithScopesAsync(
            string serverClientId,
            bool forceRefresh,
            List<GamesAuthScope> scopes,
            CancellationToken cancellationToken = default)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (!_isAuthenticated)
            {
                throw new GamesAuthException(new GamesAuthError
                {
                    errorCode = 3,
                    errorMessage = "Not authenticated - call AuthenticateAsync first",
                    isRetryable = false
                });
            }

            if (_authBridge == null)
            {
                throw new GamesAuthException(new GamesAuthError
                {
                    errorCode = -100,
                    errorMessage = "JNI bridge not initialized",
                    isRetryable = false
                });
            }

            _scopedAccessTaskSource = new TaskCompletionSource<GamesAuthResponse>();

            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _destroyTokenSource.Token))
            using (linkedCts.Token.Register(() => _scopedAccessTaskSource?.TrySetCanceled()))
            {
                try
                {
                    string scopesJson = ScopesToJson(scopes);
                    BizSimGamesLogger.Info($"Requesting server-side access with scopes (clientId={serverClientId}, forceRefresh={forceRefresh}, scopes={scopesJson})");
                    _authBridge.Call("requestServerSideAccessWithScopes", serverClientId, forceRefresh, scopesJson);

                    return await _scopedAccessTaskSource.Task;
                }
                catch (OperationCanceledException)
                {
                    BizSimGamesLogger.Warning("Scoped server-side access request cancelled");
                    throw;
                }
            }
            #else
            await Task.CompletedTask;
            throw new GamesAuthException(new GamesAuthError
            {
                errorCode = -1,
                errorMessage = "Not available on this platform",
                isRetryable = false
            });
            #endif
        }

        internal void OnScopedAccessSuccess(string authCode, string grantedScopesJson)
        {
            var grantedScopes = ParseScopesJson(grantedScopesJson);
            var response = new GamesAuthResponse(authCode, grantedScopes);

            BizSimGamesLogger.Info($"Scoped server-side access granted (scopes={grantedScopesJson})");
            _scopedAccessTaskSource?.TrySetResult(response);
        }

        internal void OnScopedAccessFailure(int errorCode, string errorMessage)
        {
            var error = new GamesAuthError
            {
                errorCode = errorCode,
                errorMessage = errorMessage,
                isRetryable = false
            };

            BizSimGamesLogger.Warning($"Scoped server-side access failure: {error}");
            _scopedAccessTaskSource?.TrySetException(new GamesAuthException(error));
        }

        private static string ScopesToJson(List<GamesAuthScope> scopes)
        {
            if (scopes == null || scopes.Count == 0)
                return "[]";

            var parts = new List<string>(scopes.Count);
            foreach (var scope in scopes)
            {
                switch (scope)
                {
                    case GamesAuthScope.Email: parts.Add("\"EMAIL\""); break;
                    case GamesAuthScope.Profile: parts.Add("\"PROFILE\""); break;
                    case GamesAuthScope.OpenId: parts.Add("\"OPEN_ID\""); break;
                    default:
                        BizSimGamesLogger.Warning($"Unknown GamesAuthScope value: {scope}");
                        break;
                }
            }
            return "[" + string.Join(",", parts) + "]";
        }

        private static List<GamesAuthScope> ParseScopesJson(string json)
        {
            var result = new List<GamesAuthScope>();
            if (string.IsNullOrEmpty(json))
                return result;

            string trimmed = json.Trim().TrimStart('[').TrimEnd(']');
            if (string.IsNullOrEmpty(trimmed))
                return result;

            string[] items = trimmed.Split(',');
            foreach (string item in items)
            {
                string cleaned = item.Trim().Trim('"');
                switch (cleaned)
                {
                    case "EMAIL": result.Add(GamesAuthScope.Email); break;
                    case "PROFILE": result.Add(GamesAuthScope.Profile); break;
                    case "OPEN_ID": result.Add(GamesAuthScope.OpenId); break;
                }
            }

            return result;
        }

        private static AndroidJavaObject GetUnityActivity()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        ~GamesAuthController()
        {
            _destroyTokenSource?.Cancel();
            _destroyTokenSource?.Dispose();
        }
    }
}
