using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Roles = AppConstants.AdminRole)]
public class SettingsController(ITestDataSeeder testDataSeeder) : Controller
{
    public IActionResult TestData() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateTestData(CancellationToken cancellationToken)
    {
        var summary = await testDataSeeder.SeedAsync(cancellationToken);
        return View("TestData", summary);
    }
}
