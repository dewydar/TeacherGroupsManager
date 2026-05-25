using TeacherGroupsManager.Core.Common;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Core.Entities;

public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;
    public GroupType GroupType { get; set; }
    public int? TeacherId { get; set; }
    public Employee? Teacher { get; set; }
    public int? AssistantTeacherId { get; set; }
    public Employee? AssistantTeacher { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public decimal DefaultLessonPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
