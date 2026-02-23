using Bruinen.Application.Abstractions;
using Bruinen.Application.Services;
using Bruinen.Host.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bruinen.Host.Controllers;

[Authorize]
public class AccountController(IUserRepository userRepository, AccountService accountService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await userRepository.GetByLoginAsync(User.GetLogin());
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
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (await accountService.ChangePassword(User.GetLogin(), model.CurrentPassword, model.NewPassword))
            return RedirectToAction(nameof(PasswordChangeSuccess));
        ModelState.AddModelError(string.Empty, "Could not change password. Please check your current password and try again.");
        return View(model);
    }

    [HttpGet]
    public IActionResult PasswordChangeSuccess()
    {
        return View();
    }
}