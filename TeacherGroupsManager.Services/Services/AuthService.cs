using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class AuthService(IUnitOfWork unitOfWork, AppMapper mapper, IPasswordHasher passwordHasher, IStringLocalizer<SharedResource> localizer) : IAuthService
{
    public async Task<OperationResult<EmployeeDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await unitOfWork.Repository<Employee>().Query()
            .Include(x => x.Role)
            .ThenInclude(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Username == dto.Username && x.IsActive, cancellationToken);

        if (employee is null || !passwordHasher.Verify(dto.Password, employee.PasswordHash))
        {
            return OperationResult<EmployeeDto>.Failure(localizer["InvalidUsernameOrPassword"]);
        }

        return OperationResult<EmployeeDto>.Success(mapper.Map(employee), localizer["LoginSuccess"]);
    }
}
