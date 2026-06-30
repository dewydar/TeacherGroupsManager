using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Dtos;

public class DashboardFilterDto
{
    public int? AcademicYearId { get; set; }
    public int? GroupId { get; set; }
    public int Month { get; set; } = DateTime.Now.Month;
    public int Year { get; set; } = DateTime.Now.Year;
}

public class DashboardSummaryDto
{
    public DashboardFilterDto Filter { get; set; } = new();
    public IReadOnlyList<AcademicYearDto> AcademicYears { get; set; } = [];
    public IReadOnlyList<GroupDto> Groups { get; set; } = [];
    public IReadOnlyList<DashboardCardDto> MainCards { get; set; } = [];

    public int TotalStudents { get; set; }
    public int TotalGroups { get; set; }
    public int TotalPublicGroups { get; set; }
    public int TotalPrivateGroups { get; set; }
    public int TotalAcademicYears { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalAssistantTeachers { get; set; }
    public int CurrentMonthLessons { get; set; }

    public decimal TotalRequiredAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalRemainingAmount { get; set; }

    public int PaidStudentsCount { get; set; }
    public int UnpaidStudentsCount { get; set; }
    public int PartiallyPaidStudentsCount { get; set; }

    public IReadOnlyList<GroupStudentsSummaryDto> StudentsPerGroup { get; set; } = [];
    public IReadOnlyList<GroupPaymentSummaryDto> PaymentsPerGroup { get; set; } = [];
    public IReadOnlyList<PaymentStatusSummaryDto> PaymentStatusSummary { get; set; } = [];
    public IReadOnlyList<MonthlyRevenueSummaryDto> MonthlyRevenueSummary { get; set; } = [];
    public IReadOnlyList<GroupsByDaySummaryDto> GroupsByDay { get; set; } = [];
    public IReadOnlyList<RecentStudentDto> RecentStudents { get; set; } = [];
    public IReadOnlyList<RecentPaymentDto> RecentPayments { get; set; } = [];
    public IReadOnlyList<UpcomingGroupDto> UpcomingGroups { get; set; } = [];

    public int CurrentMonthPaidStudentsCount => PaidStudentsCount;
    public int CurrentMonthUnpaidStudentsCount => UnpaidStudentsCount;
    public int CurrentMonthPartiallyPaidStudentsCount => PartiallyPaidStudentsCount;
    public decimal CurrentMonthTotalRequiredAmount => TotalRequiredAmount;
    public decimal CurrentMonthTotalPaidAmount => TotalPaidAmount;
    public decimal CurrentMonthTotalRemainingAmount => TotalRemainingAmount;
    public string StudentsUrl => "/Students";
    public string GroupsUrl => "/Groups";
    public string PrivateGroupsUrl => "/Groups?type=Private";
    public string PublicGroupsUrl => "/Groups?type=Public";
    public string PaidStudentsUrl => $"/Payments?month={Filter.Month}&year={Filter.Year}&status=Paid";
    public string UnpaidStudentsUrl => $"/Payments?month={Filter.Month}&year={Filter.Year}&status=Unpaid";
    public IReadOnlyList<GroupStudentsSummaryDto> StudentsCountPerGroup => StudentsPerGroup;
}

public record DashboardCardDto(
    [StringLength(AppConstants.MaxStringLength)] string Title,
    [StringLength(AppConstants.MaxStringLength)] string Value,
    [StringLength(AppConstants.MaxStringLength)] string Url,
    [StringLength(AppConstants.MaxStringLength)] string CssClass = "",
    [StringLength(AppConstants.MaxStringLength)] string ValueSuffix = "");

public record GroupStudentsSummaryDto(
    int GroupId,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    int StudentsCount,
    [StringLength(AppConstants.MaxStringLength)] string GroupDetailsUrl);

public record GroupPaymentSummaryDto(
    int GroupId,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    int TotalStudents,
    int PaidStudentsCount,
    int UnpaidStudentsCount,
    int PartiallyPaidStudentsCount,
    decimal TotalRequiredAmount,
    decimal TotalCollectedAmount,
    decimal RemainingAmount,
    [StringLength(AppConstants.MaxStringLength)] string GroupDetailsUrl);

public record PaymentStatusSummaryDto(
    PaymentStatus Status,
    [StringLength(AppConstants.MaxStringLength)] string StatusArabic,
    int Count,
    [StringLength(AppConstants.MaxStringLength)] string Url);

public record MonthlyRevenueSummaryDto(
    int Month,
    int Year,
    [StringLength(AppConstants.MaxStringLength)] string MonthNameArabic,
    decimal TotalRequiredAmount,
    decimal TotalPaidAmount,
    decimal TotalRemainingAmount);

public record GroupsByDaySummaryDto(
    DayOfWeek DayOfWeek,
    [StringLength(AppConstants.MaxStringLength)] string DayOfWeekArabic,
    int GroupsCount,
    IReadOnlyList<string> GroupNames);

public record RecentPaymentDto(
    int PaymentId,
    [StringLength(AppConstants.MaxStringLength)] string StudentName,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    decimal PaidAmount,
    int Month,
    int Year,
    PaymentStatus PaymentStatus,
    [StringLength(AppConstants.MaxStringLength)] string PaymentStatusArabic,
    DateTime? PaymentDate,
    [StringLength(AppConstants.MaxStringLength)] string Url);

public record RecentStudentDto(
    int StudentId,
    [StringLength(AppConstants.MaxStringLength)] string StudentName,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    [StringLength(AppConstants.MaxStringLength)] string AcademicYearName,
    DateTime? CreatedAt,
    [StringLength(AppConstants.MaxStringLength)] string Url);

public record UpcomingGroupDto(
    int GroupId,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    [StringLength(AppConstants.MaxStringLength)] string AcademicYearName,
    [StringLength(AppConstants.MaxStringLength)] string TeacherName,
    DayOfWeek DayOfWeek,
    [StringLength(AppConstants.MaxStringLength)] string DayOfWeekArabic,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int StudentsCount,
    [StringLength(AppConstants.MaxStringLength)] string Url);

public record DashboardChartDto(
    [StringLength(AppConstants.MaxStringLength)] string Title,
    IReadOnlyList<string> Labels,
    IReadOnlyList<decimal> Values);

public record GroupDayDto(
    DayOfWeek DayOfWeek,
    [StringLength(AppConstants.MaxStringLength)] string DayName,
    int GroupsCount,
    IReadOnlyList<string> GroupNames);
