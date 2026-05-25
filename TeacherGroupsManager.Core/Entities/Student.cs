using TeacherGroupsManager.Core.Common;

namespace TeacherGroupsManager.Core.Entities;

public class Student : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? ParentMobile { get; set; }
    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<LessonStudent> LessonStudents { get; set; } = new List<LessonStudent>();
    public ICollection<MonthlyPayment> MonthlyPayments { get; set; } = new List<MonthlyPayment>();
}
