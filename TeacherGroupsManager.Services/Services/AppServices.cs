using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Shared.Extensions;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class AuthService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher) : IAuthService
{
    public async Task<OperationResult<EmployeeDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await unitOfWork.Repository<Employee>().Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == dto.Username && x.IsActive, cancellationToken);

        if (employee is null || !passwordHasher.Verify(dto.Password, employee.PasswordHash))
        {
            return OperationResult<EmployeeDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة");
        }

        return OperationResult<EmployeeDto>.Success(mapper.Map<EmployeeDto>(employee), "تم تسجيل الدخول بنجاح");
    }
}

public class RoleService(IUnitOfWork unitOfWork, IMapper mapper) : IRoleService
{
    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<RoleDto>>(await unitOfWork.Repository<Role>().Query().OrderBy(x => x.Id).ToListAsync(cancellationToken));

    public async Task<OperationResult> CreateAsync(RoleDto dto, CancellationToken cancellationToken = default)
    {
        await unitOfWork.Repository<Role>().AddAsync(new Role { Name = dto.Name, ArabicName = dto.ArabicName, IsActive = dto.IsActive }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ الدور بنجاح");
    }
}

public class PermissionService(IUnitOfWork unitOfWork, IMapper mapper) : IPermissionService
{
    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<PermissionDto>>(await unitOfWork.Repository<Permission>().Query().OrderBy(x => x.ModuleName).ToListAsync(cancellationToken));
}

public class EmployeeService(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<EmployeeDto>>(await unitOfWork.Repository<Employee>().Query().Include(x => x.Role).OrderBy(x => x.FullName).ToListAsync(cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Repository<Employee>().AnyAsync(x => x.Username == dto.Username, cancellationToken))
        {
            return OperationResult.Failure("اسم المستخدم مستخدم من قبل");
        }

        await unitOfWork.Repository<Employee>().AddAsync(new Employee
        {
            FullName = dto.FullName,
            Mobile = dto.Mobile,
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = passwordHasher.Hash(dto.Password),
            RoleId = dto.RoleId,
            IsActive = dto.IsActive
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ الموظف بنجاح");
    }
}

public class AcademicYearService(IUnitOfWork unitOfWork, IMapper mapper) : IAcademicYearService
{
    public async Task<IReadOnlyList<AcademicYearDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<AcademicYearDto>>(await unitOfWork.Repository<AcademicYear>().Query().OrderByDescending(x => x.StartDate).ToListAsync(cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateAcademicYearDto dto, CancellationToken cancellationToken = default)
    {
        await unitOfWork.Repository<AcademicYear>().AddAsync(new AcademicYear { Name = dto.Name, StartDate = dto.StartDate, EndDate = dto.EndDate, IsActive = dto.IsActive }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ السنة الدراسية بنجاح");
    }
}

public class GroupService(IUnitOfWork unitOfWork, IMapper mapper) : IGroupService
{
    public async Task<IReadOnlyList<GroupDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<GroupDto>>(await GroupsQuery().OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToListAsync(cancellationToken));

    public async Task<GroupDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        mapper.Map<GroupDto?>(await GroupsQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateGroupDto dto, CancellationToken cancellationToken = default)
    {
        await unitOfWork.Repository<Group>().AddAsync(new Group
        {
            Name = dto.Name,
            AcademicYearId = dto.AcademicYearId,
            GroupType = dto.GroupType,
            TeacherId = dto.TeacherId,
            AssistantTeacherId = dto.AssistantTeacherId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            DefaultLessonPrice = dto.DefaultLessonPrice,
            IsActive = dto.IsActive
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ المجموعة بنجاح");
    }

    private IQueryable<Group> GroupsQuery() => unitOfWork.Repository<Group>().Query().Include(x => x.AcademicYear);
}

public class StudentService(IUnitOfWork unitOfWork, IMapper mapper) : IStudentService
{
    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<StudentDto>>(await unitOfWork.Repository<Student>().Query().Include(x => x.Group).Include(x => x.AcademicYear).OrderBy(x => x.FullName).ToListAsync(cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default)
    {
        await unitOfWork.Repository<Student>().AddAsync(new Student { FullName = dto.FullName, Mobile = dto.Mobile, ParentMobile = dto.ParentMobile, AcademicYearId = dto.AcademicYearId, GroupId = dto.GroupId, Notes = dto.Notes, IsActive = dto.IsActive }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ الطالب بنجاح");
    }
}

public class LessonService(IUnitOfWork unitOfWork, IMapper mapper) : ILessonService
{
    public async Task<IReadOnlyList<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<LessonDto>>(await unitOfWork.Repository<Lesson>().Query().Include(x => x.Group).OrderByDescending(x => x.LessonDate).ToListAsync(cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateLessonDto dto, CancellationToken cancellationToken = default)
    {
        var lesson = new Lesson { Title = dto.Title, Description = dto.Description, GroupId = dto.GroupId, LessonType = dto.LessonType, LessonDate = dto.LessonDate, Price = dto.Price, IsMonthlyPaymentRequired = dto.IsMonthlyPaymentRequired, Month = dto.Month, Year = dto.Year, CreatedByEmployeeId = dto.CreatedByEmployeeId };
        if (dto.LessonType == LessonType.Private)
        {
            foreach (var studentId in dto.StudentIds.Distinct())
            {
                lesson.LessonStudents.Add(new LessonStudent { StudentId = studentId });
            }
        }
        else
        {
            var studentIds = await unitOfWork.Repository<Student>().Query().Where(x => x.GroupId == dto.GroupId && x.IsActive).Select(x => x.Id).ToListAsync(cancellationToken);
            foreach (var studentId in studentIds)
            {
                lesson.LessonStudents.Add(new LessonStudent { StudentId = studentId });
            }
        }

        await unitOfWork.Repository<Lesson>().AddAsync(lesson, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ الدرس بنجاح");
    }
}

public class PaymentService(IUnitOfWork unitOfWork, IMapper mapper) : IPaymentService
{
    public async Task<IReadOnlyList<MonthlyPaymentDto>> GetAllAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default)
    {
        var query = PaymentsQuery();
        if (month.HasValue) query = query.Where(x => x.Month == month);
        if (year.HasValue) query = query.Where(x => x.Year == year);
        return mapper.Map<List<MonthlyPaymentDto>>(await query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync(cancellationToken));
    }

    public async Task<OperationResult> CreateAsync(CreateMonthlyPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var remaining = Math.Max(0, dto.RequiredAmount - dto.PaidAmount);
        var status = dto.PaidAmount <= 0 ? PaymentStatus.Unpaid : remaining == 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
        await unitOfWork.Repository<MonthlyPayment>().AddAsync(new MonthlyPayment
        {
            StudentId = dto.StudentId,
            GroupId = dto.GroupId,
            AcademicYearId = dto.AcademicYearId,
            Month = dto.Month,
            Year = dto.Year,
            RequiredAmount = dto.RequiredAmount,
            PaidAmount = dto.PaidAmount,
            RemainingAmount = remaining,
            PaymentStatus = status,
            PaymentDate = dto.PaymentDate,
            Notes = dto.Notes,
            CreatedByEmployeeId = dto.CreatedByEmployeeId
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ المدفوعة بنجاح");
    }

    private IQueryable<MonthlyPayment> PaymentsQuery() => unitOfWork.Repository<MonthlyPayment>().Query().Include(x => x.Student).Include(x => x.Group).Include(x => x.AcademicYear);
}

public class DashboardService(IUnitOfWork unitOfWork) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var students = unitOfWork.Repository<Student>().Query();
        var groups = unitOfWork.Repository<Group>().Query();
        var employees = unitOfWork.Repository<Employee>().Query().Include(x => x.Role);
        var payments = unitOfWork.Repository<MonthlyPayment>().Query().Include(x => x.Group).Where(x => x.Month == now.Month && x.Year == now.Year);

        var groupCards = await groups.Include(x => x.Students).OrderBy(x => x.Name)
            .Select(x => new GroupStudentCountDto(x.Id, x.Name, x.Students.Count, $"/Groups/Details/{x.Id}"))
            .ToListAsync(cancellationToken);

        var paymentGroups = await groups.Select(g => new GroupPaymentSummaryDto(
            g.Id,
            g.Name,
            g.Students.Count,
            g.Students.Count(s => s.MonthlyPayments.Any(p => p.Month == now.Month && p.Year == now.Year && p.PaymentStatus == PaymentStatus.Paid)),
            g.Students.Count(s => !s.MonthlyPayments.Any(p => p.Month == now.Month && p.Year == now.Year && p.PaymentStatus == PaymentStatus.Paid)),
            g.Students.SelectMany(s => s.MonthlyPayments).Where(p => p.Month == now.Month && p.Year == now.Year).Sum(p => p.PaidAmount),
            g.Students.SelectMany(s => s.MonthlyPayments).Where(p => p.Month == now.Month && p.Year == now.Year).Sum(p => p.RemainingAmount),
            $"/Groups/Details/{g.Id}")).ToListAsync(cancellationToken);

        var groupsByDay = await groups.GroupBy(x => x.DayOfWeek)
            .Select(x => new GroupDayDto(x.Key, x.Key.DayToArabic(), x.Count(), x.Select(g => g.Name).ToList()))
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto(
            await students.CountAsync(cancellationToken),
            await groups.CountAsync(cancellationToken),
            await groups.CountAsync(x => x.GroupType == GroupType.Private, cancellationToken),
            await groups.CountAsync(x => x.GroupType == GroupType.Public, cancellationToken),
            await employees.CountAsync(cancellationToken),
            await employees.CountAsync(x => x.Role.Name == AppConstants.TeacherRole, cancellationToken),
            await employees.CountAsync(x => x.Role.Name == AppConstants.AssistantTeacherRole, cancellationToken),
            await payments.SumAsync(x => x.RequiredAmount, cancellationToken),
            await payments.SumAsync(x => x.PaidAmount, cancellationToken),
            await payments.SumAsync(x => x.RemainingAmount, cancellationToken),
            await payments.CountAsync(x => x.PaymentStatus == PaymentStatus.Paid, cancellationToken),
            await payments.CountAsync(x => x.PaymentStatus == PaymentStatus.Unpaid, cancellationToken),
            "/Students",
            "/Groups",
            "/Groups?type=Private",
            "/Groups?type=Public",
            $"/Payments?month={now.Month}&year={now.Year}&status=Paid",
            $"/Payments?month={now.Month}&year={now.Year}&status=Unpaid",
            groupCards,
            paymentGroups,
            groupsByDay);
    }
}

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
