using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.RolesManage)]
public class RolesController(IRoleService roleService, IPermissionService permissionService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Permissions = await permissionService.GetAllAsync(cancellationToken);
        return View(await roleService.GetAllAsync(cancellationToken));
    }

    public IActionResult Create() => View(new RoleDto(0, string.Empty, string.Empty, true));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleDto dto, CancellationToken cancellationToken)
    {
        var result = await roleService.CreateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return result.Succeeded ? RedirectToAction(nameof(Index)) : View(dto);
    }
}
