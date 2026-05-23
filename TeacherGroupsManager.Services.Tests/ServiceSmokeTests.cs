using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Context;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class ServiceSmokeTests
{
    [Fact]
    public async Task GroupService_Creates_Group()
    {
        var (context, mapper) = CreateContext();
        var service = new GroupService(new UnitOfWork(context), mapper);
        var result = await service.CreateAsync(new CreateGroupDto("مجموعة اختبار", 1, GroupType.Public, null, null, DayOfWeek.Saturday, new TimeOnly(18, 0), new TimeOnly(20, 0), 150));
        Assert.True(result.Succeeded);
        Assert.Equal(3, await context.Groups.CountAsync());
    }

    [Fact]
    public async Task StudentService_Creates_Student()
    {
        var (context, mapper) = CreateContext();
        var service = new StudentService(new UnitOfWork(context), mapper);
        var result = await service.CreateAsync(new CreateStudentDto("أحمد محمد", "01000000000", "01011111111", 1, 1, null));
        Assert.True(result.Succeeded);
        Assert.Equal("أحمد محمد", (await context.Students.FirstAsync()).FullName);
    }

    [Fact]
    public async Task LessonService_Assigns_Group_Lesson_To_All_Group_Students()
    {
        var (context, mapper) = CreateContext();
        context.Students.AddRange(
            new Student { FullName = "طالب 1", Mobile = "1", AcademicYearId = 1, GroupId = 1 },
            new Student { FullName = "طالب 2", Mobile = "2", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();

        var service = new LessonService(new UnitOfWork(context), mapper);
        var result = await service.CreateAsync(new CreateLessonDto("درس الوحدة الأولى", null, 1, LessonType.Group, DateTime.Today, 150, true, DateTime.Today.Month, DateTime.Today.Year, null, []));

        Assert.True(result.Succeeded);
        Assert.Equal(2, await context.LessonStudents.CountAsync());
    }

    [Fact]
    public async Task PaymentService_Calculates_Remaining_And_Status()
    {
        var (context, mapper) = CreateContext();
        context.Students.Add(new Student { Id = 10, FullName = "طالب", Mobile = "1", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();
        var service = new PaymentService(new UnitOfWork(context), mapper);

        var result = await service.CreateAsync(new CreateMonthlyPaymentDto(10, 1, 1, 5, 2026, 300, 100, PaymentStatus.PartiallyPaid, DateTime.Today, null, null));

        Assert.True(result.Succeeded);
        var payment = await context.MonthlyPayments.FirstAsync();
        Assert.Equal(200, payment.RemainingAmount);
        Assert.Equal(PaymentStatus.PartiallyPaid, payment.PaymentStatus);
    }

    [Fact]
    public async Task DashboardService_Returns_Current_Counts()
    {
        var (context, _) = CreateContext();
        context.Students.Add(new Student { FullName = "طالب", Mobile = "1", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();

        var dashboard = await new DashboardService(new UnitOfWork(context)).GetSummaryAsync();

        Assert.True(dashboard.TotalGroups >= 2);
        Assert.Equal(1, dashboard.TotalStudents);
    }

    private static (TeacherGroupsDbContext Context, IMapper Mapper) CreateContext()
    {
        var options = new DbContextOptionsBuilder<TeacherGroupsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new TeacherGroupsDbContext(options);
        context.Database.EnsureCreated();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<AppMappingProfile>()).CreateMapper();
        _ = new Pbkdf2PasswordHasher();
        return (context, mapper);
    }
}
