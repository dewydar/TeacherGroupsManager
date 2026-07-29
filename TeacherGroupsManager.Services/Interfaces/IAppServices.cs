using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Interfaces;

public interface IAuthService
{
    Task<OperationResult<LoginResultDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordDto dto, int? currentEmployeeId = null, CancellationToken cancellationToken = default);
}

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataTableResponseDto<RoleDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default);
    Task<RoleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(RoleDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(RoleDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<RolePermissionsDto?> GetPermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdatePermissionsAsync(int roleId, int[] permissionIds, CancellationToken cancellationToken = default);
}

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataTableResponseDto<EmployeeDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(EditEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAcademicYearService
{
    Task<IReadOnlyList<AcademicYearDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataTableResponseDto<AcademicYearDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default);
    Task<AcademicYearDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateAcademicYearDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(EditAcademicYearDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IGroupService
{
    Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GroupDto>> GetByAcademicYearAsync(int academicYearId, CancellationToken cancellationToken = default);
    Task<DataTableResponseDto<GroupDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default);
    Task<GroupDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateGroupDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(EditGroupDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IStudentService
{
    Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DataTableResponseDto<StudentDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default);
    Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(EditStudentDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface ILessonService
{
    Task<IReadOnlyList<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailableLessonDateDto>> GetAvailableLessonDatesAsync(int groupId, int month, int year, DayOfWeek? dayOfWeek = null, CancellationToken cancellationToken = default);
    Task<DataTableResponseDto<LessonDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default);
    Task<LessonDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LessonAttendanceDto?> GetAttendanceAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateLessonDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(EditLessonDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAttendanceAsync(UpdateLessonAttendanceDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IPaymentService
{
    Task<IReadOnlyList<MonthlyPaymentDto>> GetAllAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default);
    Task<DataTableResponseDto<MonthlyPaymentDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default);
    Task<MonthlyPaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CreateAsync(CreateMonthlyPaymentDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(EditMonthlyPaymentDto dto, CancellationToken cancellationToken = default);
    Task<OperationResult> GenerateMonthlyPaymentsAsync(int academicYearId, int groupId, int month, int year, CancellationToken cancellationToken = default);
    Task<OperationResult> MarkAsPaidAsync(int paymentId, CancellationToken cancellationToken = default);
    Task<OperationResult> MarkAsUnpaidAsync(int paymentId, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdatePaidAmountAsync(int paymentId, decimal paidAmount, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(DashboardFilterDto filter, CancellationToken cancellationToken = default);
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}

public interface ITestDataSeeder
{
    Task<TestDataSeedSummaryDto> SeedAsync(CancellationToken cancellationToken = default);
}
