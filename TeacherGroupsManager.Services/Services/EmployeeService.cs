using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class EmployeeService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<EmployeeDto>>(await EmployeesQuery().OrderBy(x => x.FullName).ToListAsync(cancellationToken));

    public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        mapper.Map<EmployeeDto?>(await EmployeesQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        if (!await unitOfWork.Repository<Role>().AnyAsync(x => x.Id == dto.RoleId, cancellationToken))
        {
            return OperationResult.Failure("الدور غير موجود");
        }

        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Username == dto.Username, cancellationToken))
        {
            return OperationResult.Failure("اسم المستخدم مستخدم من قبل");
        }

        await unitOfWork.Repository<Employee>().AddAsync(new Employee
        {
            FullName = dto.FullName,
            Mobile = dto.Mobile,
            Email = dto.Email,
            Username = dto.Username,
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
        if (!await unitOfWork.Repository<Role>().AnyAsync(x => x.Id == dto.RoleId, cancellationToken))
        {
            return OperationResult.Failure("الدور غير موجود");
        }
        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Username == dto.Username && x.Id != dto.Id, cancellationToken))
        {
            return OperationResult.Failure("اسم المستخدم مستخدم من قبل");
        }

        employee.FullName = dto.FullName;
        employee.Mobile = dto.Mobile;
        employee.Email = dto.Email;
        employee.Username = dto.Username;
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
