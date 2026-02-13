// Copyright (c) BizSim Game Studios. All rights reserved.

namespace BizSim.GPlay.Games
{
    public class GamesIdTokenClaims
    {
        public string Sub { get; }
        public string Email { get; }
        public bool EmailVerified { get; }
        public string Name { get; }
        public string GivenName { get; }
        public string FamilyName { get; }
        public string Picture { get; }
        public string Locale { get; }

        public GamesIdTokenClaims(
            string sub,
            string email = null,
            bool emailVerified = false,
            string name = null,
            string givenName = null,
            string familyName = null,
            string picture = null,
            string locale = null)
        {
            Sub = sub;
            Email = email;
            EmailVerified = emailVerified;
            Name = name;
            GivenName = givenName;
            FamilyName = familyName;
            Picture = picture;
            Locale = locale;
        }

        public override string ToString()
        {
            return $"IdTokenClaims(sub={Sub}, email={Email ?? "(null)"}, name={Name ?? "(null)"})";
        }
    }
}
