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

public class StudentService(IUnitOfWork unitOfWork, AppMapper mapper, IStringLocalizer<SharedResource> localizer) : IStudentService
{
    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map(await StudentsQuery().OrderBy(x => x.FullName).ToListAsync(cancellationToken));

    public async Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await StudentsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) is { } student ? mapper.Map(student) : null;

    public Task<DataTableResponseDto<StudentDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default) =>
        DataTableQueryHelper.ToDataTableResponseAsync(
            StudentsQuery().AsNoTracking(),
            request,
            ApplyFilters,
            ApplySearch,
            ApplySorting,
            mapper.Map,
            cancellationToken);

    public async Task<OperationResult> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, dto.GroupId, cancellationToken);
        if (!validation.Succeeded) return validation;
        var fullName = dto.FullName.Trim();
        var mobile = dto.Mobile.Trim();
        var normalizedFullName = fullName.ToLower();
        if (await unitOfWork.Repository<Student>().AnyAsync(x => x.Mobile.Trim() == mobile, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateMobile"]);
        }
        if (await unitOfWork.Repository<Student>().AnyAsync(x => x.GroupId == dto.GroupId && x.FullName.Trim().ToLower() == normalizedFullName, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateStudent"]);
        }

        await unitOfWork.Repository<Student>().AddAsync(new Student { FullName = fullName, Mobile = mobile, ParentMobile = dto.ParentMobile?.Trim(), AcademicYearId = dto.AcademicYearId, GroupId = dto.GroupId, Notes = dto.Notes?.Trim(), IsActive = dto.IsActive }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["StudentSaved"]);
    }

    public async Task<OperationResult> UpdateAsync(EditStudentDto dto, CancellationToken cancellationToken = default)
    {
        var student = await unitOfWork.Repository<Student>().GetByIdAsync(dto.Id, cancellationToken);
        if (student is null) return OperationResult.Failure(localizer["StudentNotFound"]);
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, dto.GroupId, cancellationToken);
        if (!validation.Succeeded) return validation;
        var fullName = dto.FullName.Trim();
        var mobile = dto.Mobile.Trim();
        var normalizedFullName = fullName.ToLower();
        if (await unitOfWork.Repository<Student>().AnyAsync(x => x.Id != dto.Id && x.Mobile.Trim() == mobile, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateMobile"]);
        }
        if (await unitOfWork.Repository<Student>().AnyAsync(x => x.Id != dto.Id && x.GroupId == dto.GroupId && x.FullName.Trim().ToLower() == normalizedFullName, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateStudent"]);
        }

        student.FullName = fullName;
        student.Mobile = mobile;
        student.ParentMobile = dto.ParentMobile?.Trim();
        student.AcademicYearId = dto.AcademicYearId;
        student.GroupId = dto.GroupId;
        student.Notes = dto.Notes?.Trim();
        student.IsActive = dto.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["StudentUpdated"]);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await unitOfWork.Repository<Student>().GetByIdAsync(id, cancellationToken);
        if (student is null) return OperationResult.Failure(localizer["StudentNotFound"]);
        unitOfWork.Repository<Student>().Delete(student);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, localizer["StudentDeleted"], localizer["CannotDeleteLinkedRecord"], cancellationToken);
    }

    private IQueryable<Student> StudentsQuery() => unitOfWork.Repository<Student>().Query()
        .Include(x => x.Group)
        .Include(x => x.AcademicYear)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private static IQueryable<Student> ApplyFilters(IQueryable<Student> query, DataTableRequestDto request)
    {
        if (request.Filter("fullName") is { } fullName) query = query.Where(x => x.FullName.Contains(fullName));
        if (request.Filter("mobile") is { } mobile) query = query.Where(x => x.Mobile.Contains(mobile));
        if (request.Filter("parentMobile") is { } parentMobile) query = query.Where(x => x.ParentMobile != null && x.ParentMobile.Contains(parentMobile));
        if (request.FilterInt("academicYearId") is { } academicYearId) query = query.Where(x => x.AcademicYearId == academicYearId);
        if (request.FilterInt("groupId") is { } groupId) query = query.Where(x => x.GroupId == groupId);
        if (request.FilterBool("isActive") is { } isActive) query = query.Where(x => x.IsActive == isActive);
        return query;
    }

    private static IQueryable<Student> ApplySearch(IQueryable<Student> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        search = search.Trim();
        return query.Where(x =>
            x.FullName.Contains(search) ||
            x.Mobile.Contains(search) ||
            (x.ParentMobile != null && x.ParentMobile.Contains(search)) ||
            x.AcademicYear.Name.Contains(search) ||
            x.Group.Name.Contains(search));
    }

    private static IQueryable<Student> ApplySorting(IQueryable<Student> query, string? sortColumn, string? sortDirection)
    {
        var desc = DataTableQueryHelper.Descending(sortDirection);
        return sortColumn switch
        {
            "fullName" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "mobile" => desc ? query.OrderByDescending(x => x.Mobile) : query.OrderBy(x => x.Mobile),
            "parentMobile" => desc ? query.OrderByDescending(x => x.ParentMobile) : query.OrderBy(x => x.ParentMobile),
            "academicYearName" => desc ? query.OrderByDescending(x => x.AcademicYear.Name) : query.OrderBy(x => x.AcademicYear.Name),
            "groupName" => desc ? query.OrderByDescending(x => x.Group.Name) : query.OrderBy(x => x.Group.Name),
            "isActive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => query.OrderBy(x => x.FullName)
        };
    }

    private async Task<OperationResult> ValidateReferencesAsync(int academicYearId, int groupId, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Id == academicYearId, cancellationToken))
        {
            return OperationResult.Failure(localizer["AcademicYearNotFound"]);
        }
        if (!await unitOfWork.Repository<Group>().AnyAsync(x => x.Id == groupId && x.AcademicYearId == academicYearId, cancellationToken))
        {
            return OperationResult.Failure(localizer["GroupNotFound"]);
        }
        return OperationResult.Success();
    }
}
