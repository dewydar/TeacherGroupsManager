using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class EmployeeServiceTests : TestBase
{
    [Fact]
    public async Task UpdateAsync_Changes_Role_And_Preserves_Password()
    {
        var (context, mapper) = CreateContext();
        var employee = new Employee
        {
            FullName = "Old Employee Name",
            Mobile = "1",
            Username = "employee",
            PasswordHash = "hash",
            RoleId = 2
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var originalHash = employee.PasswordHash;
        var service = new EmployeeService(new UnitOfWork(context, new TestCurrentUserContext(1)), mapper, TestLocalizer.Instance);

        var result = await service.UpdateAsync(new EditEmployeeDto(employee.Id, "New Employee Name", "2", "new@example.com", "employee2", 3, false));

        Assert.True(result.Succeeded);
        employee = await context.Employees.SingleAsync(x => x.Id == employee.Id);
        Assert.Equal("New Employee Name", employee.FullName);
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
        var service = new EmployeeService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateEmployeeDto("Test User Name", "1", null, "user", 999));

        Assert.False(result.Succeeded);
        Assert.Empty(context.Employees);
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Username_Already_Exists_With_Different_Case()
    {
        var (context, mapper) = CreateContext();
        var service = new EmployeeService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var first = await service.CreateAsync(new CreateEmployeeDto("User One Name", "1", null, "employee", 2));
        var duplicate = await service.CreateAsync(new CreateEmployeeDto("User Two Name", "2", null, "EMPLOYEE", 2));

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Single(context.Employees);
    }

    [Theory]
    [InlineData("employee one")]
    [InlineData(" employee")]
    [InlineData("employee ")]
    [InlineData("employee_one")]
    [InlineData("employee.one")]
    public async Task CreateAsync_Fails_When_Username_Format_Is_Invalid(string username)
    {
        var (context, mapper) = CreateContext();
        var service = new EmployeeService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateEmployeeDto("User One Name", "1", null, username, 2));

        Assert.False(result.Succeeded);
        Assert.Contains("UsernameFormatInvalid", result.Errors);
        Assert.Empty(context.Employees);
    }

    [Fact]
    public async Task CreateAsync_Allows_Username_With_Letters_Numbers_And_Hyphen()
    {
        var (context, mapper) = CreateContext();
        var service = new EmployeeService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateEmployeeDto("User One Name", "1", null, "employee-123", 2));

        Assert.True(result.Succeeded);
        Assert.Equal("employee-123", (await context.Employees.SingleAsync()).Username);
    }

    [Fact]
    public async Task GetAllAsync_Excludes_System_Admin()
    {
        var (context, mapper) = CreateContext();
        context.Employees.AddRange(
            new Employee { FullName = "System Admin User", Mobile = "0", Username = AppConstants.SystemAdminUsername, PasswordHash = "hash", RoleId = 1 },
            new Employee { FullName = "Visible Test User", Mobile = "1", Username = "visible", PasswordHash = "hash", RoleId = 2 });
        await context.SaveChangesAsync();
        var service = new EmployeeService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var employees = await service.GetAllAsync();

        Assert.DoesNotContain(employees, x => x.Username == AppConstants.SystemAdminUsername);
        Assert.Contains(employees, x => x.Username == "visible");
    }

    [Fact]
    public async Task DeleteAsync_Fails_For_System_Admin()
    {
        var (context, mapper) = CreateContext();
        var admin = new Employee { FullName = "System Admin User", Mobile = "0", Username = AppConstants.SystemAdminUsername, PasswordHash = "hash", RoleId = 1 };
        context.Employees.Add(admin);
        await context.SaveChangesAsync();
        var service = new EmployeeService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.DeleteAsync(admin.Id);

        Assert.False(result.Succeeded);
        Assert.True(await context.Employees.AnyAsync(x => x.Id == admin.Id));
    }
}


