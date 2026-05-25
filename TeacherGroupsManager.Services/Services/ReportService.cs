using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;

namespace TeacherGroupsManager.Services.Services;

public class ReportService(IStudentService studentService, IPaymentService paymentService, ILessonService lessonService) : IReportService
{
    public async Task<IReadOnlyList<StudentDto>> GetStudentsReportAsync(int? academicYearId, int? groupId, CancellationToken cancellationToken = default)
    {
        var data = await studentService.GetAllAsync(cancellationToken);
        return data.Where(x => (!academicYearId.HasValue || x.AcademicYearId == academicYearId) && (!groupId.HasValue || x.GroupId == groupId)).ToList();
    }

    public async Task<IReadOnlyList<MonthlyPaymentDto>> GetPaymentsReportAsync(int? month, int? year, int? groupId, CancellationToken cancellationToken = default)
    {
        var data = await paymentService.GetAllAsync(month, year, cancellationToken);
        return data.Where(x => !groupId.HasValue || x.GroupId == groupId).ToList();
    }

    public async Task<IReadOnlyList<LessonDto>> GetLessonsReportAsync(int? groupId, int? month, int? year, CancellationToken cancellationToken = default)
    {
        var data = await lessonService.GetAllAsync(cancellationToken);
        return data.Where(x => (!groupId.HasValue || x.GroupId == groupId) && (!month.HasValue || x.Month == month) && (!year.HasValue || x.Year == year)).ToList();
    }
}
