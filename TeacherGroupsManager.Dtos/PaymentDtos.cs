using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Dtos;

public record MonthlyPaymentDto(
    int Id,
    int StudentId,
    [StringLength(AppConstants.MaxStringLength)] string StudentName,
    int GroupId,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    int AcademicYearId,
    [StringLength(AppConstants.MaxStringLength)] string AcademicYearName,
    int Month,
    int Year,
    decimal RequiredAmount,
    decimal PaidAmount,
    decimal RemainingAmount,
    PaymentStatus PaymentStatus,
    DateTime? PaymentDate,
    [StringLength(AppConstants.MaxStringLength)] string? Notes,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    [StringLength(AppConstants.MaxStringLength)] string? CreatedByEmployeeName = null,
    [StringLength(AppConstants.MaxStringLength)] string? UpdatedByEmployeeName = null);

public record CreateMonthlyPaymentDto(
    int StudentId,
    int GroupId,
    int AcademicYearId,
    int Month,
    int Year,
    decimal RequiredAmount,
    decimal PaidAmount,
    PaymentStatus PaymentStatus,
    DateTime? PaymentDate,
    [StringLength(AppConstants.MaxStringLength)] string? Notes,
    int? CreatedByEmployeeId);

public record EditMonthlyPaymentDto(
    int Id,
    int StudentId,
    int GroupId,
    int AcademicYearId,
    int Month,
    int Year,
    decimal RequiredAmount,
    decimal PaidAmount,
    PaymentStatus PaymentStatus,
    DateTime? PaymentDate,
    [StringLength(AppConstants.MaxStringLength)] string? Notes,
    int? CreatedByEmployeeId);

