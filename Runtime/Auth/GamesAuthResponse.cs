// Copyright (c) BizSim Game Studios. All rights reserved.

using System.Collections.Generic;

namespace BizSim.GPlay.Games
{
    public class GamesAuthResponse
    {
        public string AuthCode { get; }
        public List<GamesAuthScope> GrantedScopes { get; }
        public GamesIdTokenClaims IdTokenClaims { get; }

        public GamesAuthResponse(string authCode, List<GamesAuthScope> grantedScopes, GamesIdTokenClaims idTokenClaims = null)
        {
            AuthCode = authCode;
            GrantedScopes = grantedScopes ?? new List<GamesAuthScope>();
            IdTokenClaims = idTokenClaims;
        }
    }
}
