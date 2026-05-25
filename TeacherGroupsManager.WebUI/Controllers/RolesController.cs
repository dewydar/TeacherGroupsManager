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
        if (!result.Succeeded) ModelState.AddModelError(string.Empty, string.Join("، ", result.Errors));
        return result.Succeeded ? RedirectToAction(nameof(Index)) : View(dto);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var role = await roleService.GetByIdAsync(id, cancellationToken);
        return role is null ? NotFound() : View(role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleDto dto, CancellationToken cancellationToken)
    {
        var result = await roleService.UpdateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        if (!result.Succeeded) ModelState.AddModelError(string.Empty, string.Join("، ", result.Errors));
        return result.Succeeded ? RedirectToAction(nameof(Index)) : View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await roleService.DeleteAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppConstants.AdminRole)]
    public async Task<IActionResult> Permissions(int id, CancellationToken cancellationToken)
    {
        var rolePermissions = await roleService.GetPermissionsAsync(id, cancellationToken);
        if (rolePermissions is null) return NotFound();
        ViewBag.Permissions = await permissionService.GetAllAsync(cancellationToken);
        return View(rolePermissions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppConstants.AdminRole)]
    public async Task<IActionResult> Permissions(int roleId, int[] permissionIds, CancellationToken cancellationToken)
    {
        var result = await roleService.UpdatePermissionsAsync(roleId, permissionIds, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        return RedirectToAction(nameof(Index));
    }
}
