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

    private async Task FillLookups(CancellationToken cancellationToken)
    {
        ViewBag.AcademicYears = await academicYearService.GetAllAsync(cancellationToken);
        ViewBag.Employees = await employeeService.GetAllAsync(cancellationToken);
    }
}
