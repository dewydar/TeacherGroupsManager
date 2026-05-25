using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.Services.Services;

public class PermissionService(IUnitOfWork unitOfWork, IMapper mapper) : IPermissionService
{
    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<PermissionDto>>(await unitOfWork.Repository<Permission>().Query().OrderBy(x => x.ModuleName).ToListAsync(cancellationToken));
}
