using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Dtos;

public record GroupScheduleDto(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

public record GroupDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string Name,
    int AcademicYearId,
    [StringLength(AppConstants.MaxStringLength)] string AcademicYearName,
    GroupType GroupType,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal DefaultLessonPrice,
    bool IsActive,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null,
    [StringLength(AppConstants.MaxStringLength)] string? CreatedByEmployeeName = null,
    [StringLength(AppConstants.MaxStringLength)] string? UpdatedByEmployeeName = null,
    IReadOnlyList<GroupScheduleDto>? Schedules = null);

public record CreateGroupDto(
    [StringLength(AppConstants.MaxStringLength)] string Name,
    int AcademicYearId,
    GroupType GroupType,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal DefaultLessonPrice,
    bool IsActive = true,
    List<GroupScheduleDto>? Schedules = null);

public record EditGroupDto(
    int Id,
    [StringLength(AppConstants.MaxStringLength)] string Name,
    int AcademicYearId,
    GroupType GroupType,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal DefaultLessonPrice,
    bool IsActive = true,
    List<GroupScheduleDto>? Schedules = null);

