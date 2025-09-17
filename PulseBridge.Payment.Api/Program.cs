using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PulseBridge.Payment.Api.Auth;
using Serilog;

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

var authAuthority = builder.Configuration["Auth:Issuer"];
var audience = "payments-api";

// AuthN
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authAuthority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = audience
        };
        options.RequireHttpsMetadata = true;
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        // Optional: map "scope" => ClaimTypes.Role etc. Keep "scope" as-is for policy checks
    });

builder.Services.AddScopePolicies();
builder.Services.AddControllers();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("spa", p => p
        .WithOrigins("https://ui.localtest.me")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health/ready", () => Results.Ok("Payment-api up"));

logger.Information("Payment-API up and running!");

app.Run();

