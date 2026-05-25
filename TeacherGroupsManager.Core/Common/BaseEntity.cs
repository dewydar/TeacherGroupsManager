namespace TeacherGroupsManager.Core.Common;

using TeacherGroupsManager.Core.Entities;

public interface IAuditableEntity
{
    DateTime? CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    int? CreatedByEmployeeId { get; set; }
    Employee? CreatedByEmployee { get; set; }
    int? UpdatedByEmployeeId { get; set; }
    Employee? UpdatedByEmployee { get; set; }
}

public abstract class BaseEntity : IAuditableEntity
{
    public int Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByEmployeeId { get; set; }
    public Employee? CreatedByEmployee { get; set; }
    public int? UpdatedByEmployeeId { get; set; }
    public Employee? UpdatedByEmployee { get; set; }
}

public abstract class AuditableEntity : BaseEntity;
