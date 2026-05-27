using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class LessonService(IUnitOfWork unitOfWork, AppMapper mapper, IStringLocalizer<SharedResource> localizer) : ILessonService
{
    public async Task<IReadOnlyList<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map(await LessonsQuery().OrderByDescending(x => x.LessonDate).ToListAsync(cancellationToken));

    public async Task<IReadOnlyList<AvailableLessonDateDto>> GetAvailableLessonDatesAsync(int groupId, int month, int year, DayOfWeek? dayOfWeek = null, CancellationToken cancellationToken = default)
    {
        if (groupId <= 0 || month is < 1 or > 12 || year <= 2000)
        {
            return [];
        }

        var group = await unitOfWork.Repository<Group>().Query()
            .Include(x => x.Schedules)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == groupId, cancellationToken);

        if (group is null)
        {
            return [];
        }

        var schedules = (group.Schedules.Count > 0
                ? group.Schedules.Select(x => new { x.DayOfWeek, x.StartTime })
                : [new { group.DayOfWeek, group.StartTime }])
            .Where(x => dayOfWeek is null || x.DayOfWeek == dayOfWeek)
            .GroupBy(x => new { x.DayOfWeek, x.StartTime })
            .Select(x => x.Key)
            .ToList();

        if (schedules.Count == 0)
        {
            return [];
        }

        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var existingLessonDates = await unitOfWork.Repository<Lesson>().Query()
            .Where(x => x.GroupId == groupId && x.LessonDate >= monthStart && x.LessonDate < monthEnd)
            .Select(x => x.LessonDate.Date)
            .Distinct()
            .ToListAsync(cancellationToken);
        var existingDateSet = existingLessonDates.ToHashSet();

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dates = new List<AvailableLessonDateDto>();
        foreach (var schedule in schedules)
        {
            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                if (date.DayOfWeek != schedule.DayOfWeek || existingDateSet.Contains(date))
                {
                    continue;
                }

                dates.Add(new AvailableLessonDateDto(date.Add(schedule.StartTime.ToTimeSpan()), schedule.DayOfWeek));
            }
        }

        return dates
            .OrderBy(x => x.LessonDate)
            .ToList();
    }

    public async Task<LessonDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await LessonsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) is { } lesson ? mapper.Map(lesson) : null;

    public async Task<LessonAttendanceDto?> GetAttendanceAsync(int id, CancellationToken cancellationToken = default)
    {
        var lesson = await unitOfWork.Repository<Lesson>().Query()
            .Include(x => x.Group)
            .Include(x => x.LessonStudents)
            .ThenInclude(x => x.Student)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return lesson is null
            ? null
            : new LessonAttendanceDto(
                lesson.Id,
                lesson.Title,
                lesson.Group.Name,
                lesson.LessonDate,
                lesson.LessonStudents
                    .OrderBy(x => x.Student.FullName)
                    .Select(x => new LessonAttendanceStudentDto(
                        x.StudentId,
                        x.Student.FullName,
                        x.Student.Mobile,
                        x.AttendanceStatus,
                        x.AttendanceNotes))
                    .ToList());
    }

    public Task<DataTableResponseDto<LessonDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default) =>
        DataTableQueryHelper.ToDataTableResponseAsync(
            LessonsQuery().AsNoTracking(),
            request,
            ApplyFilters,
            ApplySearch,
            ApplySorting,
            mapper.Map,
            cancellationToken);

    public async Task<OperationResult> CreateAsync(CreateLessonDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateReferencesAsync(dto.GroupId, dto.LessonType, dto.StudentIds, cancellationToken);
        if (!validation.Succeeded) return validation;
        var title = dto.Title.Trim();
        var normalizedTitle = title.ToLower();
        var lessonDate = dto.LessonDate.Date;
        if (await unitOfWork.Repository<Lesson>().AnyAsync(x => x.GroupId == dto.GroupId && x.LessonDate.Date == lessonDate && x.Title.Trim().ToLower() == normalizedTitle, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateLesson"]);
        }

        var lesson = new Lesson { Title = title, Description = dto.Description?.Trim(), GroupId = dto.GroupId, LessonType = dto.LessonType, LessonDate = dto.LessonDate, Price = dto.Price, IsMonthlyPaymentRequired = dto.IsMonthlyPaymentRequired, Month = dto.Month, Year = dto.Year, CreatedByEmployeeId = dto.CreatedByEmployeeId };
        await SetLessonStudentsAsync(lesson, dto.LessonType, dto.GroupId, dto.StudentIds, cancellationToken);
        await unitOfWork.Repository<Lesson>().AddAsync(lesson, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["LessonSaved"]);
    }

    public async Task<OperationResult> UpdateAsync(EditLessonDto dto, CancellationToken cancellationToken = default)
    {
        var lesson = await unitOfWork.Repository<Lesson>().Query().Include(x => x.LessonStudents).FirstOrDefaultAsync(x => x.Id == dto.Id, cancellationToken);
        if (lesson is null) return OperationResult.Failure(localizer["LessonNotFound"]);
        var validation = await ValidateReferencesAsync(dto.GroupId, dto.LessonType, dto.StudentIds, cancellationToken);
        if (!validation.Succeeded) return validation;
        var title = dto.Title.Trim();
        var normalizedTitle = title.ToLower();
        var lessonDate = dto.LessonDate.Date;
        if (await unitOfWork.Repository<Lesson>().AnyAsync(x => x.Id != dto.Id && x.GroupId == dto.GroupId && x.LessonDate.Date == lessonDate && x.Title.Trim().ToLower() == normalizedTitle, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateLesson"]);
        }

        lesson.Title = title;
        lesson.Description = dto.Description?.Trim();
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
        return OperationResult.Success(localizer["LessonUpdated"]);
    }

    public async Task<OperationResult> UpdateAttendanceAsync(UpdateLessonAttendanceDto dto, CancellationToken cancellationToken = default)
    {
        var lesson = await unitOfWork.Repository<Lesson>().Query()
            .Include(x => x.LessonStudents)
            .FirstOrDefaultAsync(x => x.Id == dto.LessonId, cancellationToken);

        if (lesson is null) return OperationResult.Failure(localizer["LessonNotFound"]);

        var lessonStudentsById = lesson.LessonStudents.ToDictionary(x => x.StudentId);
        var requestedStudentIds = dto.Students.Select(x => x.StudentId).Distinct().ToArray();
        if (requestedStudentIds.Length != lessonStudentsById.Count || requestedStudentIds.Any(x => !lessonStudentsById.ContainsKey(x)))
        {
            return OperationResult.Failure(localizer["SomeStudentsNotFound"]);
        }

        foreach (var item in dto.Students)
        {
            var lessonStudent = lessonStudentsById[item.StudentId];
            lessonStudent.AttendanceStatus = item.AttendanceStatus;
            lessonStudent.AttendanceNotes = string.IsNullOrWhiteSpace(item.AttendanceNotes) ? null : item.AttendanceNotes.Trim();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["AttendanceUpdated"]);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var lesson = await unitOfWork.Repository<Lesson>().GetByIdAsync(id, cancellationToken);
        if (lesson is null) return OperationResult.Failure(localizer["LessonNotFound"]);
        unitOfWork.Repository<Lesson>().Delete(lesson);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, localizer["LessonDeleted"], localizer["CannotDeleteLinkedRecord"], cancellationToken);
    }

    private IQueryable<Lesson> LessonsQuery() => unitOfWork.Repository<Lesson>().Query()
        .Include(x => x.Group)
        .ThenInclude(x => x.AcademicYear)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private static IQueryable<Lesson> ApplyFilters(IQueryable<Lesson> query, DataTableRequestDto request)
    {
        if (request.Filter("title") is { } title) query = query.Where(x => x.Title.Contains(title));
        if (request.FilterInt("academicYearId") is { } academicYearId) query = query.Where(x => x.Group.AcademicYearId == academicYearId);
        if (request.FilterInt("groupId") is { } groupId) query = query.Where(x => x.GroupId == groupId);
        if (request.FilterInt("lessonType") is { } lessonType) query = query.Where(x => (int)x.LessonType == lessonType);
        if (request.FilterDateTime("lessonDateFrom") is { } from) query = query.Where(x => x.LessonDate.Date >= from);
        if (request.FilterDateTime("lessonDateTo") is { } to) query = query.Where(x => x.LessonDate.Date <= to);
        if (request.FilterInt("month") is { } month) query = query.Where(x => x.Month == month);
        if (request.FilterInt("year") is { } year) query = query.Where(x => x.Year == year);
        return query;
    }

    private static IQueryable<Lesson> ApplySearch(IQueryable<Lesson> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        search = search.Trim();
        return query.Where(x => x.Title.Contains(search) || x.Group.Name.Contains(search));
    }

    private static IQueryable<Lesson> ApplySorting(IQueryable<Lesson> query, string? sortColumn, string? sortDirection)
    {
        var desc = DataTableQueryHelper.Descending(sortDirection);
        return sortColumn switch
        {
            "title" => desc ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            "groupName" => desc ? query.OrderByDescending(x => x.Group.Name) : query.OrderBy(x => x.Group.Name),
            "lessonType" => desc ? query.OrderByDescending(x => x.LessonType) : query.OrderBy(x => x.LessonType),
            "lessonDate" => desc ? query.OrderByDescending(x => x.LessonDate) : query.OrderBy(x => x.LessonDate),
            "price" => desc ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
            "month" => desc ? query.OrderByDescending(x => x.Month) : query.OrderBy(x => x.Month),
            "year" => desc ? query.OrderByDescending(x => x.Year) : query.OrderBy(x => x.Year),
            _ => query.OrderByDescending(x => x.LessonDate)
        };
    }

    private async Task SetLessonStudentsAsync(Lesson lesson, LessonType lessonType, int groupId, IEnumerable<int>? studentIds, CancellationToken cancellationToken)
    {
        var ids = lessonType == LessonType.Private
            ? (studentIds ?? []).Distinct().ToList()
            : await unitOfWork.Repository<Student>().Query().Where(x => x.GroupId == groupId && x.IsActive).Select(x => x.Id).ToListAsync(cancellationToken);

        foreach (var studentId in ids)
        {
            lesson.LessonStudents.Add(new LessonStudent { LessonId = lesson.Id, StudentId = studentId });
        }
    }

    private async Task<OperationResult> ValidateReferencesAsync(int groupId, LessonType lessonType, int[]? studentIds, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Repository<Group>().AnyAsync(x => x.Id == groupId, cancellationToken))
        {
            return OperationResult.Failure(localizer["GroupNotFound"]);
        }
        if (lessonType == LessonType.Private)
        {
            var distinctStudentIds = (studentIds ?? []).Distinct().ToArray();
            var existingStudentCount = await unitOfWork.Repository<Student>().Query().CountAsync(x => distinctStudentIds.Contains(x.Id), cancellationToken);
            if (existingStudentCount != distinctStudentIds.Length)
            {
                return OperationResult.Failure(localizer["SomeStudentsNotFound"]);
            }
        }
        return OperationResult.Success();
    }
}
