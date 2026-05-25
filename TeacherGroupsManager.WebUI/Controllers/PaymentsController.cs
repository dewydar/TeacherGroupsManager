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
        if (dto.StudentId <= 0) ModelState.AddModelError(string.Empty, "اختر الطالب");
        if (dto.GroupId <= 0 || dto.AcademicYearId <= 0) ModelState.AddModelError(string.Empty, "اختر الطالب مرة أخرى لتحديد المجموعة والسنة الدراسية");
        if (!ModelState.IsValid)
        {
            ViewBag.Students = await studentService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        var result = await service.CreateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join("، ", result.Errors));
        ViewBag.Students = await studentService.GetAllAsync(cancellationToken);
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var payment = await service.GetByIdAsync(id, cancellationToken);
        if (payment is null) return NotFound();
        ViewBag.Students = await studentService.GetAllAsync(cancellationToken);
        return View(new EditMonthlyPaymentDto(payment.Id, payment.StudentId, payment.GroupId, payment.AcademicYearId, payment.Month, payment.Year, payment.RequiredAmount, payment.PaidAmount, payment.PaymentStatus, payment.PaymentDate, payment.Notes, null));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditMonthlyPaymentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Students = await studentService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        if (dto.StudentId <= 0) ModelState.AddModelError(string.Empty, "اختر الطالب");
        if (dto.GroupId <= 0 || dto.AcademicYearId <= 0) ModelState.AddModelError(string.Empty, "اختر الطالب مرة أخرى لتحديد المجموعة والسنة الدراسية");
        if (!ModelState.IsValid)
        {
            ViewBag.Students = await studentService.GetAllAsync(cancellationToken);
            return View(dto);
        }
        var result = await service.UpdateAsync(dto, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        ModelState.AddModelError(string.Empty, string.Join("، ", result.Errors));
        ViewBag.Students = await studentService.GetAllAsync(cancellationToken);
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? result.Message : string.Join("، ", result.Errors);
        return RedirectToAction(nameof(Index));
    }
}
