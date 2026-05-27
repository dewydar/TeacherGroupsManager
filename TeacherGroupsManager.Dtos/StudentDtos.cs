using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public record StudentDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.FullNameRegex, ErrorMessage = "FullNameMustContainThreeNames")] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MobileMaxLength)] string? ParentMobile,
    int AcademicYearId,
    [StringLength(AppConstants.MaxStringLength)] string AcademicYearName,
    int GroupId,
    [StringLength(AppConstants.MaxStringLength)] string GroupName,
    [StringLength(AppConstants.MaxStringLength)] string? Notes,
    bool IsActive,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    [StringLength(AppConstants.MaxStringLength)] string? CreatedByEmployeeName = null,
    [StringLength(AppConstants.MaxStringLength)] string? UpdatedByEmployeeName = null);

public record CreateStudentDto(
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.FullNameRegex, ErrorMessage = "FullNameMustContainThreeNames")] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MobileMaxLength)] string? ParentMobile,
    int AcademicYearId,
    int GroupId,
    [StringLength(AppConstants.MaxStringLength)] string? Notes,
    bool IsActive = true);

public record EditStudentDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength), RegularExpression(AppConstants.FullNameRegex, ErrorMessage = "FullNameMustContainThreeNames")] string FullName,
    [StringLength(AppConstants.MobileMaxLength)] string Mobile,
    [StringLength(AppConstants.MobileMaxLength)] string? ParentMobile,
    int AcademicYearId,
    int GroupId,
    [StringLength(AppConstants.MaxStringLength)] string? Notes,
    bool IsActive = true);

