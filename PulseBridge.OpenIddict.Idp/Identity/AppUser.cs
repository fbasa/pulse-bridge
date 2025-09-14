using Microsoft.AspNetCore.Identity;

namespace PulseBridge.OpenIddict.Idp.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public Guid? TenantId { get; set; }
}
