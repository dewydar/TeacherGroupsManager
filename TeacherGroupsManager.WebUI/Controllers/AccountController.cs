using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

public class AccountController(IAuthService authService) : Controller
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
            ViewBag.Error = result.Errors.FirstOrDefault() ?? "تعذر تسجيل الدخول";
            return View(dto);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data.Id.ToString()),
            new(ClaimTypes.Name, result.Data.FullName),
            new(ClaimTypes.Role, result.Data.RoleName)
        };
        claims.AddRange(result.Data.Permissions.Select(x => new Claim("Permission", x)));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { IsPersistent = dto.RememberMe });
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
}
