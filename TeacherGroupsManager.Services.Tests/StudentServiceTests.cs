using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class StudentServiceTests : TestBase
{
    [Fact]
    public async Task CreateAsync_Creates_Student()
    {
        var (context, mapper) = CreateContext();
        var service = new StudentService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateStudentDto("Ahmed Mohamed", "01000000000", "01011111111", 1, 1, null));

        Assert.True(result.Succeeded);
        Assert.Equal("Ahmed Mohamed", (await context.Students.FirstAsync()).FullName);
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Group_Does_Not_Exist()
    {
        var (context, mapper) = CreateContext();
        var service = new StudentService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateStudentDto("Missing Group", "01000000000", null, 1, 999, null));

        Assert.False(result.Succeeded);
        Assert.Empty(context.Students);
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Student_Mobile_Already_Exists()
    {
        var (context, mapper) = CreateContext();
        var service = new StudentService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var first = await service.CreateAsync(new CreateStudentDto("Ahmed Mohamed", "01000000000", null, 1, 1, null));
        var duplicate = await service.CreateAsync(new CreateStudentDto("Different Student", "01000000000", null, 1, 1, null));

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Single(context.Students);
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Student_Name_Already_Exists_In_Same_Group()
    {
        var (context, mapper) = CreateContext();
        var service = new StudentService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var first = await service.CreateAsync(new CreateStudentDto("Ahmed Mohamed", "01000000000", null, 1, 1, null));
        var duplicate = await service.CreateAsync(new CreateStudentDto(" ahmed mohamed ", "01000000001", null, 1, 1, null));

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Single(context.Students);
    }

    [Fact]
    public async Task UpdateAsync_Stamps_Audit_Fields()
    {
        var (context, mapper) = CreateContext();
        var unitOfWork = new UnitOfWork(context, new TestCurrentUserContext(42));
        var service = new StudentService(unitOfWork, mapper, TestLocalizer.Instance);

        var create = await service.CreateAsync(new CreateStudentDto("Audit Student", "01000000000", null, 1, 1, null));

        Assert.True(create.Succeeded);
        var student = await context.Students.SingleAsync(x => x.FullName == "Audit Student");
        Assert.NotNull(student.CreatedAt);
        Assert.Equal(42, student.CreatedByEmployeeId);
        Assert.Null(student.UpdatedAt);
        Assert.Null(student.UpdatedByEmployeeId);

        var update = await service.UpdateAsync(new EditStudentDto(student.Id, "Audit Student Updated", "01000000001", null, 1, 1, "Updated", true));

        Assert.True(update.Succeeded);
        student = await context.Students.SingleAsync(x => x.Id == student.Id);
        Assert.Equal("Audit Student Updated", student.FullName);
        Assert.NotNull(student.UpdatedAt);
        Assert.Equal(42, student.UpdatedByEmployeeId);
    }
}


