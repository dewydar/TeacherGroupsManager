using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;

namespace TeacherGroupsManager.Services.Services;

public class PermissionService(IUnitOfWork unitOfWork, AppMapper mapper) : IPermissionService
{
    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map(await unitOfWork.Repository<Permission>().Query().OrderBy(x => x.ModuleName).ToListAsync(cancellationToken));
}
