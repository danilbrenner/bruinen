using System.Security.Claims;
using Bruinen.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bruinen.Host.Models;

namespace Bruinen.Host.Controllers;

public class AuthController(
    ILogger<AuthController> logger,
    LoginService loginService, 
    IConfiguration configuration) : Controller
{
    [HttpGet]
    public IActionResult Login(string? rd = null)
    {
        logger.LogInformation("GET Login page requested with redirect: {RedirectUrl}", rd ?? "none");
        ViewData["rd"] = rd;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? rd = null)
    {
        logger.LogInformation("POST Login attempt for username: {Username}, redirect: {RedirectUrl}", 
            model.Username, rd ?? "none");
        ViewData["rd"] = rd;

        if (!ModelState.IsValid)
        {
            logger.LogWarning("Login failed for {Username}: Invalid model state", model.Username);
            return View(model);
        }

        if (!await loginService.LoginAsync(model.Username, model.Password))
        {
            logger.LogWarning("Login failed for {Username}: Invalid credentials", model.Username);
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

        logger.LogInformation("User {Username} logged in successfully with RememberMe: {RememberMe}", 
            model.Username, model.RememberMe);

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
        var username = HttpContext.User.Identity?.Name ?? "Unknown";
        logger.LogInformation("User {Username} logging out", username);
        
        await HttpContext.SignOutAsync("CookieAuth");
        
        logger.LogInformation("User {Username} logged out successfully", username);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Verify()
    {
        logger.LogInformation("Verify endpoint called, IsAuthenticated: {IsAuthenticated}", 
            HttpContext.User.Identity?.IsAuthenticated ?? false);
        
        if (!HttpContext.User.Identity?.IsAuthenticated ?? false)
        {
            var returnUrl = GetOriginUrl(HttpContext.Request);
            var baseUrl = 
                configuration["Authentication:LoginUrl"] 
                ?? throw new InvalidOperationException("Authentication:LoginUrl configuration is missing");
            
            logger.LogInformation("User not authenticated, redirecting to login with return URL: {ReturnUrl}", returnUrl);
            return Redirect($"{baseUrl}?rd={Uri.EscapeDataString(returnUrl)}");
        }

        var username = HttpContext.User.Identity!.Name;
        logger.LogInformation("User {Username} verified successfully", username);
        
        HttpContext.Response.Headers.Append("X-Auth-User", username);
        // TODO: HttpContext.Response.Headers.Append("X-Auth-Roles", "");

        return Ok();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        logger.LogWarning("Access denied page accessed by user: {Username}", 
            HttpContext.User.Identity?.Name ?? "Anonymous");
        return View();
    }

    private static string GetOriginUrl(HttpRequest request)
    {
        var protocol = request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? request.Protocol;
        var host = request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? request.Host.Value;
        var path = request.Headers["X-Forwarded-Uri"].FirstOrDefault() ?? request.Path.Value;
        var returnUrl = $"{protocol}://{host}{path}";
        return returnUrl;
    }
}