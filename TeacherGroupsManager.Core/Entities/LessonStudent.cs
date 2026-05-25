using TeacherGroupsManager.Core.Common;

namespace TeacherGroupsManager.Core.Entities;

public class LessonStudent : IAuditableEntity
{
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByEmployeeId { get; set; }
    public Employee? CreatedByEmployee { get; set; }
    public int? UpdatedByEmployeeId { get; set; }
    public Employee? UpdatedByEmployee { get; set; }
}
