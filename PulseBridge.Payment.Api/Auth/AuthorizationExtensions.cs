namespace PulseBridge.Payment.Api.Auth;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddScopePolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            //// Duende: way to check claims
            //options.AddPolicy("payments.read", p => p.RequireClaim("scope", "payments.read", "payments.write"));
            //options.AddPolicy("payments.write", p => p.RequireClaim("scope", "payments.write"));

            // OpenIddict: way to check scopes
            options.AddPolicy("payments.read", p => p.RequireScope("payments.read", "payments.write"));
            options.AddPolicy("payments.write", p => p.RequireScope("payments.write"));
        });
        return services;
    }
}