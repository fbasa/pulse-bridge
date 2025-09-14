using Microsoft.AspNetCore.Identity;

namespace PulseBridge.OpenIddict.Idp.Identity;

public static class IdentityBootstrap
{
    public static async Task EnsureDefaultAdminAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        const string adminRole = "admin";
        if (!await roleMgr.RoleExistsAsync(adminRole))
            await roleMgr.CreateAsync(new AppRole { Name = adminRole });

        const string adminEmail = "admin@local.test";
        var user = await userMgr.FindByEmailAsync(adminEmail);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = "System Admin"
            };
            await userMgr.CreateAsync(user, "Change_this_devP@ss1!");
            await userMgr.AddToRoleAsync(user, adminRole);
        }
    }
}