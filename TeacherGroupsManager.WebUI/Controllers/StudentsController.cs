using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.StudentsManage)]
public class StudentsController(IStudentService service, IAcademicYearService academicYearService, IGroupService groupService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await service.GetAllAsync(cancellationToken));

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken);
        return View(new CreateStudentDto(string.Empty, string.Empty, null, 0, 0, null));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStudentDto dto, CancellationToken cancellationToken)
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
        var student = await service.GetByIdAsync(id, cancellationToken);
        if (student is null) return NotFound();
        await FillLookups(cancellationToken);
        return View(new EditStudentDto(student.Id, student.FullName, student.Mobile, student.ParentMobile, student.AcademicYearId, student.GroupId, student.Notes, student.IsActive));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditStudentDto dto, CancellationToken cancellationToken)
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
        ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
    }
}
