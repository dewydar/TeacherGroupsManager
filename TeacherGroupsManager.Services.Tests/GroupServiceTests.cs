using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class GroupServiceTests : TestBase
{
    [Fact]
    public async Task CreateAsync_Creates_Group()
    {
        var (context, mapper) = CreateContext();
        var service = new GroupService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateGroupDto("Test Group", 1, GroupType.Public, null, null, DayOfWeek.Saturday, new TimeOnly(18, 0), new TimeOnly(20, 0), 150));

        Assert.True(result.Succeeded);
        Assert.Equal(3, await context.Groups.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_Fails_When_AcademicYear_Does_Not_Exist()
    {
        var (context, mapper) = CreateContext();
        var service = new GroupService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateGroupDto("Missing Year", 999, GroupType.Public, null, null, DayOfWeek.Saturday, new TimeOnly(18, 0), new TimeOnly(20, 0), 150));

        Assert.False(result.Succeeded);
        Assert.Equal(2, await context.Groups.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Group_Name_Already_Exists_With_Different_Case_Or_Spaces()
    {
        var (context, mapper) = CreateContext();
        var service = new GroupService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var first = await service.CreateAsync(new CreateGroupDto("Test Group", 1, GroupType.Public, null, null, DayOfWeek.Saturday, new TimeOnly(18, 0), new TimeOnly(20, 0), 150));
        var duplicate = await service.CreateAsync(new CreateGroupDto(" test group ", 1, GroupType.Public, null, null, DayOfWeek.Sunday, new TimeOnly(18, 0), new TimeOnly(20, 0), 150));

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Equal(3, await context.Groups.CountAsync());
    }
}


