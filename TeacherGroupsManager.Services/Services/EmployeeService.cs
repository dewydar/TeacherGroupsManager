using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class EmployeeService(IUnitOfWork unitOfWork, AppMapper mapper, IPasswordHasher passwordHasher, IStringLocalizer<SharedResource> localizer) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map(await VisibleEmployeesQuery().OrderBy(x => x.FullName).ToListAsync(cancellationToken));

    public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await VisibleEmployeesQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken) is { } employee ? mapper.Map(employee) : null;

    public Task<DataTableResponseDto<EmployeeDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default) =>
        DataTableQueryHelper.ToDataTableResponseAsync(
            VisibleEmployeesQuery().AsNoTracking(),
            request,
            ApplyFilters,
            ApplySearch,
            ApplySorting,
            mapper.Map,
            cancellationToken);

    public async Task<OperationResult> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var username = dto.Username.Trim();
        var mobile = dto.Mobile.Trim();
        var normalizedUsername = username.ToLower();
        if (IsSystemAdminUsername(normalizedUsername))
        {
            return OperationResult.Failure(localizer["SystemAdminProtected"]);
        }

        if (!await unitOfWork.Repository<Role>().AnyAsync(x => x.Id == dto.RoleId, cancellationToken))
        {
            return OperationResult.Failure(localizer["RoleNotFound"]);
        }

        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Username.Trim().ToLower() == normalizedUsername, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateUsername"]);
        }

        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Mobile.Trim() == mobile, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateMobile"]);
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
        return OperationResult.Success(localizer["EmployeeSaved"]);
    }

    public async Task<OperationResult> UpdateAsync(EditEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await unitOfWork.Repository<Employee>().GetByIdAsync(dto.Id, cancellationToken);
        if (employee is null) return OperationResult.Failure(localizer["EmployeeNotFound"]);
        if (IsSystemAdmin(employee)) return OperationResult.Failure(localizer["SystemAdminProtected"]);
        var username = dto.Username.Trim();
        var mobile = dto.Mobile.Trim();
        var normalizedUsername = username.ToLower();
        if (IsSystemAdminUsername(normalizedUsername))
        {
            return OperationResult.Failure(localizer["SystemAdminProtected"]);
        }

        if (!await unitOfWork.Repository<Role>().AnyAsync(x => x.Id == dto.RoleId, cancellationToken))
        {
            return OperationResult.Failure(localizer["RoleNotFound"]);
        }
        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Id != dto.Id && x.Username.Trim().ToLower() == normalizedUsername, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateUsername"]);
        }

        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Id != dto.Id && x.Mobile.Trim() == mobile, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateMobile"]);
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
        return OperationResult.Success(localizer["EmployeeUpdated"]);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await unitOfWork.Repository<Employee>().GetByIdAsync(id, cancellationToken);
        if (employee is null) return OperationResult.Failure(localizer["EmployeeNotFound"]);
        if (IsSystemAdmin(employee)) return OperationResult.Failure(localizer["SystemAdminProtected"]);
        unitOfWork.Repository<Employee>().Delete(employee);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, localizer["EmployeeDeleted"], localizer["CannotDeleteLinkedRecord"], cancellationToken);
    }

    private IQueryable<Employee> EmployeesQuery() => unitOfWork.Repository<Employee>().Query()
        .Include(x => x.Role)
        .ThenInclude(x => x.RolePermissions)
        .ThenInclude(x => x.Permission)
        .Include(x => x.CreatedByEmployee)
        .Include(x => x.UpdatedByEmployee);

    private IQueryable<Employee> VisibleEmployeesQuery() =>
        EmployeesQuery().Where(x => x.Username.ToLower() != AppConstants.SystemAdminUsername);

    private static bool IsSystemAdmin(Employee employee) =>
        IsSystemAdminUsername(employee.Username.Trim().ToLower());

    private static bool IsSystemAdminUsername(string normalizedUsername) =>
        normalizedUsername == AppConstants.SystemAdminUsername;

    private static IQueryable<Employee> ApplyFilters(IQueryable<Employee> query, DataTableRequestDto request)
    {
        if (request.Filter("fullName") is { } fullName) query = query.Where(x => x.FullName.Contains(fullName));
        if (request.Filter("username") is { } username) query = query.Where(x => x.Username.Contains(username));
        if (request.Filter("mobile") is { } mobile) query = query.Where(x => x.Mobile.Contains(mobile));
        if (request.Filter("email") is { } email) query = query.Where(x => x.Email != null && x.Email.Contains(email));
        if (request.FilterInt("roleId") is { } roleId) query = query.Where(x => x.RoleId == roleId);
        if (request.FilterBool("isActive") is { } isActive) query = query.Where(x => x.IsActive == isActive);
        return query;
    }

    private static IQueryable<Employee> ApplySearch(IQueryable<Employee> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        search = search.Trim();
        return query.Where(x =>
            x.FullName.Contains(search) ||
            x.Username.Contains(search) ||
            x.Mobile.Contains(search) ||
            (x.Email != null && x.Email.Contains(search)) ||
            x.Role.Name.Contains(search) ||
            x.Role.ArabicName.Contains(search));
    }

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortColumn, string? sortDirection)
    {
        var desc = DataTableQueryHelper.Descending(sortDirection);
        return sortColumn switch
        {
            "fullName" => desc ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "mobile" => desc ? query.OrderByDescending(x => x.Mobile) : query.OrderBy(x => x.Mobile),
            "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "username" => desc ? query.OrderByDescending(x => x.Username) : query.OrderBy(x => x.Username),
            "roleArabicName" => desc ? query.OrderByDescending(x => x.Role.ArabicName) : query.OrderBy(x => x.Role.ArabicName),
            "isActive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => query.OrderBy(x => x.FullName)
        };
    }
}
