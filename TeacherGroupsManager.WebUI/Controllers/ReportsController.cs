using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.WebUI.Infrastructure;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.ReportsView)]
public class ReportsController(
    IStudentService studentService,
    IPaymentService paymentService,
    ILessonService lessonService,
    IAcademicYearService academicYearService,
    IGroupService groupService) : Controller
{
    public IActionResult Index() => View();

    public async Task<IActionResult> Students(CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken);
        return View();
    }

    public async Task<IActionResult> Payments(CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken);
        return View();
    }

    public async Task<IActionResult> Lessons(CancellationToken cancellationToken)
    {
        await FillLookups(cancellationToken);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StudentsData(CancellationToken cancellationToken)
    {
        var request = DataTablesRequestHelper.Parse(Request);
        var result = await studentService.GetPagedAsync(request, cancellationToken);
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PaymentsData(CancellationToken cancellationToken)
    {
        var request = DataTablesRequestHelper.Parse(Request);
        var result = await paymentService.GetPagedAsync(request, cancellationToken);
        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LessonsData(CancellationToken cancellationToken)
    {
        var request = DataTablesRequestHelper.Parse(Request);
        var result = await lessonService.GetPagedAsync(request, cancellationToken);
        return Json(result);
    }

    private async Task FillLookups(CancellationToken cancellationToken)
    {
        ViewBag.AcademicYears = await academicYearService.GetAllAsync(cancellationToken);
        ViewBag.Groups = await groupService.GetAllAsync(cancellationToken);
    }
}
