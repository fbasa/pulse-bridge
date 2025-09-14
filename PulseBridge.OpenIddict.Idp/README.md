
**Migrations (run once):**

```bash
dotnet tool install --global dotnet-ef

cd src/AuthServer
dotnet ef migrations add Init_Auth -o Migrations
dotnet ef database update
```

---

# Payment.Api (resource server)

## `Payment.Api.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
  </ItemGroup>
</Project>
```

## `Auth/AuthorizationExtensions.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddScopePolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("payments.read",  p => p.RequireClaim("scope", "payments.read", "payments.write"));
            options.AddPolicy("payments.write", p => p.RequireClaim("scope", "payments.write"));
        });
        return services;
    }
}
```

## `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

var authority = builder.Configuration["Auth:Authority"] ?? "https://localhost:5001";
const string audience = "payments-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = audience
        };
        options.RequireHttpsMetadata = true;
    });

builder.Services.AddScopePolicies();
builder.Services.AddControllers();

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## `Controllers/PaymentsController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "payments.read")]
    public IActionResult GetPayments() => Ok(new[] { new { id = 1, amount = 500 } });

    [HttpPost]
    [Authorize(Policy = "payments.write")]
    public IActionResult CreatePayment([FromBody] object payload) => Created(string.Empty, new { ok = true });
}
```

`appsettings.json`

```json
{ "Auth": { "Authority": "https://localhost:5001" } }
```

---

# Accounting.Api (resource server)

Same as Payment.Api, but **audience** and policies are for accounting:

## `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

var authority = builder.Configuration["Auth:Authority"] ?? "https://localhost:5001";
const string audience = "accounting-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = audience
        };
        options.RequireHttpsMetadata = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("accounting.read",  p => p.RequireClaim("scope", "accounting.read", "accounting.write"));
    options.AddPolicy("accounting.write", p => p.RequireClaim("scope", "accounting.write"));
});

builder.Services.AddControllers();

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## `Controllers/AccountingController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/accounting")]
public class AccountingController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "accounting.read")]
    public IActionResult Get() => Ok(new[] { new { entryId = 1, debit = 500, credit = 0 } });

    [HttpPost]
    [Authorize(Policy = "accounting.write")]
    public IActionResult Post([FromBody] object payload) => Created(string.Empty, new { ok = true });
}
```

`appsettings.json`

```json
{ "Auth": { "Authority": "https://localhost:5001" } }
```

---

# Quick test

**Run the IdP**

```bash
cd src/AuthServer
dotnet ef database update
dotnet run
```

**Run Payment.Api & Accounting.Api**

```bash
cd ../Payment.Api && dotnet run
cd ../Accounting.Api && dotnet run
```

**Client credentials (machine-to-machine)**

```bash
# Get token for payments.write
TOKEN=$(curl -sk -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=jobs-worker&client_secret=super-secret-worker&grant_type=client_credentials&scope=payments.write" \
  | jq -r .access_token)

# Call the Payments API
curl -sk -H "Authorization: Bearer $TOKEN" https://localhost:5003/api/payments
```

**SPA (Angular)**

* Use **Authorization Code + PKCE** to `https://localhost:5001/connect/authorize` requesting scopes like
  `openid profile payments.read accounting.read`.
* The IdP will mint JWTs with `aud` set to `payments-api`/`accounting-api` depending on scopes.

---

# Hardening notes (prod)

* Replace dev certs with a real **X.509 signing cert** (Key Vault/HSM); keep **issuer** stable.
* Enforce **HTTPS**, strict **CORS**, strong password/MFA, and **short access token TTLs** with refresh tokens.
* Optionally switch to **introspection** (reference tokens) for revocation at the cost of a network hop.
* Add **OpenTelemetry** + Serilog enrichers (correlation id) and rate limiting on the APIs.
