using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public record EmployeeDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.EmailRegex)] string? Email,
    [StringLength(AppConstants.MaxStringLength)] string Username,
    int RoleId,
    [StringLength(AppConstants.MaxStringLength)] string RoleName,
    [StringLength(AppConstants.MaxStringLength)] string RoleArabicName,
    bool IsActive,
    IReadOnlyList<string> Permissions,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    [StringLength(AppConstants.MaxStringLength)] string? CreatedByEmployeeName = null,
    [StringLength(AppConstants.MaxStringLength)] string? UpdatedByEmployeeName = null);

public record CreateEmployeeDto(
    [StringLength(AppConstants.MaxStringLength)] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.EmailRegex)] string? Email,
    [StringLength(AppConstants.MaxStringLength)] string Username,
    [StringLength(AppConstants.MaxStringLength)] string Password,
    int RoleId,
    bool IsActive = true);

public record EditEmployeeDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.EmailRegex)] string? Email,
    [StringLength(AppConstants.MaxStringLength)] string Username,
    [StringLength(AppConstants.MaxStringLength)] string? Password,
    int RoleId,
    bool IsActive = true);

