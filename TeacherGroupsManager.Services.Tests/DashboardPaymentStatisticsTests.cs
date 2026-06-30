using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class DashboardPaymentStatisticsTests : TestBase
{
    [Fact]
    public async Task GetSummaryAsync_ShouldReturnCurrentMonthPaymentStatistics()
    {
        var (context, _) = CreateContext();
        var now = DateTime.Now;
        SeedDashboardPayments(context, now.Month, now.Year);
        var service = new DashboardService(new UnitOfWork(context));

        var summary = await service.GetSummaryAsync();

        Assert.Equal(1800, summary.CurrentMonthTotalRequiredAmount);
        Assert.Equal(800, summary.CurrentMonthTotalPaidAmount);
        Assert.Equal(1000, summary.CurrentMonthTotalRemainingAmount);
        Assert.Equal(1, summary.CurrentMonthPaidStudentsCount);
        Assert.Equal(1, summary.CurrentMonthUnpaidStudentsCount);
        Assert.Equal(1, summary.CurrentMonthPartiallyPaidStudentsCount);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnPaymentSummaryPerGroup()
    {
        var (context, _) = CreateContext();
        var now = DateTime.Now;
        SeedDashboardPayments(context, now.Month, now.Year);
        var service = new DashboardService(new UnitOfWork(context));

        var summary = await service.GetSummaryAsync();

        var group = summary.PaymentsPerGroup.Single(x => x.GroupId == 1);
        Assert.Equal(3, group.TotalStudents);
        Assert.Equal(1, group.PaidStudentsCount);
        Assert.Equal(1, group.UnpaidStudentsCount);
        Assert.Equal(1, group.PartiallyPaidStudentsCount);
        Assert.Equal(800, group.TotalCollectedAmount);
        Assert.Equal(1000, group.RemainingAmount);
    }

    private static void SeedDashboardPayments(TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context, int month, int year)
    {
        context.Students.AddRange(
            new Student { Id = 401, FullName = "أحمد محمد علي", Mobile = "01000000401", AcademicYearId = 1, GroupId = 1 },
            new Student { Id = 402, FullName = "منى أحمد حسن", Mobile = "01000000402", AcademicYearId = 1, GroupId = 1 },
            new Student { Id = 403, FullName = "سارة محمود علي", Mobile = "01000000403", AcademicYearId = 1, GroupId = 1 });
        context.MonthlyPayments.AddRange(
            Payment(401, 401, month, year, 600, 600, 0, PaymentStatus.Paid),
            Payment(402, 402, month, year, 600, 0, 600, PaymentStatus.Unpaid),
            Payment(403, 403, month, year, 600, 200, 400, PaymentStatus.PartiallyPaid),
            Payment(404, 401, month == 1 ? 12 : month - 1, month == 1 ? year - 1 : year, 600, 600, 0, PaymentStatus.Paid));
        context.SaveChanges();
    }

    private static MonthlyPayment Payment(int id, int studentId, int month, int year, decimal required, decimal paid, decimal remaining, PaymentStatus status) =>
        new()
        {
            Id = id,
            StudentId = studentId,
            GroupId = 1,
            AcademicYearId = 1,
            Month = month,
            Year = year,
            RequiredAmount = required,
            PaidAmount = paid,
            RemainingAmount = remaining,
            PaymentStatus = status,
            PaymentDate = paid > 0 ? DateTime.Now : null
        };
}
