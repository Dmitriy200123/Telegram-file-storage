﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuth
{
    /// <inheritdoc />
    public class TokenRefresher : ITokenRefresher
    {
        private readonly byte[] _key;
        private readonly IJwtAuthenticationManager _jWtAuthenticationManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="jWtAuthenticationManager"></param>
        public TokenRefresher(byte[] key, IJwtAuthenticationManager jWtAuthenticationManager)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _jWtAuthenticationManager = jWtAuthenticationManager ?? throw new ArgumentNullException(nameof(jWtAuthenticationManager));
        }

        /// <inheritdoc />
        public AuthenticationResponse Refresh(RefreshCred refreshCred)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(refreshCred.JwtToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
                }, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token passed!");
            }

            var userName = principal.Identity?.Name;
            if (userName == null)
                throw new InvalidOperationException("Doesn't contain name in Identity");

            if (refreshCred.RefreshToken != _jWtAuthenticationManager.UsersRefreshTokens[userName])
                throw new SecurityTokenException("Invalid token passed!");

            return _jWtAuthenticationManager.Authenticate(userName, principal.Claims.ToArray());
        }
    }
}