using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class GroupService(IUnitOfWork unitOfWork, AppMapper mapper, IStringLocalizer<SharedResource> localizer) : IGroupService
{
    public async Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map(await GroupsQuery().OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToListAsync(cancellationToken));

    public async Task<IReadOnlyList<GroupDto>> GetByAcademicYearAsync(int academicYearId, CancellationToken cancellationToken = default) =>
        mapper.Map(await GroupsQuery()
            .AsNoTracking()
            .Where(x => x.AcademicYearId == academicYearId)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToListAsync(cancellationToken));

    public async Task<GroupDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await GroupsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) is { } group ? mapper.Map(group) : null;

    public Task<DataTableResponseDto<GroupDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default) =>
        DataTableQueryHelper.ToDataTableResponseAsync(
            GroupsQuery().AsNoTracking(),
            request,
            ApplyFilters,
            ApplySearch,
            ApplySorting,
            mapper.Map,
            cancellationToken);

    public async Task<OperationResult> CreateAsync(CreateGroupDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, cancellationToken);
        if (!validation.Succeeded) return validation;
        var name = dto.Name.Trim();
        var normalizedName = name.ToLower();
        if (await unitOfWork.Repository<Group>().AnyAsync(x => x.Name.Trim().ToLower() == normalizedName, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateGroup"]);
        }

        var schedules = NormalizeSchedules(dto.Schedules, dto.DayOfWeek, dto.StartTime, dto.EndTime);
        var scheduleValidation = ValidateSchedules(schedules);
        if (!scheduleValidation.Succeeded) return scheduleValidation;
        var primarySchedule = schedules.First();
        await unitOfWork.Repository<Group>().AddAsync(new Group
        {
            Name = name,
            AcademicYearId = dto.AcademicYearId,
            GroupType = dto.GroupType,
            DayOfWeek = primarySchedule.DayOfWeek,
            StartTime = primarySchedule.StartTime,
            EndTime = primarySchedule.EndTime,
            DefaultLessonPrice = dto.DefaultLessonPrice,
            IsActive = dto.IsActive,
            Schedules = schedules
                .Select(x => new GroupSchedule { DayOfWeek = x.DayOfWeek, StartTime = x.StartTime, EndTime = x.EndTime })
                .ToList()
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["GroupSaved"]);
    }

    public async Task<OperationResult> UpdateAsync(EditGroupDto dto, CancellationToken cancellationToken = default)
    {
        var group = await unitOfWork.Repository<Group>().Query()
            .Include(x => x.Schedules)
            .FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken);
        if (group is null) return OperationResult.Failure(localizer["GroupNotFound"]);
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, cancellationToken);
        if (!validation.Succeeded) return validation;
        var name = dto.Name.Trim();
        var normalizedName = name.ToLower();
        if (await unitOfWork.Repository<Group>().AnyAsync(x => x.Id != dto.Id && x.Name.Trim().ToLower() == normalizedName, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateGroup"]);
        }

        group.Name = name;
        group.AcademicYearId = dto.AcademicYearId;
        group.GroupType = dto.GroupType;
        var schedules = NormalizeSchedules(dto.Schedules, dto.DayOfWeek, dto.StartTime, dto.EndTime);
        var scheduleValidation = ValidateSchedules(schedules);
        if (!scheduleValidation.Succeeded) return scheduleValidation;
        var primarySchedule = schedules.First();
        group.DayOfWeek = primarySchedule.DayOfWeek;
        group.StartTime = primarySchedule.StartTime;
        group.EndTime = primarySchedule.EndTime;
        group.DefaultLessonPrice = dto.DefaultLessonPrice;
        group.IsActive = dto.IsActive;
        group.Schedules.Clear();
        foreach (var schedule in schedules)
        {
            group.Schedules.Add(new GroupSchedule
            {
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime
            });
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["GroupUpdated"]);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var group = await unitOfWork.Repository<Group>().GetByIdAsync(id, cancellationToken);
        if (group is null) return OperationResult.Failure(localizer["GroupNotFound"]);
        unitOfWork.Repository<Group>().Delete(group);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, localizer["GroupDeleted"], localizer["CannotDeleteLinkedRecord"], cancellationToken);
    }

    private IQueryable<Group> GroupsQuery() => unitOfWork.Repository<Group>().Query()
        .Include(x => x.AcademicYear)
        .Include(x => x.Schedules.OrderBy(schedule => schedule.DayOfWeek).ThenBy(schedule => schedule.StartTime))
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private static IQueryable<Group> ApplyFilters(IQueryable<Group> query, DataTableRequestDto request)
    {
        if (request.Filter("name") is { } name) query = query.Where(x => x.Name.Contains(name));
        if (request.FilterInt("academicYearId") is { } academicYearId) query = query.Where(x => x.AcademicYearId == academicYearId);
        if (request.FilterInt("groupType") is { } groupType) query = query.Where(x => (int)x.GroupType == groupType);
        if (request.FilterInt("dayOfWeek") is { } dayOfWeek) query = query.Where(x => x.Schedules.Any(schedule => (int)schedule.DayOfWeek == dayOfWeek) || (!x.Schedules.Any() && (int)x.DayOfWeek == dayOfWeek));
        if (request.FilterBool("isActive") is { } isActive) query = query.Where(x => x.IsActive == isActive);
        return query;
    }

    private static IQueryable<Group> ApplySearch(IQueryable<Group> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        search = search.Trim();
        return query.Where(x => x.Name.Contains(search) || x.AcademicYear.Name.Contains(search));
    }

    private static IQueryable<Group> ApplySorting(IQueryable<Group> query, string? sortColumn, string? sortDirection)
    {
        var desc = DataTableQueryHelper.Descending(sortDirection);
        return sortColumn switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "academicYearName" => desc ? query.OrderByDescending(x => x.AcademicYear.Name) : query.OrderBy(x => x.AcademicYear.Name),
            "groupType" => desc ? query.OrderByDescending(x => x.GroupType) : query.OrderBy(x => x.GroupType),
            "dayOfWeek" => desc ? query.OrderByDescending(x => x.DayOfWeek) : query.OrderBy(x => x.DayOfWeek),
            "startTime" => desc ? query.OrderByDescending(x => x.StartTime) : query.OrderBy(x => x.StartTime),
            "defaultLessonPrice" => desc ? query.OrderByDescending(x => x.DefaultLessonPrice) : query.OrderBy(x => x.DefaultLessonPrice),
            "isActive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => query.OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime)
        };
    }

    private async Task<OperationResult> ValidateReferencesAsync(int academicYearId, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Id == academicYearId, cancellationToken))
        {
            return OperationResult.Failure(localizer["AcademicYearNotFound"]);
        }
        return OperationResult.Success();
    }

    private static List<GroupScheduleDto> NormalizeSchedules(IReadOnlyList<GroupScheduleDto>? schedules, DayOfWeek fallbackDay, TimeOnly fallbackStart, TimeOnly fallbackEnd)
    {
        var normalized = (schedules is { Count: > 0 }
                ? schedules
                : [new GroupScheduleDto(fallbackDay, fallbackStart, fallbackEnd)])
            .GroupBy(x => new { x.DayOfWeek, x.StartTime, x.EndTime })
            .Select(x => x.First())
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToList();

        return normalized.Count > 0
            ? normalized
            : [new GroupScheduleDto(fallbackDay, fallbackStart, fallbackEnd)];
    }

    private OperationResult ValidateSchedules(IReadOnlyList<GroupScheduleDto> schedules)
    {
        if (schedules.Count == 0)
        {
            return OperationResult.Failure(localizer["EndTimeAfterStartTime"]);
        }

        return schedules.Any(x => x.EndTime <= x.StartTime)
            ? OperationResult.Failure(localizer["EndTimeAfterStartTime"])
            : OperationResult.Success();
    }
}
