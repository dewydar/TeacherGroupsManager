using TeacherGroupsManager.Core.Common;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Core.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}

public class Employee : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

public class AcademicYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}

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
    public int? CreatedByEmployeeId { get; set; }
    public Employee? CreatedByEmployee { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<LessonStudent> LessonStudents { get; set; } = new List<LessonStudent>();
}

public class LessonStudent
{
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
}

public class MonthlyPayment : BaseEntity
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal RequiredAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public int? CreatedByEmployeeId { get; set; }
    public Employee? CreatedByEmployee { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
