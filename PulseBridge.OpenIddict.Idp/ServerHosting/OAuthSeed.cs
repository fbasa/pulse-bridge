using OpenIddict.Abstractions;

namespace PulseBridge.OpenIddict.Idp.ServerHosting;

public class OAuthSeed : IHostedService
{
    private readonly IServiceProvider _sp;
    public OAuthSeed(IServiceProvider sp) => _sp = sp;

    public async Task StartAsync(CancellationToken _)
    {
        using var scope = _sp.CreateScope();
        var apps = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopes = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        // SPA (Authorization Code + PKCE)
        if (await apps.FindByClientIdAsync("angular-spa") is null)
        {
            await apps.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "angular-spa",
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                DisplayName = "Angular Web App",
                RedirectUris = { new Uri("https://ui.localtest.me/auth/callback") },
                PostLogoutRedirectUris = { new Uri("https://ui.localtest.me/") },
                Permissions =
                {
                    // endpoints + flows
                    OpenIddictConstants.Permissions.Endpoints.Authorization, // to get auth code
                    OpenIddictConstants.Permissions.Endpoints.Token, // to get tokens
                    OpenIddictConstants.Permissions.Endpoints.EndSession, // logout
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode, // auth code flow
                    OpenIddictConstants.Permissions.ResponseTypes.Code, // response_type=code
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,    // allow refresh tokens

                    // scopes the SPA may request
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Scopes.OfflineAccess, // for refresh tokens
                    OpenIddictConstants.Permissions.Prefixes.Scope + "payments.read",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "accounting.read"
                },
                Requirements = { OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange }
            });
        }

        if (await apps.FindByClientIdAsync("mvc-client") is null)
        {
            await apps.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "mvc-client",
                ClientSecret = "super-secret-mvc",
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                DisplayName = "MVC Web App",
                RedirectUris = { new Uri("https://localhost:7141/signin-oidc") },
                PostLogoutRedirectUris = { new Uri("https://localhost:7141/signout-callback-oidc") },
                Permissions =
                {
                    // endpoints + flows
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,    // allow refresh tokens
                    
                    // scopes the SPA may request
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Scopes.OfflineAccess, // for refresh tokens
                    OpenIddictConstants.Permissions.Prefixes.Scope + "payments.read",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "accounting.read"
                },
                Requirements = { OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange }
            });
        }

        // M2M worker (client credentials)
        if (await apps.FindByClientIdAsync("jobs-worker") is null)
        {
            await apps.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "jobs-worker",
                ClientSecret = "super-secret-worker",
                DisplayName = "Background Jobs Worker",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "payments.write",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "accounting.write"
                }
            });
        }

        // (Optional) create named scopes/resources to appear in discovery
        if (await scopes.FindByNameAsync("payments.read") is null)
        {
            await scopes.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "payments.read",
                DisplayName = "Read payments",
                Resources = { "payments-api" }
            });
        }
        if (await scopes.FindByNameAsync("payments.write") is null)
        {
            await scopes.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "payments.write",
                DisplayName = "Write payments",
                Resources = { "payments-api" }
            });
        }
        if (await scopes.FindByNameAsync("accounting.read") is null)
        {
            await scopes.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "accounting.read",
                DisplayName = "Read accounting",
                Resources = { "accounting-api" }
            });
        }
        if (await scopes.FindByNameAsync("accounting.write") is null)
        {
            await scopes.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "accounting.write",
                DisplayName = "Write accounting",
                Resources = { "accounting-api" }
            });
        }
    }

    public Task StopAsync(CancellationToken _) => Task.CompletedTask;
}