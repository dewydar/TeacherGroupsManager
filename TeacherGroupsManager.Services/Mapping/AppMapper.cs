using Riok.Mapperly.Abstractions;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Dtos;

namespace TeacherGroupsManager.Services.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AppMapper
{
    [MapProperty(nameof(@Role.CreatedByEmployee.FullName), nameof(RoleDto.CreatedByEmployeeName))]
    [MapProperty(nameof(@Role.UpdatedByEmployee.FullName), nameof(RoleDto.UpdatedByEmployeeName))]
    public partial RoleDto Map(Role role);

    public partial List<RoleDto> Map(IEnumerable<Role> roles);

    [MapProperty(nameof(@Permission.CreatedByEmployee.FullName), nameof(PermissionDto.CreatedByEmployeeName))]
    [MapProperty(nameof(@Permission.UpdatedByEmployee.FullName), nameof(PermissionDto.UpdatedByEmployeeName))]
    public partial PermissionDto Map(Permission permission);

    public partial List<PermissionDto> Map(IEnumerable<Permission> permissions);

    [MapProperty(nameof(@AcademicYear.CreatedByEmployee.FullName), nameof(AcademicYearDto.CreatedByEmployeeName))]
    [MapProperty(nameof(@AcademicYear.UpdatedByEmployee.FullName), nameof(AcademicYearDto.UpdatedByEmployeeName))]
    public partial AcademicYearDto Map(AcademicYear academicYear);

    public partial List<AcademicYearDto> Map(IEnumerable<AcademicYear> academicYears);

    [MapProperty(nameof(@Group.AcademicYear.Name), nameof(GroupDto.AcademicYearName))]
    [MapProperty(nameof(@Group.CreatedByEmployee.FullName), nameof(GroupDto.CreatedByEmployeeName))]
    [MapProperty(nameof(@Group.UpdatedByEmployee.FullName), nameof(GroupDto.UpdatedByEmployeeName))]
    public partial GroupDto Map(Group group);

    public partial List<GroupDto> Map(IEnumerable<Group> groups);

    public partial GroupScheduleDto Map(GroupSchedule schedule);

    public partial List<GroupScheduleDto> Map(IEnumerable<GroupSchedule> schedules);

    [MapProperty(nameof(@Student.AcademicYear.Name), nameof(StudentDto.AcademicYearName))]
    [MapProperty(nameof(@Student.Group.Name), nameof(StudentDto.GroupName))]
    [MapProperty(nameof(@Student.CreatedByEmployee.FullName), nameof(StudentDto.CreatedByEmployeeName))]
    [MapProperty(nameof(@Student.UpdatedByEmployee.FullName), nameof(StudentDto.UpdatedByEmployeeName))]
    public partial StudentDto Map(Student student);

    public partial List<StudentDto> Map(IEnumerable<Student> students);

    [MapProperty(nameof(@Lesson.Group.Name), nameof(LessonDto.GroupName))]
    [MapProperty(nameof(@Lesson.CreatedByEmployee.FullName), nameof(LessonDto.CreatedByEmployeeName))]
    [MapProperty(nameof(@Lesson.UpdatedByEmployee.FullName), nameof(LessonDto.UpdatedByEmployeeName))]
    public partial LessonDto Map(Lesson lesson);

    public partial List<LessonDto> Map(IEnumerable<Lesson> lessons);

    [MapProperty(nameof(@MonthlyPayment.Student.FullName), nameof(MonthlyPaymentDto.StudentName))]
    [MapProperty(nameof(@MonthlyPayment.Group.Name), nameof(MonthlyPaymentDto.GroupName))]
    [MapProperty(nameof(@MonthlyPayment.AcademicYear.Name), nameof(MonthlyPaymentDto.AcademicYearName))]
    [MapProperty(nameof(@MonthlyPayment.CreatedByEmployee.FullName), nameof(MonthlyPaymentDto.CreatedByEmployeeName))]
    [MapProperty(nameof(@MonthlyPayment.UpdatedByEmployee.FullName), nameof(MonthlyPaymentDto.UpdatedByEmployeeName))]
    public partial MonthlyPaymentDto Map(MonthlyPayment payment);

    public partial List<MonthlyPaymentDto> Map(IEnumerable<MonthlyPayment> payments);

    public EmployeeDto Map(Employee employee) =>
        new(
            employee.Id,
            employee.FullName,
            employee.Mobile,
            employee.Email,
            employee.Username,
            employee.RoleId,
            employee.Role.Name,
            employee.Role.ArabicName,
            employee.IsActive,
            employee.Role.RolePermissions.Select(rolePermission => rolePermission.Permission.Code).ToList(),
            employee.CreatedAt,
            employee.UpdatedAt,
            employee.CreatedByEmployee?.FullName,
            employee.UpdatedByEmployee?.FullName);

    public List<EmployeeDto> Map(IEnumerable<Employee> employees) =>
        employees.Select(Map).ToList();
}
