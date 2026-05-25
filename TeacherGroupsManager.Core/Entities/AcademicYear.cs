using TeacherGroupsManager.Core.Common;

namespace TeacherGroupsManager.Core.Entities;

public class AcademicYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}
