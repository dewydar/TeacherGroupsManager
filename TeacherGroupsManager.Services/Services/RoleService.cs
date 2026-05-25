using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class RoleService(IUnitOfWork unitOfWork, IMapper mapper) : IRoleService
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<RoleDto>>(await unitOfWork.Repository<Role>().Query().OrderBy(x => x.Id).ToListAsync(cancellationToken));

    public async Task<RoleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        mapper.Map<RoleDto?>(await unitOfWork.Repository<Role>().GetByIdAsync(id, cancellationToken));

    public async Task<OperationResult> CreateAsync(RoleDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.Name.Trim();
        var arabicName = dto.ArabicName.Trim();
        var normalizedName = name.ToLower();
        var normalizedArabicName = arabicName.ToLower();
        if (await unitOfWork.Repository<Role>().AnyAsync(x =>
            x.Name.Trim().ToLower() == normalizedName ||
            x.ArabicName.Trim().ToLower() == normalizedArabicName, cancellationToken))
        {
            return OperationResult.Failure("الدور موجود من قبل");
        }

        await unitOfWork.Repository<Role>().AddAsync(new Role { Name = name, ArabicName = arabicName, IsActive = dto.IsActive }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ الدور بنجاح");
    }

    public async Task<OperationResult> UpdateAsync(RoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await unitOfWork.Repository<Role>().GetByIdAsync(dto.Id, cancellationToken);
        if (role is null) return OperationResult.Failure("الدور غير موجود");
        var name = dto.Name.Trim();
        var arabicName = dto.ArabicName.Trim();
        var normalizedName = name.ToLower();
        var normalizedArabicName = arabicName.ToLower();
        if (await unitOfWork.Repository<Role>().AnyAsync(x =>
            x.Id != dto.Id &&
            (x.Name.Trim().ToLower() == normalizedName ||
             x.ArabicName.Trim().ToLower() == normalizedArabicName), cancellationToken))
        {
            return OperationResult.Failure("الدور موجود من قبل");
        }

        role.Name = name;
        role.ArabicName = arabicName;
        role.IsActive = dto.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم تعديل الدور بنجاح");
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await unitOfWork.Repository<Role>().Query().Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null) return OperationResult.Failure("الدور غير موجود");
        if (role.Employees.Count != 0) return OperationResult.Failure("لا يمكن حذف دور مرتبط بموظفين");
        unitOfWork.Repository<Role>().Delete(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حذف الدور بنجاح");
    }

    public async Task<RolePermissionsDto?> GetPermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await unitOfWork.Repository<Role>().Query()
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        return role is null ? null : new RolePermissionsDto(role.Id, role.Name, role.ArabicName, role.RolePermissions.Select(x => x.PermissionId).ToList());
    }

    public async Task<OperationResult> UpdatePermissionsAsync(int roleId, int[] permissionIds, CancellationToken cancellationToken = default)
    {
        var role = await unitOfWork.Repository<Role>().Query()
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        if (role is null) return OperationResult.Failure("الدور غير موجود");

        var distinctPermissionIds = permissionIds.Distinct().ToArray();
        var existingPermissionIds = await unitOfWork.Repository<Permission>().Query()
            .Where(x => distinctPermissionIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        if (existingPermissionIds.Count != distinctPermissionIds.Length)
        {
            return OperationResult.Failure("توجد صلاحيات غير موجودة");
        }

        role.RolePermissions.Clear();
        foreach (var permissionId in distinctPermissionIds)
        {
            role.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم تحديث صلاحيات الدور بنجاح");
    }
}
