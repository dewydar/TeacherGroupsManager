using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Dtos;

public record LoginDto(string Username, string Password, bool RememberMe = false);
public record RoleDto(int Id, string Name, string ArabicName, bool IsActive);
public record PermissionDto(int Id, string Name, string ArabicName, string Code, string ModuleName);
public record EmployeeDto(int Id, string FullName, string Mobile, string? Email, string Username, int RoleId, string RoleName, string RoleArabicName, bool IsActive);
public record CreateEmployeeDto(string FullName, string Mobile, string? Email, string Username, string Password, int RoleId, bool IsActive = true);
public record AcademicYearDto(int Id, string Name, DateOnly StartDate, DateOnly EndDate, bool IsActive);
public record CreateAcademicYearDto(string Name, DateOnly StartDate, DateOnly EndDate, bool IsActive = true);
public record GroupDto(int Id, string Name, int AcademicYearId, string AcademicYearName, GroupType GroupType, int? TeacherId, int? AssistantTeacherId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, decimal DefaultLessonPrice, bool IsActive);
public record CreateGroupDto(string Name, int AcademicYearId, GroupType GroupType, int? TeacherId, int? AssistantTeacherId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime, decimal DefaultLessonPrice, bool IsActive = true);
public record StudentDto(int Id, string FullName, string Mobile, string? ParentMobile, int AcademicYearId, string AcademicYearName, int GroupId, string GroupName, string? Notes, bool IsActive);
public record CreateStudentDto(string FullName, string Mobile, string? ParentMobile, int AcademicYearId, int GroupId, string? Notes, bool IsActive = true);
public record LessonDto(int Id, string Title, string? Description, int GroupId, string GroupName, LessonType LessonType, DateTime LessonDate, decimal Price, bool IsMonthlyPaymentRequired, int Month, int Year, int? CreatedByEmployeeId);
public record CreateLessonDto(string Title, string? Description, int GroupId, LessonType LessonType, DateTime LessonDate, decimal Price, bool IsMonthlyPaymentRequired, int Month, int Year, int? CreatedByEmployeeId, int[] StudentIds);
public record MonthlyPaymentDto(int Id, int StudentId, string StudentName, int GroupId, string GroupName, int AcademicYearId, string AcademicYearName, int Month, int Year, decimal RequiredAmount, decimal PaidAmount, decimal RemainingAmount, PaymentStatus PaymentStatus, DateTime? PaymentDate, string? Notes);
public record CreateMonthlyPaymentDto(int StudentId, int GroupId, int AcademicYearId, int Month, int Year, decimal RequiredAmount, decimal PaidAmount, PaymentStatus PaymentStatus, DateTime? PaymentDate, string? Notes, int? CreatedByEmployeeId);

public record DashboardSummaryDto(int TotalStudents, int TotalGroups, int TotalPrivateGroups, int TotalPublicGroups, int TotalEmployees, int TotalTeachers, int TotalAssistantTeachers, decimal CurrentMonthTotalRequiredAmount, decimal CurrentMonthTotalPaidAmount, decimal CurrentMonthTotalRemainingAmount, int CurrentMonthPaidStudentsCount, int CurrentMonthUnpaidStudentsCount, string StudentsUrl, string GroupsUrl, string PrivateGroupsUrl, string PublicGroupsUrl, string PaidStudentsUrl, string UnpaidStudentsUrl, IReadOnlyList<GroupStudentCountDto> StudentsCountPerGroup, IReadOnlyList<GroupPaymentSummaryDto> PaymentsPerGroup, IReadOnlyList<GroupDayDto> GroupsByDay);
public record GroupStudentCountDto(int GroupId, string GroupName, int StudentsCount, string GroupDetailsUrl);
public record GroupPaymentSummaryDto(int GroupId, string GroupName, int TotalStudents, int PaidStudentsCount, int UnpaidStudentsCount, decimal TotalCollectedAmount, decimal RemainingAmount, string GroupDetailsUrl);
public record GroupDayDto(DayOfWeek DayOfWeek, string DayName, int GroupsCount, IReadOnlyList<string> GroupNames);
