using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class StudentService(IUnitOfWork unitOfWork, IMapper mapper) : IStudentService
{
    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<StudentDto>>(await StudentsQuery().OrderBy(x => x.FullName).ToListAsync(cancellationToken));

    public async Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        mapper.Map<StudentDto?>(await StudentsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, dto.GroupId, cancellationToken);
        if (!validation.Succeeded) return validation;

        await unitOfWork.Repository<Student>().AddAsync(new Student { FullName = dto.FullName, Mobile = dto.Mobile, ParentMobile = dto.ParentMobile, AcademicYearId = dto.AcademicYearId, GroupId = dto.GroupId, Notes = dto.Notes, IsActive = dto.IsActive }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ الطالب بنجاح");
    }

    public async Task<OperationResult> UpdateAsync(EditStudentDto dto, CancellationToken cancellationToken = default)
    {
        var student = await unitOfWork.Repository<Student>().GetByIdAsync(dto.Id, cancellationToken);
        if (student is null) return OperationResult.Failure("الطالب غير موجود");
        var validation = await ValidateReferencesAsync(dto.AcademicYearId, dto.GroupId, cancellationToken);
        if (!validation.Succeeded) return validation;

        student.FullName = dto.FullName;
        student.Mobile = dto.Mobile;
        student.ParentMobile = dto.ParentMobile;
        student.AcademicYearId = dto.AcademicYearId;
        student.GroupId = dto.GroupId;
        student.Notes = dto.Notes;
        student.IsActive = dto.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم تعديل الطالب بنجاح");
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await unitOfWork.Repository<Student>().GetByIdAsync(id, cancellationToken);
        if (student is null) return OperationResult.Failure("الطالب غير موجود");
        unitOfWork.Repository<Student>().Delete(student);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, "تم حذف الطالب بنجاح", cancellationToken);
    }

    private IQueryable<Student> StudentsQuery() => unitOfWork.Repository<Student>().Query()
        .Include(x => x.Group)
        .Include(x => x.AcademicYear)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private async Task<OperationResult> ValidateReferencesAsync(int academicYearId, int groupId, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Id == academicYearId, cancellationToken))
        {
            return OperationResult.Failure("السنة الدراسية غير موجودة");
        }
        if (!await unitOfWork.Repository<Group>().AnyAsync(x => x.Id == groupId, cancellationToken))
        {
            return OperationResult.Failure("المجموعة غير موجودة");
        }
        return OperationResult.Success();
    }
}
