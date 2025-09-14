using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PulseBridge.OpenIddict.Idp.Identity;

namespace PulseBridge.OpenIddict.Idp.Controllers;

// For OpenIddict, you can just validate returnUrl is local or starts with /connect/authorize

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signIn;
    private readonly UserManager<AppUser> _users;

    public AccountController(SignInManager<AppUser> s, UserManager<AppUser> u) => (_signIn, _users) = (s, u);

    [HttpGet("/Account/Login")]
    public IActionResult Login(string returnUrl) => View(new LoginVm { ReturnUrl = returnUrl });

    [ValidateAntiForgeryToken]
    [HttpPost("/Account/Login")]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.FindByNameAsync(vm.UserName) ?? await _users.FindByEmailAsync(vm.UserName);
        if (user is null) 
        { 
            ModelState.AddModelError("", "Invalid credentials"); 
            return View(vm); 
        }

        var result = await _signIn.PasswordSignInAsync(user, vm.Password, vm.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded) 
        { 
            ModelState.AddModelError("", "Invalid credentials"); 
            return View(vm); 
        }

        // Safely allow return to the authorize endpoint or local URLs
        if (!string.IsNullOrEmpty(vm.ReturnUrl) &&
            (vm.ReturnUrl.StartsWith("/connect/authorize") || Url.IsLocalUrl(vm.ReturnUrl)))
            return Redirect(vm.ReturnUrl);

        return Redirect("~/");
    }
}

public record LoginVm
{
    public string? ReturnUrl { get; init; }
    public string UserName { get; init; } = "";
    public string Password { get; init; } = "";
    public bool RememberMe { get; init; }
}
