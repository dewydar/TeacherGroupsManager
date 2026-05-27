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

[Authorize(Policy = PermissionCodes.LessonsManage)]
public class LessonsController(ILessonService service, IGroupService groupService, IAcademicYearService academicYearService, IStringLocalizer<SharedResource> localizer) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
        ViewBag.AcademicYears = await academicYearService.GetAllAsync(cancellationToken);
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
        ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
        var now = DateTime.Now;
        return View(new CreateLessonDto(string.Empty, null, 0, LessonType.Group, now, 0, true, now.Month, now.Year, null, []));
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableDates(int groupId, int month, int year, DayOfWeek? dayOfWeek, CancellationToken cancellationToken)
    {
        var dates = await service.GetAvailableLessonDatesAsync(groupId, month, year, dayOfWeek, cancellationToken);
        return Json(dates.Select(x => new
        {
            value = x.LessonDate.ToString("yyyy-MM-ddTHH:mm"),
            text = $"{localizer[x.DayOfWeek.ToString()]} - {x.LessonDate:yyyy-MM-dd HH:mm}"
        }));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLessonDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        if (dto.GroupId <= 0)
        {
            ModelState.AddModelError(string.Empty, localizer["SelectGroup"]);
            ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        var result = await service.CreateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors));
        ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var lesson = await service.GetByIdAsync(id, cancellationToken);
        if (lesson is null) return NotFound();
        ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
        return View(new EditLessonDto(lesson.Id, lesson.Title, lesson.Description, lesson.GroupId, lesson.LessonType, lesson.LessonDate, lesson.Price, lesson.IsMonthlyPaymentRequired, lesson.Month, lesson.Year, lesson.CreatedByEmployeeId, []));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditLessonDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        if (dto.GroupId <= 0)
        {
            ModelState.AddModelError(string.Empty, localizer["SelectGroup"]);
            ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        var result = await service.UpdateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors));
        ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
        return View(dto);
    }

    public async Task<IActionResult> Attendance(int id, CancellationToken cancellationToken)
    {
        var attendance = await service.GetAttendanceAsync(id, cancellationToken);
        return attendance is null ? NotFound() : View(attendance);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Attendance(UpdateLessonAttendanceDto dto, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAttendanceAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors));
        var attendance = await service.GetAttendanceAsync(dto.LessonId, cancellationToken);
        return attendance is null ? NotFound() : View(attendance);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join(", ", result.Errors);
        return RedirectToAction(nameof(Index));
    }
}




