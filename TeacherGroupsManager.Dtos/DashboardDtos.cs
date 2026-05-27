using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public record DashboardSummaryDto(
    int TotalStudents,
    int TotalGroups,
    int TotalPrivateGroups,
    int TotalPublicGroups,
    int TotalEmployees,
    int TotalTeachers,
    int TotalAssistantTeachers,
    decimal CurrentMonthTotalRequiredAmount,
    decimal CurrentMonthTotalPaidAmount,
    decimal CurrentMonthTotalRemainingAmount,
    int CurrentMonthPaidStudentsCount,
    int CurrentMonthUnpaidStudentsCount,
    [StringLength(AppConstants.MaxStringLength)] string StudentsUrl,
    [StringLength(AppConstants.MaxStringLength)] string GroupsUrl,
    [StringLength(AppConstants.MaxStringLength)] string PrivateGroupsUrl,
    [StringLength(AppConstants.MaxStringLength)] string PublicGroupsUrl,
    [StringLength(AppConstants.MaxStringLength)] string PaidStudentsUrl,
    [StringLength(AppConstants.MaxStringLength)] string UnpaidStudentsUrl,
    IReadOnlyList<GroupStudentCountDto> StudentsCountPerGroup,
    IReadOnlyList<GroupPaymentSummaryDto> PaymentsPerGroup,
    IReadOnlyList<GroupDayDto> GroupsByDay);

public record GroupStudentCountDto(
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
    decimal TotalCollectedAmount,
    decimal RemainingAmount,
    [StringLength(AppConstants.MaxStringLength)] string GroupDetailsUrl);

public record GroupDayDto(
    DayOfWeek DayOfWeek,
    [StringLength(AppConstants.MaxStringLength)] string DayName,
    int GroupsCount,
    IReadOnlyList<string> GroupNames);

