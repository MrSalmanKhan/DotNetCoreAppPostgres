using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    // GET: /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = "/")
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // GET: /Account/ExternalLogin?provider=Google&returnUrl=...
    // Starts the external login challenge (Google)
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider = GoogleDefaults.AuthenticationScheme, string returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl // after Google signs in, user will be redirected here
        };

        return Challenge(props, provider);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // sign out the cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // optionally redirect to a logged-out page
        return RedirectToAction(nameof(LoggedOut));
    }

    // GET: /Account/LoggedOut
    [HttpGet]
    [AllowAnonymous]
    public IActionResult LoggedOut()
    {
        return View();
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return View();
    }
}
