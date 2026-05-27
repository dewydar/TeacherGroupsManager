using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class LessonServiceTests : TestBase
{
    [Fact]
    public async Task CreateAsync_Assigns_Group_Lesson_To_All_Group_Students()
    {
        var (context, mapper) = CreateContext();
        context.Students.AddRange(
            new Student { FullName = "Student 1", Mobile = "1", AcademicYearId = 1, GroupId = 1 },
            new Student { FullName = "Student 2", Mobile = "2", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();
        var service = new LessonService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateLessonDto("Unit 1", null, 1, LessonType.Group, DateTime.Today, 150, true, DateTime.Today.Month, DateTime.Today.Year, null, []));

        Assert.True(result.Succeeded);
        Assert.Equal(2, await context.LessonStudents.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_Allows_Group_Lesson_Without_Posted_StudentIds()
    {
        var (context, mapper) = CreateContext();
        context.Students.Add(new Student { FullName = "Student 1", Mobile = "1", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();
        var service = new LessonService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateLessonDto("Unit 1", null, 1, LessonType.Group, DateTime.Today, 150, true, DateTime.Today.Month, DateTime.Today.Year, null, null));

        Assert.True(result.Succeeded);
        Assert.Single(await context.LessonStudents.ToListAsync());
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Private_Lesson_Student_Does_Not_Exist()
    {
        var (context, mapper) = CreateContext();
        var service = new LessonService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateLessonDto("Private", null, 1, LessonType.Private, DateTime.Today, 150, true, DateTime.Today.Month, DateTime.Today.Year, null, [999]));

        Assert.False(result.Succeeded);
        Assert.Empty(context.Lessons);
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Lesson_Title_Group_And_Date_Already_Exist()
    {
        var (context, mapper) = CreateContext();
        var service = new LessonService(new UnitOfWork(context), mapper, TestLocalizer.Instance);
        var lessonDate = DateTime.Today;

        var first = await service.CreateAsync(new CreateLessonDto("Unit 1", null, 1, LessonType.Group, lessonDate, 150, true, lessonDate.Month, lessonDate.Year, null, []));
        var duplicate = await service.CreateAsync(new CreateLessonDto(" unit 1 ", null, 1, LessonType.Group, lessonDate.AddHours(2), 150, true, lessonDate.Month, lessonDate.Year, null, []));

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Single(context.Lessons);
    }

    [Fact]
    public async Task GetAvailableLessonDatesAsync_Returns_Group_Days_Without_Existing_Lessons()
    {
        var (context, mapper) = CreateContext();
        var service = new LessonService(new UnitOfWork(context), mapper, TestLocalizer.Instance);
        var monthStart = new DateTime(2026, 5, 1);
        var firstSaturday = Enumerable.Range(0, DateTime.DaysInMonth(monthStart.Year, monthStart.Month))
            .Select(offset => monthStart.AddDays(offset))
            .First(date => date.DayOfWeek == DayOfWeek.Saturday);
        context.Lessons.Add(new Lesson
        {
            Title = "Existing",
            GroupId = 1,
            LessonType = LessonType.Group,
            LessonDate = firstSaturday,
            Price = 150,
            IsMonthlyPaymentRequired = true,
            Month = monthStart.Month,
            Year = monthStart.Year
        });
        await context.SaveChangesAsync();

        var dates = await service.GetAvailableLessonDatesAsync(1, monthStart.Month, monthStart.Year, DayOfWeek.Saturday);

        Assert.DoesNotContain(dates, x => x.LessonDate.Date == firstSaturday.Date);
        Assert.All(dates, x => Assert.Equal(DayOfWeek.Saturday, x.DayOfWeek));
        Assert.NotEmpty(dates);
    }

    [Fact]
    public async Task GetAttendanceAsync_Returns_Lesson_Roster_With_Default_Present_Status()
    {
        var (context, mapper) = CreateContext();
        context.Students.AddRange(
            new Student { FullName = "Student 1", Mobile = "1", AcademicYearId = 1, GroupId = 1 },
            new Student { FullName = "Student 2", Mobile = "2", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();
        var service = new LessonService(new UnitOfWork(context), mapper, TestLocalizer.Instance);
        await service.CreateAsync(new CreateLessonDto("Unit 1", null, 1, LessonType.Group, DateTime.Today, 150, true, DateTime.Today.Month, DateTime.Today.Year, null, []));
        var lessonId = await context.Lessons.Select(x => x.Id).SingleAsync();

        var attendance = await service.GetAttendanceAsync(lessonId);

        Assert.NotNull(attendance);
        Assert.Equal(2, attendance.Students.Count);
        Assert.All(attendance.Students, student => Assert.Equal(AttendanceStatus.Present, student.AttendanceStatus));
    }

    [Fact]
    public async Task UpdateAttendanceAsync_Saves_Status_And_Notes()
    {
        var (context, mapper) = CreateContext();
        context.Students.AddRange(
            new Student { FullName = "Student 1", Mobile = "1", AcademicYearId = 1, GroupId = 1 },
            new Student { FullName = "Student 2", Mobile = "2", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();
        var service = new LessonService(new UnitOfWork(context), mapper, TestLocalizer.Instance);
        await service.CreateAsync(new CreateLessonDto("Unit 1", null, 1, LessonType.Group, DateTime.Today, 150, true, DateTime.Today.Month, DateTime.Today.Year, null, []));
        var lesson = await context.Lessons.Include(x => x.LessonStudents).SingleAsync();
        var firstStudentId = lesson.LessonStudents.Select(x => x.StudentId).Min();
        var secondStudentId = lesson.LessonStudents.Select(x => x.StudentId).Max();

        var result = await service.UpdateAttendanceAsync(new UpdateLessonAttendanceDto(lesson.Id, [
            new UpdateLessonAttendanceStudentDto(firstStudentId, AttendanceStatus.Absent, "Called parent"),
            new UpdateLessonAttendanceStudentDto(secondStudentId, AttendanceStatus.Excused, " Sick ")
        ]));

        Assert.True(result.Succeeded);
        var updated = await context.LessonStudents.AsNoTracking().ToDictionaryAsync(x => x.StudentId);
        Assert.Equal(AttendanceStatus.Absent, updated[firstStudentId].AttendanceStatus);
        Assert.Equal("Called parent", updated[firstStudentId].AttendanceNotes);
        Assert.Equal(AttendanceStatus.Excused, updated[secondStudentId].AttendanceStatus);
        Assert.Equal("Sick", updated[secondStudentId].AttendanceNotes);
    }
}


