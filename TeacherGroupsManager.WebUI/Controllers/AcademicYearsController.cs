using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.AcademicYearsManage)]
public class AcademicYearsController(IAcademicYearService service) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await service.GetAllAsync(cancellationToken));

    public IActionResult Create() => View(new CreateAcademicYearDto(string.Empty, DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddMonths(9))));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAcademicYearDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await service.CreateAsync(dto, cancellationToken);
        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var year = await service.GetByIdAsync(id, cancellationToken);
        return year is null ? NotFound() : View(new EditAcademicYearDto(year.Id, year.Name, year.StartDate, year.EndDate, year.IsActive));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditAcademicYearDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await service.UpdateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        return result.Succeeded ? RedirectToAction(nameof(Index)) : View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        return RedirectToAction(nameof(Index));
    }
}
