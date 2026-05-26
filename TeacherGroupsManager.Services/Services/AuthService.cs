using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class AuthService(IUnitOfWork unitOfWork, AppMapper mapper, IPasswordHasher passwordHasher) : IAuthService
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
            return OperationResult<EmployeeDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        return OperationResult<EmployeeDto>.Success(mapper.Map(employee), "تم تسجيل الدخول بنجاح");
    }
}
