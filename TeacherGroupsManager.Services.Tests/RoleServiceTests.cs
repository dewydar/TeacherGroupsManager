using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class RoleServiceTests : TestBase
{
    [Fact]
    public async Task UpdatePermissionsAsync_Updates_Role_Permissions()
    {
        var (context, mapper) = CreateContext();
        var service = new RoleService(new UnitOfWork(context, new TestCurrentUserContext(7)), mapper);

        var result = await service.UpdatePermissionsAsync(3, [5, 9]);

        Assert.True(result.Succeeded);
        var permissions = await context.RolePermissions
            .Where(x => x.RoleId == 3)
            .OrderBy(x => x.PermissionId)
            .ToListAsync();
        Assert.Equal([5, 9], permissions.Select(x => x.PermissionId).ToArray());
        Assert.All(permissions, permission => Assert.Equal(7, permission.CreatedByEmployeeId));
    }

    [Fact]
    public async Task UpdatePermissionsAsync_Fails_When_Permission_Does_Not_Exist()
    {
        var (context, mapper) = CreateContext();
        var service = new RoleService(new UnitOfWork(context), mapper);

        var result = await service.UpdatePermissionsAsync(3, [999]);

        Assert.False(result.Succeeded);
        Assert.Empty(context.RolePermissions.Where(x => x.RoleId == 3));
    }
}
