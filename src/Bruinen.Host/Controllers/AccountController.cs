using Bruinen.Application.Abstractions;
using Bruinen.Application.Services;
using Bruinen.Host.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bruinen.Host.Controllers;

[Authorize]
public class AccountController(
    IUserRepository userRepository, 
    AccountService accountService,
    ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var login = User.GetLogin();
        logger.LogInformation("Account Index page requested by user: {Username}", login);
        
        var user = await userRepository.GetByLoginAsync(login);
        if (user == null)
        {
            logger.LogWarning("User {Username} not found in repository", login);
            return NotFound();
        }
        
        logger.LogInformation("Account Index page loaded successfully for user: {Username}", login);
        return View(user.ToViewModel());
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        logger.LogInformation("Change password page requested by user: {Username}", User.GetLogin());
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var login = User.GetLogin();
        logger.LogInformation("Password change attempt by user: {Username}", login);
        
        if (await accountService.ChangePassword(login, model.CurrentPassword, model.NewPassword))
        {
            logger.LogInformation("Password changed successfully for user: {Username}", login);
            return RedirectToAction(nameof(PasswordChangeSuccess));
        }
        
        logger.LogWarning("Password change failed for user: {Username}", login);
        ModelState.AddModelError(string.Empty, "Could not change password. Please check your current password and try again.");
        return View(model);
    }

    [HttpGet]
    public IActionResult PasswordChangeSuccess()
    {
        logger.LogInformation("Password change success page displayed for user: {Username}", User.GetLogin());
        return View();
    }
}