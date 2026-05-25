using System.Security.Claims;
using TeacherGroupsManager.Data.Repositories;

namespace TeacherGroupsManager.WebUI.Security;

public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public int? EmployeeId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var employeeId) ? employeeId : null;
        }
    }
}
