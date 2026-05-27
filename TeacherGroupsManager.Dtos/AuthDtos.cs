using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public record LoginDto(
    [StringLength(AppConstants.MaxStringLength)] string Username,
    [StringLength(AppConstants.MaxStringLength)] string Password,
    bool RememberMe = false);

