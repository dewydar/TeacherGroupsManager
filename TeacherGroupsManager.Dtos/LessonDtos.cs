using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Dtos;

public record LessonDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string Title,
    [StringLength(AppConstants.MaxStringLength)] string? Description,
    int GroupId,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    LessonType LessonType,
    DateTime LessonDate,
    decimal Price,
    bool IsMonthlyPaymentRequired,
    int Month,
    int Year,
    int? CreatedByEmployeeId,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    [StringLength(AppConstants.MaxStringLength)] string? CreatedByEmployeeName = null,
    [StringLength(AppConstants.MaxStringLength)] string? UpdatedByEmployeeName = null);

public record AvailableLessonDateDto(
    DateTime LessonDate,
    DayOfWeek DayOfWeek);

public record CreateLessonDto(
    [StringLength(AppConstants.MaxStringLength)] string Title,
    [StringLength(AppConstants.MaxStringLength)] string? Description,
    int GroupId,
    LessonType LessonType,
    DateTime LessonDate,
    decimal Price,
    bool IsMonthlyPaymentRequired,
    int Month,
    int Year,
    int? CreatedByEmployeeId,
    int[]? StudentIds);

public record EditLessonDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string Title,
    [StringLength(AppConstants.MaxStringLength)] string? Description,
    int GroupId,
    LessonType LessonType,
    DateTime LessonDate,
    decimal Price,
    bool IsMonthlyPaymentRequired,
    int Month,
    int Year,
    int? CreatedByEmployeeId,
    int[]? StudentIds);

public record LessonAttendanceStudentDto(
    int StudentId,
    [StringLength(AppConstants.MaxStringLength)] string StudentName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    AttendanceStatus AttendanceStatus,
    PaymentStatus? PaymentStatus,
    [StringLength(AppConstants.MaxStringLength)] string? AttendanceNotes);

public record LessonAttendanceDto(
    int LessonId,
    [StringLength(AppConstants.MaxStringLength)] string LessonTitle,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    DateTime LessonDate,
    IReadOnlyList<LessonAttendanceStudentDto> Students);

public record UpdateLessonAttendanceStudentDto(
    int StudentId,
    AttendanceStatus AttendanceStatus,
    [StringLength(AppConstants.MaxStringLength)] string? AttendanceNotes);

public record UpdateLessonAttendanceDto(
    int LessonId,
    IReadOnlyList<UpdateLessonAttendanceStudentDto> Students);

