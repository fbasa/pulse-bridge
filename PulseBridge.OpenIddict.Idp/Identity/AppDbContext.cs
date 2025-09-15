using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PulseBridge.OpenIddict.Idp.Identity;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity tweaks
        builder.Entity<AppUser>(u =>
        {
            u.Property(x => x.DisplayName).HasMaxLength(128);
            u.HasIndex(x => x.TenantId);
        });

        // OpenIddict stores (applications, authorizations, scopes, tokens)
        builder.UseOpenIddict();
    }
}