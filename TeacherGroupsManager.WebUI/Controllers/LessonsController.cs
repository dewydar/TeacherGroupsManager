using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.LessonsManage)]
public class LessonsController(ILessonService service, IGroupService groupService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) => View(await service.GetAllAsync(cancellationToken));
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
        var now = DateTime.Now;
        return View(new CreateLessonDto(string.Empty, null, 0, LessonType.Group, now, 0, true, now.Month, now.Year, null, []));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLessonDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        var result = await service.CreateAsync(dto, cancellationToken);
        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
