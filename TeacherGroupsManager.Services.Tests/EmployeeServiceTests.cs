using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class EmployeeServiceTests : TestBase
{
    [Fact]
    public async Task UpdateAsync_Changes_Role_And_Preserves_Password_When_Blank()
    {
        var (context, mapper) = CreateContext();
        var hasher = new Pbkdf2PasswordHasher();
        var employee = new Employee
        {
            FullName = "Old Name",
            Mobile = "1",
            Username = "employee",
            PasswordHash = hasher.Hash("OldPassword123"),
            RoleId = 2
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var originalHash = employee.PasswordHash;
        var service = new EmployeeService(new UnitOfWork(context, new TestCurrentUserContext(1)), mapper, hasher);

        var result = await service.UpdateAsync(new EditEmployeeDto(employee.Id, "New Name", "2", "new@example.com", "employee2", null, 3, false));

        Assert.True(result.Succeeded);
        employee = await context.Employees.SingleAsync(x => x.Id == employee.Id);
        Assert.Equal("New Name", employee.FullName);
        Assert.Equal("employee2", employee.Username);
        Assert.Equal(3, employee.RoleId);
        Assert.False(employee.IsActive);
        Assert.Equal(originalHash, employee.PasswordHash);
        Assert.Equal(1, employee.UpdatedByEmployeeId);
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Role_Does_Not_Exist()
    {
        var (context, mapper) = CreateContext();
        var service = new EmployeeService(new UnitOfWork(context), mapper, new Pbkdf2PasswordHasher());

        var result = await service.CreateAsync(new CreateEmployeeDto("User", "1", null, "user", "Password123", 999));

        Assert.False(result.Succeeded);
        Assert.Empty(context.Employees);
    }
}
