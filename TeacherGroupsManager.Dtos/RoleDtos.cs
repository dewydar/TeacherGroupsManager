using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public record RoleDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string Name,
    [StringLength(AppConstants.MaxStringLength)] string ArabicName,
    bool IsActive,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    [StringLength(AppConstants.MaxStringLength)] string? CreatedByEmployeeName = null,
    [StringLength(AppConstants.MaxStringLength)] string? UpdatedByEmployeeName = null);

public record PermissionDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string Name,
    [StringLength(AppConstants.MaxStringLength)] string ArabicName,
    [StringLength(AppConstants.MaxStringLength)] string Code,
    [StringLength(AppConstants.MaxStringLength)] string ModuleName,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    [StringLength(AppConstants.MaxStringLength)] string? CreatedByEmployeeName = null,
    [StringLength(AppConstants.MaxStringLength)] string? UpdatedByEmployeeName = null);

public record RolePermissionsDto(
    int RoleId,
    [StringLength(AppConstants.MaxStringLength)] string RoleName,
    [StringLength(AppConstants.MaxStringLength)] string RoleArabicName,
    IReadOnlyList<int> PermissionIds);

