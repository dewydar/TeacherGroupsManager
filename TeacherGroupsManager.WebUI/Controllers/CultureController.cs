using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace TeacherGroupsManager.WebUI.Controllers;

public class CultureController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ar-EG", "en-US", "fr-FR" };
        culture = supported.Contains(culture) ? culture : "ar-EG";
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}

