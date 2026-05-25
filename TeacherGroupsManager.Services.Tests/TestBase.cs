using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Data.Context;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Services.Mapping;

namespace TeacherGroupsManager.Services.Tests;

public abstract class TestBase
{
    protected static (TeacherGroupsDbContext Context, IMapper Mapper) CreateContext()
    {
        var options = new DbContextOptionsBuilder<TeacherGroupsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new TeacherGroupsDbContext(options);
        context.Database.EnsureCreated();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<AppMappingProfile>()).CreateMapper();
        return (context, mapper);
    }

    protected sealed class TestCurrentUserContext(int employeeId) : ICurrentUserContext
    {
        public int? EmployeeId => employeeId;
    }
}
