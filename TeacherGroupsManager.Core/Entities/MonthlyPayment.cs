using TeacherGroupsManager.Core.Common;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Core.Entities;

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
}
