using TeacherGroupsManager.Core.Common;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Core.Entities;

public class Lesson : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public LessonType LessonType { get; set; }
    public DateTime LessonDate { get; set; }
    public decimal Price { get; set; }
    public bool IsMonthlyPaymentRequired { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public ICollection<LessonStudent> LessonStudents { get; set; } = new List<LessonStudent>();
}
