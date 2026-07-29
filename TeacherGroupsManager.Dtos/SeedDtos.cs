using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.Dtos;

public class TestDataSeedSummaryDto
{
    public int TeachersAdded { get; set; }
    public int AssistantTeachersAdded { get; set; }
    public int AcademicYearsAdded { get; set; }
    public int GroupsAdded { get; set; }
    public int StudentsAdded { get; set; }
    public int LessonsAdded { get; set; }
    public int MonthlyPaymentsAdded { get; set; }
    public int SkippedDuplicates { get; set; }

    [StringLength(AppConstants.MaxStringLength)]
    public string Message => "تم توليد البيانات التجريبية بنجاح";
}
