using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public record EmployeeDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.FullNameRegex, ErrorMessage = "FullNameMustContainThreeNames")] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.EmailRegex)] string? Email,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.UsernameRegex, ErrorMessage = "UsernameFormatInvalid")] string Username,
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
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.FullNameRegex, ErrorMessage = "FullNameMustContainThreeNames")] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.EmailRegex)] string? Email,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.UsernameRegex, ErrorMessage = "UsernameFormatInvalid")] string Username,
    int RoleId,
    bool IsActive = false);

public record EditEmployeeDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.FullNameRegex, ErrorMessage = "FullNameMustContainThreeNames")] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.EmailRegex)] string? Email,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.UsernameRegex, ErrorMessage = "UsernameFormatInvalid")] string Username,
    int RoleId,
    bool IsActive = true);

