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
    int[] StudentIds);

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
    int[] StudentIds);

