using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Text.RegularExpressions;
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

public class AuthService(IUnitOfWork unitOfWork, AppMapper mapper, IPasswordHasher passwordHasher, IStringLocalizer<SharedResource> localizer) : IAuthService
{
    public async Task<OperationResult<LoginResultDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var username = dto.Username.Trim();
        var employee = await unitOfWork.Repository<Employee>().Query()
            .Include(x => x.Role)
            .ThenInclude(x => x.RolePermissions)
            .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (employee is null)
        {
            return OperationResult<LoginResultDto>.Failure(localizer["InvalidUsernameOrPassword"]);
        }

        if (!employee.IsActive)
        {
            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                return OperationResult<LoginResultDto>.Success(new LoginResultDto(null, true, employee.Username), localizer["PasswordSetupRequired"]);
            }

            return OperationResult<LoginResultDto>.Failure(localizer["InactiveUserContactAdmin"]);
        }

        if (string.IsNullOrWhiteSpace(employee.PasswordHash))
        {
            return OperationResult<LoginResultDto>.Success(new LoginResultDto(null, true, employee.Username), localizer["PasswordSetupRequired"]);
        }

        if (!passwordHasher.Verify(dto.Password, employee.PasswordHash))
        {
            return OperationResult<LoginResultDto>.Failure(localizer["InvalidUsernameOrPassword"]);
        }

        return OperationResult<LoginResultDto>.Success(new LoginResultDto(mapper.Map(employee), false), localizer["LoginSuccess"]);
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordDto dto, int? currentEmployeeId = null, CancellationToken cancellationToken = default)
    {
        if (dto.NewPassword.Length < 8)
        {
            return OperationResult.Failure(localizer["PasswordMinLength"]);
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return OperationResult.Failure(localizer["PasswordMismatch"]);
        }

        if (!Regex.IsMatch(dto.NewPassword, AppConstants.PasswordRegex))
        {
            return OperationResult.Failure(localizer["PasswordComplexity"]);
        }

        var employee = dto.IsFirstTime && !string.IsNullOrWhiteSpace(dto.Username)
            ? await unitOfWork.Repository<Employee>().Query().FirstOrDefaultAsync(x => x.Username == dto.Username.Trim(), cancellationToken)
            : currentEmployeeId is { } id
                ? await unitOfWork.Repository<Employee>().GetByIdAsync(id, cancellationToken)
                : null;

        if (employee is null)
        {
            return OperationResult.Failure(localizer["InvalidUsernameOrPassword"]);
        }

        if (dto.IsFirstTime)
        {
            if (employee.IsActive || !string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                return OperationResult.Failure(localizer["InactiveUserContactAdmin"]);
            }

            employee.PasswordHash = passwordHasher.Hash(dto.NewPassword);
            employee.IsActive = true;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return OperationResult.Success(localizer["PasswordUpdated"]);
        }

        if (!employee.IsActive)
        {
            return OperationResult.Failure(localizer["InactiveUserContactAdmin"]);
        }

        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || !passwordHasher.Verify(dto.CurrentPassword, employee.PasswordHash))
        {
            return OperationResult.Failure(localizer["CurrentPasswordInvalid"]);
        }

        employee.PasswordHash = passwordHasher.Hash(dto.NewPassword);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["PasswordUpdated"]);
    }
}
