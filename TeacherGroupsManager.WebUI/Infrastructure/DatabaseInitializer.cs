using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Context;
using TeacherGroupsManager.Services.Security;

namespace TeacherGroupsManager.WebUI.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TeacherGroupsDbContext>();

        await dbContext.Database.MigrateAsync();
        await SeedDefaultAdminAsync(scope.ServiceProvider, dbContext);
    }

    private static async Task SeedDefaultAdminAsync(IServiceProvider services, TeacherGroupsDbContext dbContext)
    {
        if (await dbContext.Employees.AnyAsync(x => x.Username == "admin"))
        {
            return;
        }

        var adminRole = await dbContext.Roles.SingleAsync(x => x.Name == AppConstants.AdminRole);
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        dbContext.Employees.Add(new Employee
        {
            FullName = "System Admin",
            Mobile = "0000000000",
            Username = "admin",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            RoleId = adminRole.Id,
            IsActive = true
        });

        await dbContext.SaveChangesAsync();
    }
}
