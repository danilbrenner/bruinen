using System.Security.Claims;
using System.Security.Principal;

namespace Bruinen.Host.Controllers;

public static class ControllersExtensions
{
    public static string GetLogin(this ClaimsPrincipal principal)
        => principal.Identity?.Name ?? throw new InvalidOperationException("User is not authenticated");
}