using TeacherGroupsManager.Core.Common;

namespace TeacherGroupsManager.Core.Entities;

public class RolePermission : IAuditableEntity
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByEmployeeId { get; set; }
    public Employee? CreatedByEmployee { get; set; }
    public int? UpdatedByEmployeeId { get; set; }
    public Employee? UpdatedByEmployee { get; set; }
}
