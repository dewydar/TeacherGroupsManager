using TeacherGroupsManager.Core.Common;

namespace TeacherGroupsManager.Core.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
