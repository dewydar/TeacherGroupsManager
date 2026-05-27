using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public record AcademicYearDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    [StringLength(AppConstants.MaxStringLength)] string? CreatedByEmployeeName = null,
    [StringLength(AppConstants.MaxStringLength)] string? UpdatedByEmployeeName = null);

public record CreateAcademicYearDto(
    [StringLength(AppConstants.MaxStringLength)] string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive = true);

public record EditAcademicYearDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive = true);

