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
        var dateValidation = ValidateMonthYear(dto.Month, dto.Year);
        if (!dateValidation.Succeeded) return dateValidation;
        if (dto.PaidAmount < 0) return OperationResult.Failure(localizer["PaidAmountCannotBeNegative"]);

        var student = await GetPaymentStudentAsync(dto.StudentId, cancellationToken);
        if (student is null) return OperationResult.Failure(localizer["StudentNotFound"]);
        var requiredAmount = GetRequiredAmount(student);
        if (requiredAmount < 0) return OperationResult.Failure(localizer["MonthlyPriceCannotBeNegative"]);
        if (dto.PaidAmount > requiredAmount) return OperationResult.Failure(localizer["PaidAmountCannotExceedRequired"]);

        if (await unitOfWork.Repository<MonthlyPayment>().AnyAsync(x => x.StudentId == dto.StudentId && x.GroupId == student.GroupId && x.AcademicYearId == student.AcademicYearId && x.Month == dto.Month && x.Year == dto.Year, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicatePaymentForMonth"]);
        }

        var remaining = Math.Max(0, requiredAmount - dto.PaidAmount);
        var status = dto.PaidAmount <= 0 ? PaymentStatus.Unpaid : remaining == 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        await unitOfWork.Repository<MonthlyPayment>().AddAsync(new MonthlyPayment
        {
            StudentId = dto.StudentId,
            GroupId = student.GroupId,
            AcademicYearId = student.AcademicYearId,
            Month = dto.Month,
            Year = dto.Year,
            RequiredAmount = requiredAmount,
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
        var dateValidation = ValidateMonthYear(dto.Month, dto.Year);
        if (!dateValidation.Succeeded) return dateValidation;
        if (dto.PaidAmount < 0) return OperationResult.Failure(localizer["PaidAmountCannotBeNegative"]);

        var payment = await unitOfWork.Repository<MonthlyPayment>().GetByIdAsync(dto.Id, cancellationToken);
        if (payment is null) return OperationResult.Failure(localizer["PaymentNotFound"]);
        var student = await GetPaymentStudentAsync(dto.StudentId, cancellationToken);
        if (student is null) return OperationResult.Failure(localizer["StudentNotFound"]);
        var requiredAmount = GetRequiredAmount(student);
        if (requiredAmount < 0) return OperationResult.Failure(localizer["MonthlyPriceCannotBeNegative"]);
        if (dto.PaidAmount > requiredAmount) return OperationResult.Failure(localizer["PaidAmountCannotExceedRequired"]);

        if (await unitOfWork.Repository<MonthlyPayment>().AnyAsync(x => x.Id != dto.Id && x.StudentId == dto.StudentId && x.GroupId == student.GroupId && x.AcademicYearId == student.AcademicYearId && x.Month == dto.Month && x.Year == dto.Year, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicatePaymentForMonth"]);
        }

        var remaining = Math.Max(0, requiredAmount - dto.PaidAmount);
        payment.StudentId = dto.StudentId;
        payment.GroupId = student.GroupId;
        payment.AcademicYearId = student.AcademicYearId;
        payment.Month = dto.Month;
        payment.Year = dto.Year;
        payment.RequiredAmount = requiredAmount;
        payment.PaidAmount = dto.PaidAmount;
        payment.RemainingAmount = remaining;
        payment.PaymentStatus = dto.PaidAmount <= 0 ? PaymentStatus.Unpaid : remaining == 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        payment.PaymentDate = dto.PaymentDate;
        payment.Notes = dto.Notes?.Trim();
        payment.CreatedByEmployeeId = dto.CreatedByEmployeeId;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["PaymentUpdated"]);
    }

    public async Task<OperationResult> GenerateMonthlyPaymentsAsync(int academicYearId, int groupId, int month, int year, CancellationToken cancellationToken = default)
    {
        var dateValidation = ValidateMonthYear(month, year);
        if (!dateValidation.Succeeded) return dateValidation;

        var academicYear = await unitOfWork.Repository<AcademicYear>().Query()
            .FirstOrDefaultAsync(x => x.Id == academicYearId, cancellationToken);
        if (academicYear is null) return OperationResult.Failure(localizer["AcademicYearNotFound"]);
        if (academicYear.MonthlyPrice < 0) return OperationResult.Failure(localizer["MonthlyPriceCannotBeNegative"]);

        var group = await unitOfWork.Repository<Group>().Query()
            .FirstOrDefaultAsync(x => x.Id == groupId && x.AcademicYearId == academicYearId, cancellationToken);
        if (group is null) return OperationResult.Failure(localizer["GroupNotFound"]);
        if (group.MonthlyPrice is < 0) return OperationResult.Failure(localizer["MonthlyPriceCannotBeNegative"]);

        var requiredAmount = group.MonthlyPrice ?? academicYear.MonthlyPrice;
        var students = await unitOfWork.Repository<Student>().Query()
            .Where(x => x.IsActive && x.AcademicYearId == academicYearId && x.GroupId == groupId)
            .ToListAsync(cancellationToken);

        var studentIds = students.Select(x => x.Id).ToArray();
        var existingStudentIds = await unitOfWork.Repository<MonthlyPayment>().Query()
            .Where(x => studentIds.Contains(x.StudentId) && x.GroupId == groupId && x.AcademicYearId == academicYearId && x.Month == month && x.Year == year)
            .Select(x => x.StudentId)
            .ToListAsync(cancellationToken);
        var existing = existingStudentIds.ToHashSet();

        foreach (var student in students.Where(x => !existing.Contains(x.Id)))
        {
            await unitOfWork.Repository<MonthlyPayment>().AddAsync(new MonthlyPayment
            {
                StudentId = student.Id,
                GroupId = groupId,
                AcademicYearId = academicYearId,
                Month = month,
                Year = year,
                RequiredAmount = requiredAmount,
                PaidAmount = 0,
                RemainingAmount = requiredAmount,
                PaymentStatus = PaymentStatus.Unpaid,
                PaymentDate = null
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["MonthlyPaymentsGenerated"]);
    }

    public Task<OperationResult> MarkAsPaidAsync(int paymentId, CancellationToken cancellationToken = default) =>
        UpdatePaidAmountAsync(paymentId, paidAmount: -1, cancellationToken);

    public async Task<OperationResult> MarkAsUnpaidAsync(int paymentId, CancellationToken cancellationToken = default) =>
        await UpdatePaymentStatusAsync(paymentId, 0, cancellationToken);

    public async Task<OperationResult> UpdatePaidAmountAsync(int paymentId, decimal paidAmount, CancellationToken cancellationToken = default)
    {
        if (paidAmount < 0 && paidAmount != -1) return OperationResult.Failure(localizer["PaidAmountCannotBeNegative"]);
        return await UpdatePaymentStatusAsync(paymentId, paidAmount, cancellationToken);
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
            "paymentDate" => desc ? query.OrderByDescending(x => x.PaymentDate) : query.OrderBy(x => x.PaymentDate),
            _ => query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
        };
    }

    private Task<Student?> GetPaymentStudentAsync(int studentId, CancellationToken cancellationToken) =>
        unitOfWork.Repository<Student>().Query()
            .Include(x => x.Group)
            .Include(x => x.AcademicYear)
            .FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

    private static decimal GetRequiredAmount(Student student) =>
        student.Group.MonthlyPrice ?? student.AcademicYear.MonthlyPrice;

    private OperationResult ValidateMonthYear(int month, int year)
    {
        if (month is < 1 or > 12) return OperationResult.Failure(localizer["MonthBetween1And12"]);
        if (year <= 0) return OperationResult.Failure(localizer["InvalidYear"]);
        return OperationResult.Success();
    }

    private async Task<OperationResult> UpdatePaymentStatusAsync(int paymentId, decimal paidAmount, CancellationToken cancellationToken)
    {
        var payment = await unitOfWork.Repository<MonthlyPayment>().GetByIdAsync(paymentId, cancellationToken);
        if (payment is null) return OperationResult.Failure(localizer["PaymentNotFound"]);

        var amount = paidAmount == -1 ? payment.RequiredAmount : paidAmount;
        if (amount > payment.RequiredAmount) return OperationResult.Failure(localizer["PaidAmountCannotExceedRequired"]);

        payment.PaidAmount = amount;
        payment.RemainingAmount = payment.RequiredAmount - amount;
        payment.PaymentStatus = amount <= 0
            ? PaymentStatus.Unpaid
            : payment.RemainingAmount == 0
                ? PaymentStatus.Paid
                : PaymentStatus.PartiallyPaid;
        payment.PaymentDate = amount <= 0 ? null : DateTime.Now;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(payment.PaymentStatus == PaymentStatus.Unpaid ? localizer["PaymentMarkedUnpaid"] : localizer["PaymentMarkedPaid"]);
    }
}
