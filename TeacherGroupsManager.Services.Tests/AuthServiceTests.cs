using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class AuthServiceTests : TestBase
{
    [Fact]
    public async Task LoginAsync_Inactive_User_Without_Password_Requires_Setup()
    {
        var (context, mapper) = CreateContext();
        context.Employees.Add(new Employee { FullName = "New User", Mobile = "1", Username = "newuser", PasswordHash = string.Empty, RoleId = 2, IsActive = false });
        await context.SaveChangesAsync();
        var service = CreateService(context, mapper);

        var result = await service.LoginAsync(new LoginDto("newuser", "anything"));

        Assert.True(result.Succeeded);
        Assert.True(result.Data?.RequiresPasswordSetup);
        Assert.Equal("newuser", result.Data?.Username);
    }

    [Fact]
    public async Task LoginAsync_Inactive_User_With_Password_Is_Blocked()
    {
        var (context, mapper) = CreateContext();
        var hasher = new Pbkdf2PasswordHasher();
        context.Employees.Add(new Employee { FullName = "Blocked User", Mobile = "1", Username = "blocked", PasswordHash = hasher.Hash("Password@123"), RoleId = 2, IsActive = false });
        await context.SaveChangesAsync();
        var service = CreateService(context, mapper, hasher);

        var result = await service.LoginAsync(new LoginDto("blocked", "Password@123"));

        Assert.False(result.Succeeded);
        Assert.Contains("InactiveUserContactAdmin", result.Errors);
    }

    [Fact]
    public async Task ResetPasswordAsync_First_Time_Sets_Password_And_Activates_User()
    {
        var (context, mapper) = CreateContext();
        context.Employees.Add(new Employee { FullName = "New User", Mobile = "1", Username = "newuser", PasswordHash = string.Empty, RoleId = 2, IsActive = false });
        await context.SaveChangesAsync();
        var hasher = new Pbkdf2PasswordHasher();
        var service = CreateService(context, mapper, hasher);

        var result = await service.ResetPasswordAsync(new ResetPasswordDto("newuser", null, "Password@123", "Password@123", true));

        Assert.True(result.Succeeded);
        var employee = await context.Employees.SingleAsync(x => x.Username == "newuser");
        Assert.True(employee.IsActive);
        Assert.True(hasher.Verify("Password@123", employee.PasswordHash));
    }

    [Fact]
    public async Task ResetPasswordAsync_Change_Password_Requires_Current_Password()
    {
        var (context, mapper) = CreateContext();
        var hasher = new Pbkdf2PasswordHasher();
        var employee = new Employee { FullName = "Active User", Mobile = "1", Username = "active", PasswordHash = hasher.Hash("Password@123"), RoleId = 2, IsActive = true };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        var service = CreateService(context, mapper, hasher);

        var result = await service.ResetPasswordAsync(new ResetPasswordDto(null, "wrong", "NewPassword@123", "NewPassword@123", false), employee.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("CurrentPasswordInvalid", result.Errors);
    }

    private static AuthService CreateService(TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context, AppMapper mapper, IPasswordHasher? hasher = null) =>
        new(new UnitOfWork(context), mapper, hasher ?? new Pbkdf2PasswordHasher(), TestLocalizer.Instance);
}
