using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class GroupService(IUnitOfWork unitOfWork, IMapper mapper) : IGroupService
{
    public async Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<GroupDto>>(await GroupsQuery().OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToListAsync(cancellationToken));

    public async Task<GroupDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        mapper.Map<GroupDto?>(await GroupsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateGroupDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, dto.TeacherId, dto.AssistantTeacherId, cancellationToken);
        if (!validation.Succeeded) return validation;
        if (await unitOfWork.Repository<Group>().AnyAsync(x => x.Name == dto.Name, cancellationToken))
        {
            return OperationResult.Failure("المجموعة موجودة من قبل");
        }

        await unitOfWork.Repository<Group>().AddAsync(new Group
        {
            Name = dto.Name,
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
        if (await unitOfWork.Repository<Group>().AnyAsync(x => x.Name == dto.Name && x.Id != dto.Id, cancellationToken))
        {
            return OperationResult.Failure("المجموعة موجودة من قبل");
        }

        group.Name = dto.Name;
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
