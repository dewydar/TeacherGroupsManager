using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class EmployeeService(IUnitOfWork unitOfWork, AppMapper mapper, IPasswordHasher passwordHasher) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map(await EmployeesQuery().OrderBy(x => x.FullName).ToListAsync(cancellationToken));

    public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await EmployeesQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) is { } employee ? mapper.Map(employee) : null;

    public async Task<OperationResult> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var username = dto.Username.Trim();
        var mobile = dto.Mobile.Trim();
        var normalizedUsername = username.ToLower();

        if (!await unitOfWork.Repository<Role>().AnyAsync(x => x.Id == dto.RoleId, cancellationToken))
        {
            return OperationResult.Failure("الدور غير موجود");
        }

        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Username.Trim().ToLower() == normalizedUsername, cancellationToken))
        {
            return OperationResult.Failure("اسم المستخدم مستخدم من قبل");
        }

        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Mobile.Trim() == mobile, cancellationToken))
        {
            return OperationResult.Failure("رقم الموبايل مستخدم من قبل");
        }

        await unitOfWork.Repository<Employee>().AddAsync(new Employee
        {
            FullName = dto.FullName.Trim(),
            Mobile = mobile,
            Email = dto.Email?.Trim(),
            Username = username,
            PasswordHash = passwordHasher.Hash(dto.Password),
            RoleId = dto.RoleId,
            IsActive = dto.IsActive
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ الموظف بنجاح");
    }

    public async Task<OperationResult> UpdateAsync(EditEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await unitOfWork.Repository<Employee>().GetByIdAsync(dto.Id, cancellationToken);
        if (employee is null) return OperationResult.Failure("الموظف غير موجود");
        var username = dto.Username.Trim();
        var mobile = dto.Mobile.Trim();
        var normalizedUsername = username.ToLower();
        if (!await unitOfWork.Repository<Role>().AnyAsync(x => x.Id == dto.RoleId, cancellationToken))
        {
            return OperationResult.Failure("الدور غير موجود");
        }
        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Id != dto.Id && x.Username.Trim().ToLower() == normalizedUsername, cancellationToken))
        {
            return OperationResult.Failure("اسم المستخدم مستخدم من قبل");
        }

        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Id != dto.Id && x.Mobile.Trim() == mobile, cancellationToken))
        {
            return OperationResult.Failure("رقم الموبايل مستخدم من قبل");
        }

        employee.FullName = dto.FullName.Trim();
        employee.Mobile = mobile;
        employee.Email = dto.Email?.Trim();
        employee.Username = username;
        employee.RoleId = dto.RoleId;
        employee.IsActive = dto.IsActive;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            employee.PasswordHash = passwordHasher.Hash(dto.Password);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم تعديل الموظف بنجاح");
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await unitOfWork.Repository<Employee>().GetByIdAsync(id, cancellationToken);
        if (employee is null) return OperationResult.Failure("الموظف غير موجود");
        unitOfWork.Repository<Employee>().Delete(employee);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, "تم حذف الموظف بنجاح", cancellationToken);
    }

    private IQueryable<Employee> EmployeesQuery() => unitOfWork.Repository<Employee>().Query()
        .Include(x => x.Role)
        .ThenInclude(x => x.RolePermissions)
        .ThenInclude(x => x.Permission)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);
}
