using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class PaymentService(IUnitOfWork unitOfWork, AppMapper mapper, IStringLocalizer<SharedResource> localizer) : IPaymentService
{
    public async Task<IReadOnlyList<MonthlyPaymentDto>> GetAllAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default)
    {
        var query = PaymentsQuery();
        if (month.HasValue) query = query.Where(x => x.Month == month);
        if (year.HasValue) query = query.Where(x => x.Year == year);
        return mapper.Map(await query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync(cancellationToken));
    }

    public async Task<MonthlyPaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await PaymentsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) is { } payment ? mapper.Map(payment) : null;

    public Task<DataTableResponseDto<MonthlyPaymentDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default) =>
        DataTableQueryHelper.ToDataTableResponseAsync(
            PaymentsQuery().AsNoTracking(),
            request,
            ApplyFilters,
            ApplySearch,
            ApplySorting,
            mapper.Map,
            cancellationToken);

    public async Task<OperationResult> CreateAsync(CreateMonthlyPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateReferencesAsync(dto.StudentId, dto.GroupId, dto.AcademicYearId, cancellationToken);
        if (!validation.Succeeded) return validation;
        if (await unitOfWork.Repository<MonthlyPayment>().AnyAsync(x => x.StudentId == dto.StudentId && x.Month == dto.Month && x.Year == dto.Year, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicatePaymentForMonth"]);
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
        return OperationResult.Success(localizer["PaymentSaved"]);
    }

    public async Task<OperationResult> UpdateAsync(EditMonthlyPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Repository<MonthlyPayment>().GetByIdAsync(dto.Id, cancellationToken);
        if (payment is null) return OperationResult.Failure(localizer["PaymentNotFound"]);
        var validation = await ValidateReferencesAsync(dto.StudentId, dto.GroupId, dto.AcademicYearId, cancellationToken);
        if (!validation.Succeeded) return validation;
        if (await unitOfWork.Repository<MonthlyPayment>().AnyAsync(x => x.Id != dto.Id && x.StudentId == dto.StudentId && x.Month == dto.Month && x.Year == dto.Year, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicatePaymentForMonth"]);
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
        return OperationResult.Success(localizer["PaymentUpdated"]);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var payment = await unitOfWork.Repository<MonthlyPayment>().GetByIdAsync(id, cancellationToken);
        if (payment is null) return OperationResult.Failure(localizer["PaymentNotFound"]);
        unitOfWork.Repository<MonthlyPayment>().Delete(payment);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, localizer["PaymentDeleted"], localizer["CannotDeleteLinkedRecord"], cancellationToken);
    }

    private IQueryable<MonthlyPayment> PaymentsQuery() => unitOfWork.Repository<MonthlyPayment>().Query()
        .Include(x => x.Student)
        .Include(x => x.Group)
        .Include(x => x.AcademicYear)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private static IQueryable<MonthlyPayment> ApplyFilters(IQueryable<MonthlyPayment> query, DataTableRequestDto request)
    {
        if (request.FilterInt("studentId") is { } studentId) query = query.Where(x => x.StudentId == studentId);
        if (request.Filter("student") is { } student) query = query.Where(x => x.Student.FullName.Contains(student));
        if (request.FilterInt("academicYearId") is { } academicYearId) query = query.Where(x => x.AcademicYearId == academicYearId);
        if (request.FilterInt("groupId") is { } groupId) query = query.Where(x => x.GroupId == groupId);
        if (request.FilterInt("month") is { } month) query = query.Where(x => x.Month == month);
        if (request.FilterInt("year") is { } year) query = query.Where(x => x.Year == year);
        if (request.FilterInt("paymentStatus") is { } status) query = query.Where(x => (int)x.PaymentStatus == status);
        if (request.FilterDateTime("paymentDateFrom") is { } from) query = query.Where(x => x.PaymentDate != null && x.PaymentDate.Value.Date >= from);
        if (request.FilterDateTime("paymentDateTo") is { } to) query = query.Where(x => x.PaymentDate != null && x.PaymentDate.Value.Date <= to);
        return query;
    }

    private static IQueryable<MonthlyPayment> ApplySearch(IQueryable<MonthlyPayment> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        search = search.Trim();
        return query.Where(x =>
            x.Student.FullName.Contains(search) ||
            x.Group.Name.Contains(search) ||
            x.AcademicYear.Name.Contains(search));
    }

    private static IQueryable<MonthlyPayment> ApplySorting(IQueryable<MonthlyPayment> query, string? sortColumn, string? sortDirection)
    {
        var desc = DataTableQueryHelper.Descending(sortDirection);
        return sortColumn switch
        {
            "studentName" => desc ? query.OrderByDescending(x => x.Student.FullName) : query.OrderBy(x => x.Student.FullName),
            "groupName" => desc ? query.OrderByDescending(x => x.Group.Name) : query.OrderBy(x => x.Group.Name),
            "academicYearName" => desc ? query.OrderByDescending(x => x.AcademicYear.Name) : query.OrderBy(x => x.AcademicYear.Name),
            "month" => desc ? query.OrderByDescending(x => x.Month) : query.OrderBy(x => x.Month),
            "year" => desc ? query.OrderByDescending(x => x.Year) : query.OrderBy(x => x.Year),
            "requiredAmount" => desc ? query.OrderByDescending(x => x.RequiredAmount) : query.OrderBy(x => x.RequiredAmount),
            "paidAmount" => desc ? query.OrderByDescending(x => x.PaidAmount) : query.OrderBy(x => x.PaidAmount),
            "remainingAmount" => desc ? query.OrderByDescending(x => x.RemainingAmount) : query.OrderBy(x => x.RemainingAmount),
            "paymentStatus" => desc ? query.OrderByDescending(x => x.PaymentStatus) : query.OrderBy(x => x.PaymentStatus),
            _ => query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
        };
    }

    private async Task<OperationResult> ValidateReferencesAsync(int studentId, int groupId, int academicYearId, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<Student>().AnyAsync(x => x.Id == studentId, cancellationToken))
        {
            return OperationResult.Failure(localizer["StudentNotFound"]);
        }
        if (!await unitOfWork.Repository<Group>().AnyAsync(x => x.Id == groupId, cancellationToken))
        {
            return OperationResult.Failure(localizer["GroupNotFound"]);
        }
        if (!await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Id == academicYearId, cancellationToken))
        {
            return OperationResult.Failure(localizer["AcademicYearNotFound"]);
        }
        return OperationResult.Success();
    }
}
