// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    internal class MockAuthProvider : IGamesAuthProvider
    {
        private readonly GamesServicesConfig.MockSettings _mock;
        private GamesPlayer _currentPlayer;
        private bool _isAuthenticated;

        public event Action<GamesPlayer> OnAuthenticationSuccess;
        public event Action<GamesAuthError> OnAuthenticationFailed;

        public bool IsAuthenticated => _isAuthenticated;
        public GamesPlayer CurrentPlayer => _currentPlayer;

        public MockAuthProvider(GamesServicesConfig.MockSettings mock)
        {
            _mock = mock;
        }

        public async Task<GamesPlayer> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            BizSimGamesLogger.Info("[Mock] AuthenticateAsync called");

            if (_mock == null)
            {
                BizSimGamesLogger.Warning("[Mock] No config - simulating auth failure");
                var error = new GamesAuthError
                {
                    errorCode = GamesErrorCodes.NotAuthenticated,
                    errorMessage = "Mock config not assigned",
                    isRetryable = false
                };
                OnAuthenticationFailed?.Invoke(error);
                throw new GamesAuthException(error);
            }

            if (_mock.authDelaySeconds > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(_mock.authDelaySeconds), cancellationToken);
            }

            if (_mock.authSucceeds)
            {
                _currentPlayer = new GamesPlayer(
                    _mock.mockPlayerId,
                    _mock.mockDisplayName
                );
                _isAuthenticated = true;

                BizSimGamesLogger.Info($"[Mock] Auth success: {_currentPlayer}");
                OnAuthenticationSuccess?.Invoke(_currentPlayer);
                return _currentPlayer;
            }
            else
            {
                var error = new GamesAuthError
                {
                    errorCode = _mock.GetAuthErrorCode(),
                    errorMessage = _mock.GetAuthErrorMessage(),
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
                    errorCode = GamesErrorCodes.NotAuthenticated,
                    errorMessage = "Not authenticated - call AuthenticateAsync first",
                    isRetryable = false
                });
            }

            await Task.Delay(100, cancellationToken);

            string mockAuthCode = $"mock_auth_code_{Guid.NewGuid():N}";
            BizSimGamesLogger.Info($"[Mock] Server-side access: {mockAuthCode}");
            return mockAuthCode;
        }

        public async Task<GamesAuthResponse> RequestServerSideAccessWithScopesAsync(
            string serverClientId,
            bool forceRefresh,
            List<GamesAuthScope> scopes,
            CancellationToken cancellationToken = default)
        {
            BizSimGamesLogger.Info($"[Mock] RequestServerSideAccessWithScopesAsync called (serverClientId={serverClientId}, scopes={scopes?.Count ?? 0})");

            if (!_isAuthenticated)
            {
                throw new GamesAuthException(new GamesAuthError
                {
                    errorCode = GamesErrorCodes.NotAuthenticated,
                    errorMessage = "Not authenticated - call AuthenticateAsync first",
                    isRetryable = false
                });
            }

            await Task.Delay(200, cancellationToken);

            string mockAuthCode = $"mock_auth_code_{Guid.NewGuid():N}";

            if (scopes == null || scopes.Count == 0)
            {
                BizSimGamesLogger.Info("[Mock] No scopes requested — returning auth code only");
                return new GamesAuthResponse(mockAuthCode, new List<GamesAuthScope>());
            }

            if (!_mock.mockConsentGranted)
            {
                BizSimGamesLogger.Info("[Mock] Consent DECLINED — returning auth code with empty scopes, no claims");
                return new GamesAuthResponse(mockAuthCode, new List<GamesAuthScope>());
            }

            var grantedScopes = new List<GamesAuthScope>(scopes);

            bool hasEmail = grantedScopes.Contains(GamesAuthScope.Email);
            bool hasProfile = grantedScopes.Contains(GamesAuthScope.Profile);

            var claims = new GamesIdTokenClaims(
                sub: _mock.mockPlayerId,
                email: hasEmail ? _mock.mockEmail : null,
                emailVerified: hasEmail && _mock.mockEmailVerified,
                name: hasProfile ? _mock.mockFullName : null,
                givenName: hasProfile ? _mock.mockGivenName : null,
                familyName: hasProfile ? _mock.mockFamilyName : null,
                picture: hasProfile ? _mock.mockPictureUrl : null,
                locale: hasProfile ? _mock.mockLocale : null
            );

            var response = new GamesAuthResponse(mockAuthCode, grantedScopes, claims);

            BizSimGamesLogger.Info($"[Mock] Scoped access granted: {grantedScopes.Count} scopes, claims={claims}");
            return response;
        }
    }
}
