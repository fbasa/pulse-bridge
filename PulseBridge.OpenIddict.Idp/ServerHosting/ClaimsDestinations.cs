using OpenIddict.Abstractions;
using System.Security.Claims;

namespace PulseBridge.OpenIddict.Idp.ServerHosting;

public static class ClaimsDestinations
{
    public static IEnumerable<string> For(ClaimsPrincipal subject, Claim claim) => claim.Type switch
    {
        // Include basic profile claims only when 'profile' is granted
        ClaimTypes.Name when subject.HasScope(OpenIddictConstants.Scopes.Profile)
            => new[] { OpenIddictConstants.Destinations.IdentityToken, OpenIddictConstants.Destinations.AccessToken },

        ClaimTypes.Email when subject.HasScope(OpenIddictConstants.Scopes.Email)
            => new[] { OpenIddictConstants.Destinations.IdentityToken, OpenIddictConstants.Destinations.AccessToken },

        // default -> access token
        _ => new[] { OpenIddictConstants.Destinations.AccessToken }
    };
}