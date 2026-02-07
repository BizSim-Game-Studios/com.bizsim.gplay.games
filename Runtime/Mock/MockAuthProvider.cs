// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Mock authentication provider for Editor testing.
    /// Returns simulated auth results based on GamesServicesMockConfig.
    /// </summary>
    internal class MockAuthProvider : IGamesAuthProvider
    {
        private readonly GamesServicesMockConfig _config;
        private GamesPlayer _currentPlayer;
        private bool _isAuthenticated;

        public event Action<GamesPlayer> OnAuthenticationSuccess;
        public event Action<GamesAuthError> OnAuthenticationFailed;

        public bool IsAuthenticated => _isAuthenticated;
        public GamesPlayer CurrentPlayer => _currentPlayer;

        public MockAuthProvider(GamesServicesMockConfig config)
        {
            _config = config;
        }

        public async Task<GamesPlayer> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            BizSimGamesLogger.Info("[Mock] AuthenticateAsync called");

            if (_config == null)
            {
                BizSimGamesLogger.Warning("[Mock] No config - simulating auth failure");
                var error = new GamesAuthError
                {
                    errorCode = 3,
                    errorMessage = "Mock config not assigned",
                    isRetryable = false
                };
                OnAuthenticationFailed?.Invoke(error);
                throw new GamesAuthException(error);
            }

            // Simulate network delay
            if (_config.authDelaySeconds > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(_config.authDelaySeconds), cancellationToken);
            }

            if (_config.authSucceeds)
            {
                // Simulate successful auth
                _currentPlayer = new GamesPlayer(
                    _config.mockPlayerId,
                    _config.mockDisplayName,
                    null, // No banner image in mock
                    null  // No hi-res image in mock
                );
                _isAuthenticated = true;

                BizSimGamesLogger.Info($"[Mock] Auth success: {_currentPlayer}");
                OnAuthenticationSuccess?.Invoke(_currentPlayer);
                return _currentPlayer;
            }
            else
            {
                // Simulate auth failure
                var error = new GamesAuthError
                {
                    errorCode = _config.GetAuthErrorCode(),
                    errorMessage = _config.GetAuthErrorMessage(),
                    isRetryable = false
                };

                _isAuthenticated = false;
                _currentPlayer = null;

                BizSimGamesLogger.Warning($"[Mock] Auth failed: {error}");
                OnAuthenticationFailed?.Invoke(error);
                throw new GamesAuthException(error);
            }
        }

        public async Task<string> RequestServerSideAccessAsync(string serverClientId, bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            BizSimGamesLogger.Info($"[Mock] RequestServerSideAccessAsync called (serverClientId={serverClientId})");

            if (!_isAuthenticated)
            {
                throw new GamesAuthException(new GamesAuthError
                {
                    errorCode = 3,
                    errorMessage = "Not authenticated - call AuthenticateAsync first",
                    isRetryable = false
                });
            }

            // Simulate delay
            await Task.Delay(100, cancellationToken);

            // Return mock OAuth2 code
            string mockAuthCode = $"mock_auth_code_{Guid.NewGuid():N}";
            BizSimGamesLogger.Info($"[Mock] Server-side access: {mockAuthCode}");
            return mockAuthCode;
        }
    }
}
