using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class PaymentService(IUnitOfWork unitOfWork, IMapper mapper) : IPaymentService
{
    public async Task<IReadOnlyList<MonthlyPaymentDto>> GetAllAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default)
    {
        var query = PaymentsQuery();
        if (month.HasValue) query = query.Where(x => x.Month == month);
        if (year.HasValue) query = query.Where(x => x.Year == year);
        return mapper.Map<List<MonthlyPaymentDto>>(await query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync(cancellationToken));
    }

    public async Task<MonthlyPaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        mapper.Map<MonthlyPaymentDto?>(await PaymentsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateMonthlyPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateReferencesAsync(dto.StudentId, dto.GroupId, dto.AcademicYearId, cancellationToken);
        if (!validation.Succeeded) return validation;
        if (await unitOfWork.Repository<MonthlyPayment>().AnyAsync(x => x.StudentId == dto.StudentId && x.Month == dto.Month && x.Year == dto.Year, cancellationToken))
        {
            return OperationResult.Failure("المدفوعة موجودة من قبل لهذا الشهر");
        }

        var remaining = Math.Max(0, dto.RequiredAmount - dto.PaidAmount);
        var status = dto.PaidAmount <= 0 ? PaymentStatus.Unpaid : remaining == 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        await unitOfWork.Repository<MonthlyPayment>().AddAsync(new MonthlyPayment
        {
            StudentId = dto.StudentId,
            GroupId = dto.GroupId,
            AcademicYearId = dto.AcademicYearId,
            Month = dto.Month,
            Year = dto.Year,
            RequiredAmount = dto.RequiredAmount,
            PaidAmount = dto.PaidAmount,
            RemainingAmount = remaining,
            PaymentStatus = status,
            PaymentDate = dto.PaymentDate,
            Notes = dto.Notes?.Trim(),
            CreatedByEmployeeId = dto.CreatedByEmployeeId
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ المدفوعة بنجاح");
    }

    public async Task<OperationResult> UpdateAsync(EditMonthlyPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Repository<MonthlyPayment>().GetByIdAsync(dto.Id, cancellationToken);
        if (payment is null) return OperationResult.Failure("المدفوعة غير موجودة");
        var validation = await ValidateReferencesAsync(dto.StudentId, dto.GroupId, dto.AcademicYearId, cancellationToken);
        if (!validation.Succeeded) return validation;
        if (await unitOfWork.Repository<MonthlyPayment>().AnyAsync(x => x.Id != dto.Id && x.StudentId == dto.StudentId && x.Month == dto.Month && x.Year == dto.Year, cancellationToken))
        {
            return OperationResult.Failure("المدفوعة موجودة من قبل لهذا الشهر");
        }

        var remaining = Math.Max(0, dto.RequiredAmount - dto.PaidAmount);
        payment.StudentId = dto.StudentId;
        payment.GroupId = dto.GroupId;
        payment.AcademicYearId = dto.AcademicYearId;
        payment.Month = dto.Month;
        payment.Year = dto.Year;
        payment.RequiredAmount = dto.RequiredAmount;
        payment.PaidAmount = dto.PaidAmount;
        payment.RemainingAmount = remaining;
        payment.PaymentStatus = dto.PaidAmount <= 0 ? PaymentStatus.Unpaid : remaining == 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        payment.PaymentDate = dto.PaymentDate;
        payment.Notes = dto.Notes?.Trim();
        payment.CreatedByEmployeeId = dto.CreatedByEmployeeId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم تعديل المدفوعة بنجاح");
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Repository<MonthlyPayment>().GetByIdAsync(id, cancellationToken);
        if (payment is null) return OperationResult.Failure("المدفوعة غير موجودة");
        unitOfWork.Repository<MonthlyPayment>().Delete(payment);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, "تم حذف المدفوعة بنجاح", cancellationToken);
    }

    private IQueryable<MonthlyPayment> PaymentsQuery() => unitOfWork.Repository<MonthlyPayment>().Query()
        .Include(x => x.Student)
        .Include(x => x.Group)
        .Include(x => x.AcademicYear)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private async Task<OperationResult> ValidateReferencesAsync(int studentId, int groupId, int academicYearId, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<Student>().AnyAsync(x => x.Id == studentId, cancellationToken))
        {
            return OperationResult.Failure("الطالب غير موجود");
        }
        if (!await unitOfWork.Repository<Group>().AnyAsync(x => x.Id == groupId, cancellationToken))
        {
            return OperationResult.Failure("المجموعة غير موجودة");
        }
        if (!await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Id == academicYearId, cancellationToken))
        {
            return OperationResult.Failure("السنة الدراسية غير موجودة");
        }
        return OperationResult.Success();
    }
}
