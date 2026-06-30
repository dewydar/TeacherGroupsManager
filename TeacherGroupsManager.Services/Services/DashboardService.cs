using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Shared.Extensions;

namespace TeacherGroupsManager.Services.Services;

public class DashboardService(IUnitOfWork unitOfWork, AppMapper? mapper = null) : IDashboardService
{
    private const int DashboardListLimit = 5;

    public Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default) =>
        GetDashboardSummaryAsync(new DashboardFilterDto(), cancellationToken);

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(DashboardFilterDto filter, CancellationToken cancellationToken = default)
    {
        filter = NormalizeFilter(filter);
        var students = FilterStudents(unitOfWork.Repository<Student>().Query().AsNoTracking(), filter);
        var groups = FilterGroups(unitOfWork.Repository<Group>().Query().AsNoTracking(), filter);
        var lessons = FilterLessons(unitOfWork.Repository<Lesson>().Query().AsNoTracking(), filter);
        var payments = FilterPayments(unitOfWork.Repository<MonthlyPayment>().Query().AsNoTracking(), filter);
        var employees = unitOfWork.Repository<Employee>().Query().AsNoTracking().Include(x => x.Role);

        var summary = new DashboardSummaryDto
        {
            Filter = filter,
            AcademicYears = mapper is null
                ? []
                : mapper.Map(await unitOfWork.Repository<AcademicYear>().Query().AsNoTracking().OrderByDescending(x => x.StartDate).ToListAsync(cancellationToken)),
            Groups = mapper is null
                ? []
                : mapper.Map(await unitOfWork.Repository<Group>().Query().AsNoTracking().Include(x => x.AcademicYear).Include(x => x.Schedules).OrderBy(x => x.Name).ToListAsync(cancellationToken)),
            TotalStudents = await students.CountAsync(x => x.IsActive, cancellationToken),
            TotalGroups = await groups.CountAsync(x => x.IsActive, cancellationToken),
            TotalPublicGroups = await groups.CountAsync(x => x.IsActive && x.GroupType == GroupType.Public, cancellationToken),
            TotalPrivateGroups = await groups.CountAsync(x => x.IsActive && x.GroupType == GroupType.Private, cancellationToken),
            TotalAcademicYears = await unitOfWork.Repository<AcademicYear>().Query().AsNoTracking().CountAsync(x => x.IsActive, cancellationToken),
            TotalEmployees = await employees.CountAsync(cancellationToken),
            TotalTeachers = await employees.CountAsync(x => x.Role.Name == AppConstants.TeacherRole, cancellationToken),
            TotalAssistantTeachers = await employees.CountAsync(x => x.Role.Name == AppConstants.AssistantTeacherRole, cancellationToken),
            CurrentMonthLessons = await lessons.CountAsync(cancellationToken),
            TotalRequiredAmount = await payments.SumAsync(x => (decimal?)x.RequiredAmount, cancellationToken) ?? 0,
            TotalPaidAmount = await payments.SumAsync(x => (decimal?)x.PaidAmount, cancellationToken) ?? 0,
            TotalRemainingAmount = await payments.SumAsync(x => (decimal?)x.RemainingAmount, cancellationToken) ?? 0,
            PaidStudentsCount = await payments.CountAsync(x => x.PaymentStatus == PaymentStatus.Paid, cancellationToken),
            UnpaidStudentsCount = await payments.CountAsync(x => x.PaymentStatus == PaymentStatus.Unpaid, cancellationToken),
            PartiallyPaidStudentsCount = await payments.CountAsync(x => x.PaymentStatus == PaymentStatus.PartiallyPaid, cancellationToken)
        };

        summary.StudentsPerGroup = await GetStudentsPerGroupAsync(filter, cancellationToken);
        summary.PaymentsPerGroup = await GetPaymentsPerGroupAsync(filter, cancellationToken);
        summary.PaymentStatusSummary = BuildPaymentStatusSummary(summary, filter);
        summary.MonthlyRevenueSummary = await GetMonthlyRevenueSummaryAsync(filter, cancellationToken);
        summary.GroupsByDay = await GetGroupsByDayAsync(filter, cancellationToken);
        summary.RecentStudents = await GetRecentStudentsAsync(filter, cancellationToken);
        summary.RecentPayments = await GetRecentPaymentsAsync(filter, cancellationToken);
        summary.UpcomingGroups = await GetUpcomingGroupsAsync(filter, cancellationToken);
        summary.MainCards = BuildMainCards(summary);

        return summary;
    }

    private static DashboardFilterDto NormalizeFilter(DashboardFilterDto? filter)
    {
        var now = DateTime.Now;
        return new DashboardFilterDto
        {
            AcademicYearId = filter?.AcademicYearId,
            GroupId = filter?.GroupId,
            Month = filter?.Month is >= 1 and <= 12 ? filter.Month : now.Month,
            Year = filter?.Year is > 0 ? filter.Year : now.Year
        };
    }

    private static IQueryable<Student> FilterStudents(IQueryable<Student> query, DashboardFilterDto filter)
    {
        if (filter.AcademicYearId is { } academicYearId) query = query.Where(x => x.AcademicYearId == academicYearId);
        if (filter.GroupId is { } groupId) query = query.Where(x => x.GroupId == groupId);
        return query;
    }

    private static IQueryable<Group> FilterGroups(IQueryable<Group> query, DashboardFilterDto filter)
    {
        if (filter.AcademicYearId is { } academicYearId) query = query.Where(x => x.AcademicYearId == academicYearId);
        if (filter.GroupId is { } groupId) query = query.Where(x => x.Id == groupId);
        return query;
    }

    private static IQueryable<Lesson> FilterLessons(IQueryable<Lesson> query, DashboardFilterDto filter)
    {
        if (filter.GroupId is { } groupId) query = query.Where(x => x.GroupId == groupId);
        if (filter.AcademicYearId is { } academicYearId) query = query.Where(x => x.Group.AcademicYearId == academicYearId);
        return query.Where(x => x.Month == filter.Month && x.Year == filter.Year);
    }

    private static IQueryable<MonthlyPayment> FilterPayments(IQueryable<MonthlyPayment> query, DashboardFilterDto filter)
    {
        if (filter.AcademicYearId is { } academicYearId) query = query.Where(x => x.AcademicYearId == academicYearId);
        if (filter.GroupId is { } groupId) query = query.Where(x => x.GroupId == groupId);
        return query.Where(x => x.Month == filter.Month && x.Year == filter.Year);
    }

    private async Task<IReadOnlyList<GroupStudentsSummaryDto>> GetStudentsPerGroupAsync(DashboardFilterDto filter, CancellationToken cancellationToken)
    {
        var query = FilterGroups(unitOfWork.Repository<Group>().Query().AsNoTracking(), filter);
        return await query
            .OrderBy(x => x.Name)
            .Select(x => new GroupStudentsSummaryDto(
                x.Id,
                x.Name,
                x.Students.Count(student => student.IsActive),
                $"/Groups/Details/{x.Id}"))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<GroupPaymentSummaryDto>> GetPaymentsPerGroupAsync(DashboardFilterDto filter, CancellationToken cancellationToken)
    {
        var query = FilterGroups(unitOfWork.Repository<Group>().Query().AsNoTracking(), filter);
        return await query
            .OrderBy(x => x.Name)
            .Take(DashboardListLimit)
            .Select(group => new GroupPaymentSummaryDto(
                group.Id,
                group.Name,
                group.Students.Count(student => student.IsActive),
                group.Students.SelectMany(student => student.MonthlyPayments).Count(payment => payment.Month == filter.Month && payment.Year == filter.Year && payment.PaymentStatus == PaymentStatus.Paid),
                group.Students.SelectMany(student => student.MonthlyPayments).Count(payment => payment.Month == filter.Month && payment.Year == filter.Year && payment.PaymentStatus == PaymentStatus.Unpaid),
                group.Students.SelectMany(student => student.MonthlyPayments).Count(payment => payment.Month == filter.Month && payment.Year == filter.Year && payment.PaymentStatus == PaymentStatus.PartiallyPaid),
                group.Students.SelectMany(student => student.MonthlyPayments).Where(payment => payment.Month == filter.Month && payment.Year == filter.Year).Sum(payment => (decimal?)payment.RequiredAmount) ?? 0,
                group.Students.SelectMany(student => student.MonthlyPayments).Where(payment => payment.Month == filter.Month && payment.Year == filter.Year).Sum(payment => (decimal?)payment.PaidAmount) ?? 0,
                group.Students.SelectMany(student => student.MonthlyPayments).Where(payment => payment.Month == filter.Month && payment.Year == filter.Year).Sum(payment => (decimal?)payment.RemainingAmount) ?? 0,
                $"/Groups/Details/{group.Id}"))
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyList<PaymentStatusSummaryDto> BuildPaymentStatusSummary(DashboardSummaryDto summary, DashboardFilterDto filter) =>
    [
        new(PaymentStatus.Paid, PaymentStatus.Paid.ToArabic(), summary.PaidStudentsCount, $"/Payments?status=Paid&month={filter.Month}&year={filter.Year}"),
        new(PaymentStatus.PartiallyPaid, PaymentStatus.PartiallyPaid.ToArabic(), summary.PartiallyPaidStudentsCount, $"/Payments?status=PartiallyPaid&month={filter.Month}&year={filter.Year}"),
        new(PaymentStatus.Unpaid, PaymentStatus.Unpaid.ToArabic(), summary.UnpaidStudentsCount, $"/Payments?status=Unpaid&month={filter.Month}&year={filter.Year}")
    ];

    private async Task<IReadOnlyList<MonthlyRevenueSummaryDto>> GetMonthlyRevenueSummaryAsync(DashboardFilterDto filter, CancellationToken cancellationToken)
    {
        var monthStarts = Enumerable.Range(0, 6)
            .Select(offset => new DateTime(filter.Year, filter.Month, 1).AddMonths(-5 + offset))
            .ToList();
        var from = monthStarts.First();
        var fromKey = from.Year * 100 + from.Month;
        var toKey = filter.Year * 100 + filter.Month;

        var query = unitOfWork.Repository<MonthlyPayment>().Query().AsNoTracking();
        if (filter.AcademicYearId is { } academicYearId) query = query.Where(x => x.AcademicYearId == academicYearId);
        if (filter.GroupId is { } groupId) query = query.Where(x => x.GroupId == groupId);

        var grouped = await query
            .Where(x => x.Year * 100 + x.Month >= fromKey && x.Year * 100 + x.Month <= toKey)
            .GroupBy(x => new { x.Year, x.Month })
            .Select(x => new
            {
                x.Key.Year,
                x.Key.Month,
                Required = x.Sum(payment => (decimal?)payment.RequiredAmount) ?? 0,
                Paid = x.Sum(payment => (decimal?)payment.PaidAmount) ?? 0,
                Remaining = x.Sum(payment => (decimal?)payment.RemainingAmount) ?? 0
            })
            .ToListAsync(cancellationToken);

        return monthStarts
            .Select(month =>
            {
                var item = grouped.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month);
                return new MonthlyRevenueSummaryDto(month.Month, month.Year, MonthNameArabic(month.Month), item?.Required ?? 0, item?.Paid ?? 0, item?.Remaining ?? 0);
            })
            .ToList();
    }

    private async Task<IReadOnlyList<GroupsByDaySummaryDto>> GetGroupsByDayAsync(DashboardFilterDto filter, CancellationToken cancellationToken)
    {
        var query = unitOfWork.Repository<GroupSchedule>().Query().AsNoTracking();
        if (filter.GroupId is { } groupId) query = query.Where(x => x.GroupId == groupId);
        if (filter.AcademicYearId is { } academicYearId) query = query.Where(x => x.Group.AcademicYearId == academicYearId);

        var grouped = await query
            .GroupBy(x => x.DayOfWeek)
            .Select(x => new
            {
                DayOfWeek = x.Key,
                GroupsCount = x.Select(schedule => schedule.GroupId).Distinct().Count(),
                GroupNames = x.Select(schedule => schedule.Group.Name).Distinct().ToList()
            })
            .ToListAsync(cancellationToken);

        var days = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
        return days
            .Select(day =>
            {
                var item = grouped.FirstOrDefault(x => x.DayOfWeek == day);
                return new GroupsByDaySummaryDto(day, day.DayToArabic(), item?.GroupsCount ?? 0, item?.GroupNames ?? []);
            })
            .ToList();
    }

    private async Task<IReadOnlyList<RecentStudentDto>> GetRecentStudentsAsync(DashboardFilterDto filter, CancellationToken cancellationToken)
    {
        var query = FilterStudents(unitOfWork.Repository<Student>().Query().AsNoTracking(), filter);
        return await query
            .Include(x => x.Group)
            .Include(x => x.AcademicYear)
            .OrderByDescending(x => x.CreatedAt ?? DateTime.MinValue)
            .ThenByDescending(x => x.Id)
            .Take(DashboardListLimit)
            .Select(x => new RecentStudentDto(x.Id, x.FullName, x.Group.Name, x.AcademicYear.Name, x.CreatedAt, $"/Students/Edit/{x.Id}"))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<RecentPaymentDto>> GetRecentPaymentsAsync(DashboardFilterDto filter, CancellationToken cancellationToken)
    {
        var query = FilterPayments(unitOfWork.Repository<MonthlyPayment>().Query().AsNoTracking(), filter);
        var payments = await query
            .Include(x => x.Student)
            .Include(x => x.Group)
            .OrderByDescending(x => x.PaymentDate ?? x.CreatedAt ?? DateTime.MinValue)
            .ThenByDescending(x => x.Id)
            .Take(DashboardListLimit)
            .Select(x => new
            {
                x.Id,
                StudentName = x.Student.FullName,
                GroupName = x.Group.Name,
                x.PaidAmount,
                x.Month,
                x.Year,
                x.PaymentStatus,
                x.PaymentDate
            })
            .ToListAsync(cancellationToken);

        return payments
            .Select(x => new RecentPaymentDto(x.Id, x.StudentName, x.GroupName, x.PaidAmount, x.Month, x.Year, x.PaymentStatus, x.PaymentStatus.ToArabic(), x.PaymentDate, $"/Payments/Edit/{x.Id}"))
            .ToList();
    }

    private async Task<IReadOnlyList<UpcomingGroupDto>> GetUpcomingGroupsAsync(DashboardFilterDto filter, CancellationToken cancellationToken)
    {
        var today = DateTime.Today.DayOfWeek;
        var query = unitOfWork.Repository<GroupSchedule>().Query().AsNoTracking();
        if (filter.GroupId is { } groupId) query = query.Where(x => x.GroupId == groupId);
        if (filter.AcademicYearId is { } academicYearId) query = query.Where(x => x.Group.AcademicYearId == academicYearId);

        var schedules = await query
            .Include(x => x.Group)
            .ThenInclude(x => x.AcademicYear)
            .Select(x => new
            {
                x.GroupId,
                GroupName = x.Group.Name,
                AcademicYearName = x.Group.AcademicYear.Name,
                x.DayOfWeek,
                x.StartTime,
                x.EndTime,
                StudentsCount = x.Group.Students.Count(student => student.IsActive)
            })
            .ToListAsync(cancellationToken);

        return schedules
            .OrderBy(x => DaysUntil(today, x.DayOfWeek))
            .ThenBy(x => x.StartTime)
            .Take(DashboardListLimit)
            .Select(x => new UpcomingGroupDto(x.GroupId, x.GroupName, x.AcademicYearName, "-", x.DayOfWeek, x.DayOfWeek.DayToArabic(), x.StartTime, x.EndTime, x.StudentsCount, $"/Groups/Details/{x.GroupId}"))
            .ToList();
    }

    private static IReadOnlyList<DashboardCardDto> BuildMainCards(DashboardSummaryDto summary) =>
    [
        new("TotalStudents", summary.TotalStudents.ToString("N0"), "/Students"),
        new("TotalGroups", summary.TotalGroups.ToString("N0"), "/Groups"),
        new("PublicGroups", summary.TotalPublicGroups.ToString("N0"), "/Groups?type=Public"),
        new("PrivateGroups", summary.TotalPrivateGroups.ToString("N0"), "/Groups?type=Private"),
        new("AcademicYears", summary.TotalAcademicYears.ToString("N0"), "/AcademicYears"),
        new("TotalTeachers", summary.TotalTeachers.ToString("N0"), "/Employees?role=Teacher"),
        new("TotalAssistants", summary.TotalAssistantTeachers.ToString("N0"), "/Employees?role=AssistantTeacher"),
        new("CurrentMonthLessons", summary.CurrentMonthLessons.ToString("N0"), $"/Lessons?month={summary.Filter.Month}&year={summary.Filter.Year}"),
        new("TotalRequiredThisMonth", Money(summary.TotalRequiredAmount), "/Payments", ValueSuffix: "EgyptianPoundAbbreviation"),
        new("TotalPaidThisMonth", Money(summary.TotalPaidAmount), "/Payments", ValueSuffix: "EgyptianPoundAbbreviation"),
        new("TotalRemainingThisMonth", Money(summary.TotalRemainingAmount), "/Payments", ValueSuffix: "EgyptianPoundAbbreviation"),
        new("PaidStudentsThisMonth", summary.PaidStudentsCount.ToString("N0"), summary.PaidStudentsUrl),
        new("UnpaidStudentsThisMonth", summary.UnpaidStudentsCount.ToString("N0"), summary.UnpaidStudentsUrl),
        new("PartiallyPaidStudentsThisMonth", summary.PartiallyPaidStudentsCount.ToString("N0"), $"/Payments?status=PartiallyPaid&month={summary.Filter.Month}&year={summary.Filter.Year}")
    ];

    private static int DaysUntil(DayOfWeek today, DayOfWeek target) =>
        ((int)target - (int)today + 7) % 7;

    private static string Money(decimal amount) =>
        amount.ToString("N2");

    private static string MonthNameArabic(int month) =>
        CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
}
