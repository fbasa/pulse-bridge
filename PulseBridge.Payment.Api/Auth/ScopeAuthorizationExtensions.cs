using Microsoft.AspNetCore.Authorization;

namespace PulseBridge.Payment.Api.Auth;

public static class ScopeAuthorizationExtensions
{
    public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder builder, params string[] required)
    => builder.RequireAssertion(ctx =>
    {
        var values = ctx.User.FindAll("scope")
            .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToHashSet(StringComparer.Ordinal);

        // Fallback for providers that use "scp"
        if (values.Count == 0)
            values = ctx.User.FindAll("scp")
                .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .ToHashSet(StringComparer.Ordinal);

        return required.Any(r => values.Contains(r));
    });
}
