using OpenIddict.Abstractions;
using PulseBridge.OpenIddict.Idp.Identity;

namespace PulseBridge.OpenIddict.Idp.ServerHosting;

public static class OAuthServerHosting
{
    public static IServiceCollection AddConfiguredOpenIddict(this IServiceCollection services, IConfiguration cfg)
    {
        var issuer = cfg["Auth:Issuer"] ?? "https://idp.localtest.me"; // e.g., https://localhost:7210

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<AppDbContext>();
            })
            .AddServer(options =>
            {
                if (!string.IsNullOrWhiteSpace(issuer))
                    options.SetIssuer(new Uri(issuer));

                options.SetAuthorizationEndpointUris("/connect/authorize")
                       .SetTokenEndpointUris("/connect/token")
                       .SetUserInfoEndpointUris("/connect/userinfo")
                       .SetEndSessionEndpointUris("/connect/logout");

                // Flows
                options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();     // PKCE for public clients
                options.AllowClientCredentialsFlow();
                options.AllowRefreshTokenFlow();

                // Scopes (Duende IdentityResources -> OpenIddict scopes)
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Roles,
                    "payments.read", "payments.write",
                    "accounting.read", "accounting.write"
                );

                // Issue JWT access tokens that resource APIs can validate via JwtBearer
                options.DisableAccessTokenEncryption();
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserInfoEndpointPassthrough()
                       .EnableEndSessionEndpointPassthrough();

                
            });

        return services;
    }
}