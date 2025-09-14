using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PulseBridge.OpenIddict.Idp.Identity;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Identity tweaks
        b.Entity<AppUser>(u =>
        {
            u.Property(x => x.DisplayName).HasMaxLength(128);
            u.HasIndex(x => x.TenantId);
        });

        // OpenIddict stores (applications, authorizations, scopes, tokens)
        b.UseOpenIddict();
    }
}