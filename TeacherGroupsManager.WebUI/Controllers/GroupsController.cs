using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.WebUI.Infrastructure;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.GroupsManage)]
public class GroupsController(IGroupService service, IAcademicYearService academicYearService, IStringLocalizer<SharedResource> localizer) : Controller
{
    public async Task<IActionResult> Index(GroupType? type, CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken);
        ViewBag.GroupType = type;
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

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken) => View(await service.GetByIdAsync(id, cancellationToken));

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken);
        var schedules = new List<GroupScheduleDto> { new(DayOfWeek.Saturday, new TimeOnly(18, 0), new TimeOnly(20, 0)) };
        return View(new CreateGroupDto(string.Empty, 0, GroupType.Public, DayOfWeek.Saturday, new TimeOnly(18, 0), new TimeOnly(20, 0), 0, null, true, schedules));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGroupDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillLookups(cancellationToken);
            return View(dto);
        }
        if (dto.AcademicYearId <= 0)
        {
            ModelState.AddModelError(string.Empty, localizer["RequiredAcademicYear"]);
            await FillLookups(cancellationToken);
            return View(dto);
        }
        var result = await service.CreateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors));
        await FillLookups(cancellationToken);
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var group = await service.GetByIdAsync(id, cancellationToken);
        if (group is null) return NotFound();
        await FillLookups(cancellationToken);
        return View(new EditGroupDto(group.Id, group.Name, group.AcademicYearId, group.GroupType, group.DayOfWeek, group.StartTime, group.EndTime, group.DefaultLessonPrice, group.MonthlyPrice, group.IsActive, group.Schedules?.ToList()));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditGroupDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillLookups(cancellationToken);
            return View(dto);
        }
        if (dto.AcademicYearId <= 0)
        {
            ModelState.AddModelError(string.Empty, localizer["RequiredAcademicYear"]);
            await FillLookups(cancellationToken);
            return View(dto);
        }
        var result = await service.UpdateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors));
        await FillLookups(cancellationToken);
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        return RedirectToAction(nameof(Index));
    }

    private async Task FillLookups(CancellationToken cancellationToken)
    {
        ViewBag.AcademicYears = await academicYearService.GetAllAsync(cancellationToken);
    }
}


