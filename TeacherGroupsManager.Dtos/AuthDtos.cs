using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public record LoginDto(
    [StringLength(AppConstants.MaxStringLength)] string Username,
    [StringLength(AppConstants.MaxStringLength)] string Password,
    bool RememberMe = false);

public record LoginResultDto(
    EmployeeDto? Employee,
    bool RequiresPasswordSetup,
    [StringLength(AppConstants.MaxStringLength)] string? Username = null);

public record ResetPasswordDto(
    [StringLength(AppConstants.MaxStringLength)] string? Username,
    [StringLength(AppConstants.MaxStringLength)] string? CurrentPassword,
    [Required, StringLength(AppConstants.MaxStringLength), MinLength(8), RegularExpression(AppConstants.PasswordRegex)] string NewPassword,
    [Required, StringLength(AppConstants.MaxStringLength)] string ConfirmPassword,
    bool IsFirstTime);

