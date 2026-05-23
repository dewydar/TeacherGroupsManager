using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.WebUI.Controllers;

[Authorize(Policy = PermissionCodes.PaymentsManage)]
public class PaymentsController(IPaymentService service, IStudentService studentService) : Controller
{
    public async Task<IActionResult> Index(int? month, int? year, PaymentStatus? status, CancellationToken cancellationToken)
    {
        var data = await service.GetAllAsync(month, year, cancellationToken);
        return View(status.HasValue ? data.Where(x => x.PaymentStatus == status).ToList() : data);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Students = await studentService.GetAllAsync(cancellationToken);
        var now = DateTime.Now;
        return View(new CreateMonthlyPaymentDto(0, 0, 0, now.Month, now.Year, 0, 0, PaymentStatus.Unpaid, null, null, null));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMonthlyPaymentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Students = await studentService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        var result = await service.CreateAsync(dto, cancellationToken);
        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
