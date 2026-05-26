using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.WebUI.Infrastructure;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.EmployeesManage)]
public class EmployeesController(IEmployeeService employeeService, IRoleService roleService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetData(CancellationToken cancellationToken)
    {
        var request = DataTablesRequestHelper.Parse(Request);
        var result = await employeeService.GetPagedAsync(request, cancellationToken);
        return Json(result);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
        return View(new CreateEmployeeDto(string.Empty, string.Empty, null, string.Empty, string.Empty, 0));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        if (dto.RoleId <= 0)
        {
            ModelState.AddModelError(string.Empty, "اختر الدور");
            ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        var result = await employeeService.CreateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join("، ", result.Errors));
        ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var employee = await employeeService.GetByIdAsync(id, cancellationToken);
        if (employee is null) return NotFound();
        ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
        return View(new EditEmployeeDto(employee.Id, employee.FullName, employee.Mobile, employee.Email, employee.Username, null, employee.RoleId, employee.IsActive));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditEmployeeDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        if (dto.RoleId <= 0)
        {
            ModelState.AddModelError(string.Empty, "اختر الدور");
            ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        var result = await employeeService.UpdateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join("، ", result.Errors));
        ViewBag.Roles = await roleService.GetAllAsync(cancellationToken);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await employeeService.DeleteAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        return RedirectToAction(nameof(Index));
    }
}
