﻿using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace timer_app.Middleware
{
    public class TokenValidator : ITokenValidator
    {
        private readonly IConfiguration _configuration;

        public TokenValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ClaimsPrincipal> ValidateIdToken(string idToken)
        {
            var issuer = Environment.GetEnvironmentVariable("Auth0_Domain")
                ?? _configuration.GetValue<string>("Auth0_Domain");

            var audience = Environment.GetEnvironmentVariable("Auth0_Audience_IdToken")
                ?? _configuration.GetValue<string>("Auth0_Audience_IdToken");

            // Ensure issuer ends with a slash
            if (!issuer.EndsWith("/")) issuer += "/";

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
           $"{issuer}.well-known/openid-configuration",
           new OpenIdConnectConfigurationRetriever(),
           new HttpDocumentRetriever());

            var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

            // Validate the token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openIdConfig.SigningKeys,

                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(idToken, validationParameters, out var validatedToken);
            return principal;
        }

    }
}
