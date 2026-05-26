using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.WebUI.Infrastructure;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.StudentsManage)]
public class StudentsController(IStudentService service, IAcademicYearService academicYearService, IGroupService groupService, IStringLocalizer<SharedResource> localizer) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetData(CancellationToken cancellationToken)
    {
        var request = DataTablesRequestHelper.Parse(Request);
        var result = await service.GetPagedAsync(request, cancellationToken);
        return Json(result);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken, includeAllGroupsWhenNoYear: false);
        return View(new CreateStudentDto(string.Empty, string.Empty, null, 0, 0, null));
    }

    public async Task<IActionResult> GroupsByAcademicYear(int academicYearId, CancellationToken cancellationToken)
    {
        if (academicYearId <= 0)
        {
            return Json(Array.Empty<object>());
        }

        var groups = await groupService.GetByAcademicYearAsync(academicYearId, cancellationToken);
        return Json(groups.Select(x => new { id = x.Id, name = x.Name }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillLookups(cancellationToken, dto.AcademicYearId, includeAllGroupsWhenNoYear: false);
            return View(dto);
        }
        if (dto.AcademicYearId <= 0) ModelState.AddModelError(string.Empty, localizer["RequiredAcademicYear"]);
        if (dto.GroupId <= 0) ModelState.AddModelError(string.Empty, localizer["RequiredGroup"]);
        if (!ModelState.IsValid)
        {
            await FillLookups(cancellationToken, dto.AcademicYearId, includeAllGroupsWhenNoYear: false);
            return View(dto);
        }
        var result = await service.CreateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors));
        await FillLookups(cancellationToken, dto.AcademicYearId, includeAllGroupsWhenNoYear: false);
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var student = await service.GetByIdAsync(id, cancellationToken);
        if (student is null) return NotFound();
        await FillLookups(cancellationToken, student.AcademicYearId);
        return View(new EditStudentDto(student.Id, student.FullName, student.Mobile, student.ParentMobile, student.AcademicYearId, student.GroupId, student.Notes, student.IsActive));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditStudentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillLookups(cancellationToken, dto.AcademicYearId);
            return View(dto);
        }
        if (dto.AcademicYearId <= 0) ModelState.AddModelError(string.Empty, localizer["RequiredAcademicYear"]);
        if (dto.GroupId <= 0) ModelState.AddModelError(string.Empty, localizer["RequiredGroup"]);
        if (!ModelState.IsValid)
        {
            await FillLookups(cancellationToken, dto.AcademicYearId);
            return View(dto);
        }
        var result = await service.UpdateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors));
        await FillLookups(cancellationToken, dto.AcademicYearId);
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        return RedirectToAction(nameof(Index));
    }

    private async Task FillLookups(CancellationToken cancellationToken, int? academicYearId = null, bool includeAllGroupsWhenNoYear = true)
    {
        ViewBag.AcademicYears = await academicYearService.GetAllAsync(cancellationToken);
        ViewBag.Groups = academicYearId is > 0
            ? await groupService.GetByAcademicYearAsync(academicYearId.Value, cancellationToken)
            : includeAllGroupsWhenNoYear ? await groupService.GetAllAsync(cancellationToken) : Array.Empty<GroupDto>();
    }
}


