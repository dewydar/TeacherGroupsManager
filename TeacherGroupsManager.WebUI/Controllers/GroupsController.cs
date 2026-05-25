using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.GroupsManage)]
public class GroupsController(IGroupService service, IAcademicYearService academicYearService, IEmployeeService employeeService) : Controller
{
    public async Task<IActionResult> Index(GroupType? type, CancellationToken cancellationToken)
    {
        var data = await service.GetAllAsync(cancellationToken);
        return View(type.HasValue ? data.Where(x => x.GroupType == type).ToList() : data);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken) => View(await service.GetByIdAsync(id, cancellationToken));

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken);
        return View(new CreateGroupDto(string.Empty, 0, GroupType.Public, null, null, DayOfWeek.Saturday, new TimeOnly(18, 0), new TimeOnly(20, 0), 0));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGroupDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillLookups(cancellationToken);
            return View(dto);
        }
        var result = await service.CreateAsync(dto, cancellationToken);
        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var group = await service.GetByIdAsync(id, cancellationToken);
        if (group is null) return NotFound();
        await FillLookups(cancellationToken);
        return View(new EditGroupDto(group.Id, group.Name, group.AcademicYearId, group.GroupType, group.TeacherId, group.AssistantTeacherId, group.DayOfWeek, group.StartTime, group.EndTime, group.DefaultLessonPrice, group.IsActive));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditGroupDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await FillLookups(cancellationToken);
            return View(dto);
        }
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

    private async Task FillLookups(CancellationToken cancellationToken)
    {
        ViewBag.AcademicYears = await academicYearService.GetAllAsync(cancellationToken);
        ViewBag.Employees = await employeeService.GetAllAsync(cancellationToken);
    }
}
