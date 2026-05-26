using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class DashboardServiceTests : TestBase
{
    [Fact]
    public async Task GetSummaryAsync_Returns_Current_Counts()
    {
        var (context, _) = CreateContext();
        context.Students.Add(new Student { FullName = "Student", Mobile = "1", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();

        var dashboard = await new DashboardService(new UnitOfWork(context)).GetSummaryAsync();

        Assert.True(dashboard.TotalGroups >= 2);
        Assert.Equal(1, dashboard.TotalStudents);
    }
}


