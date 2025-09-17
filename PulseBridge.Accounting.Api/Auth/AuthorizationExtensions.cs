namespace PulseBridge.Accounting.Api.Auth;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddScopePolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            //// Duende: way to check claims
            //options.AddPolicy("accounting.read", p => p.RequireClaim("scope", "accounting.read", "accounting.write"));
            //options.AddPolicy("accounting.write", p => p.RequireClaim("scope", "accounting.write"));


            // OpenIddict: way to check scopes
            options.AddPolicy("accounting.read", p => p.RequireScope("accounting.read", "accounting.write"));
            options.AddPolicy("accounting.write", p => p.RequireScope("accounting.write"));
        });
        return services;
    }
}