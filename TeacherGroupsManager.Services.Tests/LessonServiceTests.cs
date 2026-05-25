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
        var service = new LessonService(new UnitOfWork(context), mapper);

        var result = await service.CreateAsync(new CreateLessonDto("Unit 1", null, 1, LessonType.Group, DateTime.Today, 150, true, DateTime.Today.Month, DateTime.Today.Year, null, []));

        Assert.True(result.Succeeded);
        Assert.Equal(2, await context.LessonStudents.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Private_Lesson_Student_Does_Not_Exist()
    {
        var (context, mapper) = CreateContext();
        var service = new LessonService(new UnitOfWork(context), mapper);

        var result = await service.CreateAsync(new CreateLessonDto("Private", null, 1, LessonType.Private, DateTime.Today, 150, true, DateTime.Today.Month, DateTime.Today.Year, null, [999]));

        Assert.False(result.Succeeded);
        Assert.Empty(context.Lessons);
    }
}
