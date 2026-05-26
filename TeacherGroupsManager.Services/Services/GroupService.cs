using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class GroupService(IUnitOfWork unitOfWork, AppMapper mapper) : IGroupService
{
    public async Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map(await GroupsQuery().OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToListAsync(cancellationToken));

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
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, dto.TeacherId, dto.AssistantTeacherId, cancellationToken);
        if (!validation.Succeeded) return validation;
        var name = dto.Name.Trim();
        var normalizedName = name.ToLower();
        if (await unitOfWork.Repository<Group>().AnyAsync(x => x.Name.Trim().ToLower() == normalizedName, cancellationToken))
        {
            return OperationResult.Failure("المجموعة موجودة من قبل");
        }

        await unitOfWork.Repository<Group>().AddAsync(new Group
        {
            Name = name,
            AcademicYearId = dto.AcademicYearId,
            GroupType = dto.GroupType,
            TeacherId = dto.TeacherId,
            AssistantTeacherId = dto.AssistantTeacherId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            DefaultLessonPrice = dto.DefaultLessonPrice,
            IsActive = dto.IsActive
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ المجموعة بنجاح");
    }

    public async Task<OperationResult> UpdateAsync(EditGroupDto dto, CancellationToken cancellationToken = default)
    {
        var group = await unitOfWork.Repository<Group>().GetByIdAsync(dto.Id, cancellationToken);
        if (group is null) return OperationResult.Failure("المجموعة غير موجودة");
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, dto.TeacherId, dto.AssistantTeacherId, cancellationToken);
        if (!validation.Succeeded) return validation;
        var name = dto.Name.Trim();
        var normalizedName = name.ToLower();
        if (await unitOfWork.Repository<Group>().AnyAsync(x => x.Id != dto.Id && x.Name.Trim().ToLower() == normalizedName, cancellationToken))
        {
            return OperationResult.Failure("المجموعة موجودة من قبل");
        }

        group.Name = name;
        group.AcademicYearId = dto.AcademicYearId;
        group.GroupType = dto.GroupType;
        group.TeacherId = dto.TeacherId;
        group.AssistantTeacherId = dto.AssistantTeacherId;
        group.DayOfWeek = dto.DayOfWeek;
        group.StartTime = dto.StartTime;
        group.EndTime = dto.EndTime;
        group.DefaultLessonPrice = dto.DefaultLessonPrice;
        group.IsActive = dto.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم تعديل المجموعة بنجاح");
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var group = await unitOfWork.Repository<Group>().GetByIdAsync(id, cancellationToken);
        if (group is null) return OperationResult.Failure("المجموعة غير موجودة");
        unitOfWork.Repository<Group>().Delete(group);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, "تم حذف المجموعة بنجاح", cancellationToken);
    }

    private IQueryable<Group> GroupsQuery() => unitOfWork.Repository<Group>().Query()
        .Include(x => x.AcademicYear)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private static IQueryable<Group> ApplyFilters(IQueryable<Group> query, DataTableRequestDto request)
    {
        if (request.Filter("name") is { } name) query = query.Where(x => x.Name.Contains(name));
        if (request.FilterInt("academicYearId") is { } academicYearId) query = query.Where(x => x.AcademicYearId == academicYearId);
        if (request.FilterInt("groupType") is { } groupType) query = query.Where(x => (int)x.GroupType == groupType);
        if (request.FilterInt("teacherId") is { } teacherId) query = query.Where(x => x.TeacherId == teacherId);
        if (request.FilterInt("assistantTeacherId") is { } assistantTeacherId) query = query.Where(x => x.AssistantTeacherId == assistantTeacherId);
        if (request.FilterInt("dayOfWeek") is { } dayOfWeek) query = query.Where(x => (int)x.DayOfWeek == dayOfWeek);
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

    private async Task<OperationResult> ValidateReferencesAsync(int academicYearId, int? teacherId, int? assistantTeacherId, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Id == academicYearId, cancellationToken))
        {
            return OperationResult.Failure("السنة الدراسية غير موجودة");
        }
        if (teacherId.HasValue && !await unitOfWork.Repository<Employee>().AnyAsync(x => x.Id == teacherId.Value, cancellationToken))
        {
            return OperationResult.Failure("المدرس غير موجود");
        }
        if (assistantTeacherId.HasValue && !await unitOfWork.Repository<Employee>().AnyAsync(x => x.Id == assistantTeacherId.Value, cancellationToken))
        {
            return OperationResult.Failure("المدرس المساعد غير موجود");
        }
        return OperationResult.Success();
    }
}
