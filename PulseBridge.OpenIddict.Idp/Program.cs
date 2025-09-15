
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using PulseBridge.OpenIddict.Idp.Identity;
using PulseBridge.OpenIddict.Idp.ServerHosting;
using Serilog;

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

var conn = builder.Configuration.GetConnectionString("IDP_DB");

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(conn, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequiredLength = 8;
    opt.SignIn.RequireConfirmedEmail = false; // true in prod when email sender is configured
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(o =>
{
    // Make Identity emit OIDC-standard names
    o.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject; // "sub"
    o.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;    // "name"
    o.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;    // "role"
});

//logger.Information("Configuring OpenId Identity ");

// AddOpenIddict() registration (IdP)
builder.Services.AddConfiguredOpenIddict(builder.Configuration);

// builder.Services.Configure<OpenIddictServerAspNetCoreOptions>(opt =>
// {
// #if DEBUG
//     opt.DisableTransportSecurityRequirement = true; // DEV ONLY
// #endif
// });

//logger.Information("Done Configuring OpenId Identity ");

builder.Services.AddHostedService<OAuthSeed>();

builder.Services.AddControllersWithViews();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("spa", p => p
        .WithOrigins("http://localhost:4200", "https://ui.localtest.me")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    o.KnownNetworks.Clear(); o.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("spa");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// DB + admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
await app.Services.EnsureDefaultAdminAsync();

app.MapGet("/", () => Results.Ok("IDP up"));

logger.Information("IDP up and running!");

app.Run();
