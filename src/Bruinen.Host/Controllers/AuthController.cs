using System.Security.Claims;
using Bruinen.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bruinen.Host.Models;

namespace Bruinen.Host.Controllers;

public class AuthController(LoginService loginService, IConfiguration configuration) : Controller
{
    [HttpGet]
    public IActionResult Login(string? rd = null)
    {
        ViewData["rd"] = rd;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? rd = null)
    {
        ViewData["rd"] = rd;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!await loginService.LoginAsync(model.Username, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
        };

        await HttpContext.SignInAsync("CookieAuth", claimsPrincipal, authProperties);

        return
            !string.IsNullOrEmpty(rd)
                ? Redirect(rd)
                : RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Verify()
    {
        if (!HttpContext.User.Identity?.IsAuthenticated ?? false)
        {
            var returnUrl = GetOriginUrl(HttpContext.Request);
            var baseUrl = 
                configuration["Authentication:LoginUrl"] 
                ?? throw new InvalidOperationException("Authentication:LoginUrl configuration is missing");
            return Redirect($"{baseUrl}?rd={Uri.EscapeDataString(returnUrl)}");
        }

        HttpContext.Response.Headers.Append("X-Auth-User", HttpContext.User.Identity!.Name);
        // TODO: HttpContext.Response.Headers.Append("X-Auth-Roles", "");

        return Ok();
    }

    private static string GetOriginUrl(HttpRequest request)
    {
        var protocol = request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? request.Protocol;
        var host = request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? request.Host.Value;
        var path = request.Headers["X-Forwarded-Uri"].FirstOrDefault() ?? request.Path.Value;
        var returnUrl = $"{protocol}://{host}{path}";
        return returnUrl;
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}