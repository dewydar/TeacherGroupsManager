using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class LessonService(IUnitOfWork unitOfWork, IMapper mapper) : ILessonService
{
    public async Task<IReadOnlyList<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<LessonDto>>(await LessonsQuery().OrderByDescending(x => x.LessonDate).ToListAsync(cancellationToken));

    public async Task<LessonDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        mapper.Map<LessonDto?>(await LessonsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateLessonDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateReferencesAsync(dto.GroupId, dto.LessonType, dto.StudentIds, cancellationToken);
        if (!validation.Succeeded) return validation;

        var lesson = new Lesson { Title = dto.Title, Description = dto.Description, GroupId = dto.GroupId, LessonType = dto.LessonType, LessonDate = dto.LessonDate, Price = dto.Price, IsMonthlyPaymentRequired = dto.IsMonthlyPaymentRequired, Month = dto.Month, Year = dto.Year, CreatedByEmployeeId = dto.CreatedByEmployeeId };
        await SetLessonStudentsAsync(lesson, dto.LessonType, dto.GroupId, dto.StudentIds, cancellationToken);
        await unitOfWork.Repository<Lesson>().AddAsync(lesson, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ الدرس بنجاح");
    }

    public async Task<OperationResult> UpdateAsync(EditLessonDto dto, CancellationToken cancellationToken = default)
    {
        var lesson = await unitOfWork.Repository<Lesson>().Query().Include(x => x.LessonStudents).FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken);
        if (lesson is null) return OperationResult.Failure("الدرس غير موجود");
        var validation = await ValidateReferencesAsync(dto.GroupId, dto.LessonType, dto.StudentIds, cancellationToken);
        if (!validation.Succeeded) return validation;

        lesson.Title = dto.Title;
        lesson.Description = dto.Description;
        lesson.GroupId = dto.GroupId;
        lesson.LessonType = dto.LessonType;
        lesson.LessonDate = dto.LessonDate;
        lesson.Price = dto.Price;
        lesson.IsMonthlyPaymentRequired = dto.IsMonthlyPaymentRequired;
        lesson.Month = dto.Month;
        lesson.Year = dto.Year;
        lesson.CreatedByEmployeeId = dto.CreatedByEmployeeId;
        lesson.LessonStudents.Clear();
        await SetLessonStudentsAsync(lesson, dto.LessonType, dto.GroupId, dto.StudentIds, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم تعديل الدرس بنجاح");
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var lesson = await unitOfWork.Repository<Lesson>().GetByIdAsync(id, cancellationToken);
        if (lesson is null) return OperationResult.Failure("الدرس غير موجود");
        unitOfWork.Repository<Lesson>().Delete(lesson);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, "تم حذف الدرس بنجاح", cancellationToken);
    }

    private IQueryable<Lesson> LessonsQuery() => unitOfWork.Repository<Lesson>().Query()
        .Include(x => x.Group)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private async Task SetLessonStudentsAsync(Lesson lesson, LessonType lessonType, int groupId, IEnumerable<int> studentIds, CancellationToken cancellationToken)
    {
        var ids = lessonType == LessonType.Private
            ? studentIds.Distinct().ToList()
            : await unitOfWork.Repository<Student>().Query().Where(x => x.GroupId == groupId && x.IsActive).Select(x => x.Id).ToListAsync(cancellationToken);

        foreach (var studentId in ids)
        {
            lesson.LessonStudents.Add(new LessonStudent { LessonId = lesson.Id, StudentId = studentId });
        }
    }

    private async Task<OperationResult> ValidateReferencesAsync(int groupId, LessonType lessonType, int[] studentIds, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<Group>().AnyAsync(x => x.Id == groupId, cancellationToken))
        {
            return OperationResult.Failure("المجموعة غير موجودة");
        }
        if (lessonType == LessonType.Private)
        {
            var distinctStudentIds = studentIds.Distinct().ToArray();
            var existingStudentCount = await unitOfWork.Repository<Student>().Query().CountAsync(x => distinctStudentIds.Contains(x.Id), cancellationToken);
            if (existingStudentCount != distinctStudentIds.Length)
            {
                return OperationResult.Failure("يوجد طلاب غير موجودين");
            }
        }
        return OperationResult.Success();
    }
}
