using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Localization;

namespace TeacherGroupsManager.WebUI.Controllers;

public class AccountController(IAuthService authService, IStringLocalizer<SharedResource> localizer) : Controller
{
    [AllowAnonymous]
    public IActionResult Login() => View(new LoginDto(string.Empty, string.Empty));

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(dto, cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            ViewBag.Error = result.Errors.FirstOrDefault() ?? localizer["FailureDefault"];
            return View(dto);
        }

        if (result.Data.RequiresPasswordSetup)
        {
            return RedirectToAction(nameof(ResetPassword), new { username = result.Data.Username, firstTime = true });
        }

        if (result.Data.Employee is null)
        {
            ViewBag.Error = localizer["FailureDefault"];
            return View(dto);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data.Employee.Id.ToString()),
            new(ClaimTypes.Name, result.Data.Employee.FullName),
            new(ClaimTypes.Role, result.Data.Employee.RoleName)
        };
        claims.AddRange(result.Data.Employee.Permissions.Select(x => new Claim("Permission", x)));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { IsPersistent = dto.RememberMe });
        return RedirectToAction("Index", "Dashboard");
    }

    [AllowAnonymous]
    public IActionResult ResetPassword(string? username = null, bool firstTime = false)
    {
        if (!firstTime && User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordDto(username, null, string.Empty, string.Empty, firstTime));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        if (!dto.IsFirstTime && User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var currentEmployeeId = dto.IsFirstTime ? null : GetCurrentEmployeeId();
        var result = await authService.ResetPasswordAsync(dto, currentEmployeeId, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors));
            return View(dto);
        }

        TempData["Success"] = result.Message;
        if (dto.IsFirstTime)
        {
            return RedirectToAction(nameof(Login));
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    private int? GetCurrentEmployeeId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var employeeId) ? employeeId : null;
}

