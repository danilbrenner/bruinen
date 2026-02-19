using Bruinen.Application.Abstractions;
using Bruinen.Host.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bruinen.Host.Controllers;

[Authorize]
public class AccountController(IUserRepository userRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await userRepository.GetByLoginAsync(User.Identity?.Name ?? string.Empty);
        if (user == null)
            return NotFound();
        return View(user.ToViewModel());
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        return View();
    }
}