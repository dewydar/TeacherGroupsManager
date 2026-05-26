using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Data.Context;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Services.Mapping;

namespace TeacherGroupsManager.Services.Tests;

public abstract class TestBase
{
    protected static (TeacherGroupsDbContext Context, AppMapper Mapper) CreateContext()
    {
        var options = new DbContextOptionsBuilder<TeacherGroupsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new TeacherGroupsDbContext(options);
        context.Database.EnsureCreated();
        return (context, new AppMapper());
    }

    protected sealed class TestCurrentUserContext(int employeeId) : ICurrentUserContext
    {
        public int? EmployeeId => employeeId;
    }
}
