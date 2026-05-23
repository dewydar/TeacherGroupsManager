using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Interfaces;

public interface IAuthService
{
    Task<OperationResult<EmployeeDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(RoleDto dto, CancellationToken cancellationToken = default);
}

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
}

public interface IAcademicYearService
{
    Task<IReadOnlyList<AcademicYearDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateAcademicYearDto dto, CancellationToken cancellationToken = default);
}

public interface IGroupService
{
    Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GroupDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateGroupDto dto, CancellationToken cancellationToken = default);
}

public interface IStudentService
{
    Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default);
}

public interface ILessonService
{
    Task<IReadOnlyList<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateLessonDto dto, CancellationToken cancellationToken = default);
}

public interface IPaymentService
{
    Task<IReadOnlyList<MonthlyPaymentDto>> GetAllAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateMonthlyPaymentDto dto, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}

public interface IReportService
{
    Task<IReadOnlyList<StudentDto>> GetStudentsReportAsync(int? academicYearId, int? groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyPaymentDto>> GetPaymentsReportAsync(int? month, int? year, int? groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LessonDto>> GetLessonsReportAsync(int? groupId, int? month, int? year, CancellationToken cancellationToken = default);
}
