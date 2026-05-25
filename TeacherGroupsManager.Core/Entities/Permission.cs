using TeacherGroupsManager.Core.Common;

namespace TeacherGroupsManager.Core.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
