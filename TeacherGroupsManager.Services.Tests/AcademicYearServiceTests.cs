using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class AcademicYearServiceTests : TestBase
{
    [Fact]
    public async Task CreateAsync_Fails_When_Academic_Year_Name_Already_Exists_With_Different_Case_Or_Spaces()
    {
        var (context, mapper) = CreateContext();
        var service = new AcademicYearService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var first = await service.CreateAsync(new CreateAcademicYearDto("Year 2026", new DateOnly(2026, 9, 1), new DateOnly(2027, 6, 30)));
        var duplicate = await service.CreateAsync(new CreateAcademicYearDto(" year 2026 ", new DateOnly(2027, 9, 1), new DateOnly(2028, 6, 30)));

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Equal(3, await context.AcademicYears.CountAsync());
    }
}


