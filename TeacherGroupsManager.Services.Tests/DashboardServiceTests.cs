using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class DashboardServiceTests : TestBase
{
    [Fact]
    public async Task GetSummaryAsync_Returns_Current_Counts()
    {
        var (context, _) = CreateContext();
        context.Students.Add(new Student { FullName = "Student", Mobile = "1", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();

        var dashboard = await new DashboardService(new UnitOfWork(context)).GetSummaryAsync();

        Assert.True(dashboard.TotalGroups >= 2);
        Assert.Equal(1, dashboard.TotalStudents);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldReturnZeros_WhenSystemDataIsEmpty()
    {
        var (context, _) = CreateContext();
        context.GroupSchedules.RemoveRange(context.GroupSchedules);
        context.Groups.RemoveRange(context.Groups);
        context.AcademicYears.RemoveRange(context.AcademicYears);
        await context.SaveChangesAsync();
        var service = new DashboardService(new UnitOfWork(context));

        var summary = await service.GetDashboardSummaryAsync(new DashboardFilterDto { Month = 5, Year = 2026 });

        Assert.Equal(0, summary.TotalStudents);
        Assert.Equal(0, summary.TotalGroups);
        Assert.Equal(0, summary.TotalRequiredAmount);
        Assert.Equal(0, summary.TotalPaidAmount);
        Assert.Equal(0, summary.TotalRemainingAmount);
        Assert.Empty(summary.StudentsPerGroup);
        Assert.Empty(summary.PaymentsPerGroup);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldCalculateCountsAndRespectAcademicYearFilter()
    {
        var (context, _) = CreateContext();
        SeedDashboardData(context, month: 5, year: 2026);
        var service = new DashboardService(new UnitOfWork(context));

        var summary = await service.GetDashboardSummaryAsync(new DashboardFilterDto { AcademicYearId = 1, Month = 5, Year = 2026 });

        Assert.Equal(2, summary.TotalStudents);
        Assert.Equal(1, summary.TotalGroups);
        Assert.Equal(1, summary.TotalPublicGroups);
        Assert.Equal(0, summary.TotalPrivateGroups);
        Assert.Equal(1, summary.CurrentMonthLessons);
        Assert.Equal(900, summary.TotalRequiredAmount);
        Assert.Equal(300, summary.TotalPaidAmount);
        Assert.Equal(600, summary.TotalRemainingAmount);
        Assert.Equal(1, summary.PaidStudentsCount);
        Assert.Equal(1, summary.UnpaidStudentsCount);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldRespectGroupIdFilter()
    {
        var (context, _) = CreateContext();
        SeedDashboardData(context, month: 5, year: 2026);
        var service = new DashboardService(new UnitOfWork(context));

        var summary = await service.GetDashboardSummaryAsync(new DashboardFilterDto { GroupId = 2, Month = 5, Year = 2026 });

        Assert.Equal(1, summary.TotalStudents);
        Assert.Equal(1, summary.TotalGroups);
        Assert.Equal(700, summary.TotalRequiredAmount);
        Assert.Equal(100, summary.TotalPaidAmount);
        Assert.Single(summary.StudentsPerGroup);
        Assert.Equal(1, summary.StudentsPerGroup.Single().StudentsCount);
        Assert.Single(summary.PaymentsPerGroup);
        Assert.Equal(1, summary.PaymentsPerGroup.Single().PartiallyPaidStudentsCount);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldCalculateStudentsAndPaymentSummaryPerGroup()
    {
        var (context, _) = CreateContext();
        SeedDashboardData(context, month: 5, year: 2026);
        var service = new DashboardService(new UnitOfWork(context));

        var summary = await service.GetDashboardSummaryAsync(new DashboardFilterDto { Month = 5, Year = 2026 });

        var groupOneStudents = summary.StudentsPerGroup.Single(x => x.GroupId == 1);
        Assert.Equal(2, groupOneStudents.StudentsCount);
        var groupOnePayments = summary.PaymentsPerGroup.Single(x => x.GroupId == 1);
        Assert.Equal(2, groupOnePayments.TotalStudents);
        Assert.Equal(1, groupOnePayments.PaidStudentsCount);
        Assert.Equal(1, groupOnePayments.UnpaidStudentsCount);
        Assert.Equal(900, groupOnePayments.TotalRequiredAmount);
        Assert.Equal(300, groupOnePayments.TotalCollectedAmount);
        Assert.Equal(600, groupOnePayments.RemainingAmount);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldLimitDashboardListsToTopFive()
    {
        var (context, _) = CreateContext();
        var month = 5;
        var year = 2026;
        var baseDate = new DateTime(year, month, 1);

        for (var i = 1; i <= 6; i++)
        {
            var groupId = 600 + i;
            var studentId = 600 + i;
            context.Groups.Add(new Group
            {
                Id = groupId,
                Name = $"Dashboard Limit Group {i:D2}",
                AcademicYearId = 1,
                GroupType = GroupType.Public,
                DayOfWeek = (DayOfWeek)(i % 7),
                StartTime = new TimeOnly(8 + i, 0),
                EndTime = new TimeOnly(9 + i, 0),
                DefaultLessonPrice = 150,
                IsActive = true
            });
            context.GroupSchedules.Add(new GroupSchedule
            {
                Id = groupId,
                GroupId = groupId,
                DayOfWeek = (DayOfWeek)(i % 7),
                StartTime = new TimeOnly(8 + i, 0),
                EndTime = new TimeOnly(9 + i, 0)
            });
            context.Students.Add(new Student
            {
                Id = studentId,
                FullName = $"Dashboard Student {i}",
                Mobile = $"0100000060{i}",
                AcademicYearId = 1,
                GroupId = groupId,
                IsActive = true,
                CreatedAt = baseDate.AddDays(i)
            });
            context.MonthlyPayments.Add(Payment(600 + i, studentId, groupId, 1, month, year, 300, 300, 0, PaymentStatus.Paid, baseDate.AddDays(i)));
        }
        await context.SaveChangesAsync();
        var service = new DashboardService(new UnitOfWork(context));

        var summary = await service.GetDashboardSummaryAsync(new DashboardFilterDto { Month = month, Year = year });

        Assert.Equal(5, summary.PaymentsPerGroup.Count);
        Assert.Equal(5, summary.RecentStudents.Count);
        Assert.Equal(5, summary.RecentPayments.Count);
        Assert.Equal(5, summary.UpcomingGroups.Count);
    }

    private static void SeedDashboardData(TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context, int month, int year)
    {
        context.Students.AddRange(
            new Student { Id = 501, FullName = "Student One Name", Mobile = "01000000501", AcademicYearId = 1, GroupId = 1, IsActive = true },
            new Student { Id = 502, FullName = "Student Two Name", Mobile = "01000000502", AcademicYearId = 1, GroupId = 1, IsActive = true },
            new Student { Id = 503, FullName = "Student Three Name", Mobile = "01000000503", AcademicYearId = 2, GroupId = 2, IsActive = true },
            new Student { Id = 504, FullName = "Inactive Student Name", Mobile = "01000000504", AcademicYearId = 1, GroupId = 1, IsActive = false });
        context.Lessons.AddRange(
            new Lesson { Id = 501, Title = "Lesson One", GroupId = 1, LessonType = LessonType.Group, LessonDate = new DateTime(year, month, 5), Month = month, Year = year, Price = 150 },
            new Lesson { Id = 502, Title = "Lesson Two", GroupId = 2, LessonType = LessonType.Private, LessonDate = new DateTime(year, month, 6), Month = month, Year = year, Price = 300 });
        context.MonthlyPayments.AddRange(
            Payment(501, 501, 1, 1, month, year, 300, 300, 0, PaymentStatus.Paid),
            Payment(502, 502, 1, 1, month, year, 600, 0, 600, PaymentStatus.Unpaid),
            Payment(503, 503, 2, 2, month, year, 700, 100, 600, PaymentStatus.PartiallyPaid),
            Payment(504, 501, 1, 1, month == 1 ? 12 : month - 1, month == 1 ? year - 1 : year, 300, 300, 0, PaymentStatus.Paid));
        context.SaveChanges();
    }

    private static MonthlyPayment Payment(int id, int studentId, int groupId, int academicYearId, int month, int year, decimal required, decimal paid, decimal remaining, PaymentStatus status, DateTime? paymentDate = null) =>
        new()
        {
            Id = id,
            StudentId = studentId,
            GroupId = groupId,
            AcademicYearId = academicYearId,
            Month = month,
            Year = year,
            RequiredAmount = required,
            PaidAmount = paid,
            RemainingAmount = remaining,
            PaymentStatus = status,
            PaymentDate = paid > 0 ? paymentDate ?? DateTime.Now : null
        };
}
