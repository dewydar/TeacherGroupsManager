using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.ReportsView)]
public class ReportsController(IReportService reportService) : Controller
{
    public IActionResult Index() => View();
    public async Task<IActionResult> Students(int? academicYearId, int? groupId, CancellationToken cancellationToken) => View(await reportService.GetStudentsReportAsync(academicYearId, groupId, cancellationToken));
    public async Task<IActionResult> Payments(int? month, int? year, int? groupId, CancellationToken cancellationToken) => View(await reportService.GetPaymentsReportAsync(month, year, groupId, cancellationToken));
    public async Task<IActionResult> Lessons(int? groupId, int? month, int? year, CancellationToken cancellationToken) => View(await reportService.GetLessonsReportAsync(groupId, month, year, cancellationToken));
}
