using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class PaymentsDataTableTests : TestBase
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnOnlyRequestedPage()
    {
        var (context, mapper) = CreateContext();
        await SeedPaymentsAsync(context);
        var service = CreateService(context, mapper);

        var result = await service.GetPagedAsync(new DataTableRequestDto { Draw = 1, Start = 1, Length = 2 });

        Assert.Equal(1, result.Draw);
        Assert.Equal(5, result.RecordsTotal);
        Assert.Equal(5, result.RecordsFiltered);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterPaymentsByGroupMonthYearAndStatus()
    {
        var (context, mapper) = CreateContext();
        await SeedPaymentsAsync(context);
        var service = CreateService(context, mapper);

        var result = await service.GetPagedAsync(new DataTableRequestDto
        {
            Start = 0,
            Length = 10,
            Filters = new Dictionary<string, string?>
            {
                ["academicYearId"] = "1",
                ["groupId"] = "1",
                ["month"] = "5",
                ["year"] = "2026",
                ["paymentStatus"] = ((int)PaymentStatus.PartiallyPaid).ToString()
            }
        });

        Assert.Equal(1, result.RecordsFiltered);
        Assert.Single(result.Data);
        Assert.Equal("أحمد محمد علي", result.Data[0].StudentName);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldSearchByStudentName()
    {
        var (context, mapper) = CreateContext();
        await SeedPaymentsAsync(context);
        var service = CreateService(context, mapper);

        var result = await service.GetPagedAsync(new DataTableRequestDto { Start = 0, Length = 10, SearchValue = "منى" });

        Assert.Equal(1, result.RecordsFiltered);
        Assert.Equal("منى أحمد حسن", result.Data.Single().StudentName);
    }

    [Theory]
    [InlineData("studentName")]
    [InlineData("requiredAmount")]
    [InlineData("paidAmount")]
    [InlineData("remainingAmount")]
    [InlineData("paymentDate")]
    public async Task GetPagedAsync_ShouldSortByRequestedPaymentColumns(string sortColumn)
    {
        var (context, mapper) = CreateContext();
        await SeedPaymentsAsync(context);
        var service = CreateService(context, mapper);

        var result = await service.GetPagedAsync(new DataTableRequestDto
        {
            Start = 0,
            Length = 10,
            SortColumn = sortColumn,
            SortDirection = "asc"
        });

        Assert.Equal(5, result.Data.Count);
        AssertSorted(result.Data, sortColumn);
    }

    private static void AssertSorted(IReadOnlyList<MonthlyPaymentDto> payments, string sortColumn)
    {
        switch (sortColumn)
        {
            case "studentName":
                Assert.Equal(payments.OrderBy(x => x.StudentName).Select(x => x.Id), payments.Select(x => x.Id));
                break;
            case "requiredAmount":
                Assert.Equal(payments.OrderBy(x => x.RequiredAmount).Select(x => x.Id), payments.Select(x => x.Id));
                break;
            case "paidAmount":
                Assert.Equal(payments.OrderBy(x => x.PaidAmount).Select(x => x.Id), payments.Select(x => x.Id));
                break;
            case "remainingAmount":
                Assert.Equal(payments.OrderBy(x => x.RemainingAmount).Select(x => x.Id), payments.Select(x => x.Id));
                break;
            case "paymentDate":
                Assert.Equal(payments.OrderBy(x => x.PaymentDate).Select(x => x.Id), payments.Select(x => x.Id));
                break;
        }
    }

    private static PaymentService CreateService(TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context, AppMapper mapper) =>
        new(new UnitOfWork(context), mapper, TestLocalizer.Instance);

    private static async Task SeedPaymentsAsync(TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context)
    {
        context.AcademicYears.Find(1)!.MonthlyPrice = 600;
        context.AcademicYears.Find(2)!.MonthlyPrice = 700;
        context.Students.AddRange(
            new Student { Id = 301, FullName = "أحمد محمد علي", Mobile = "01000000301", AcademicYearId = 1, GroupId = 1 },
            new Student { Id = 302, FullName = "منى أحمد حسن", Mobile = "01000000302", AcademicYearId = 1, GroupId = 1 },
            new Student { Id = 303, FullName = "سارة محمود علي", Mobile = "01000000303", AcademicYearId = 1, GroupId = 1 },
            new Student { Id = 304, FullName = "خالد محمد حسن", Mobile = "01000000304", AcademicYearId = 2, GroupId = 2 },
            new Student { Id = 305, FullName = "ليلى أحمد سعيد", Mobile = "01000000305", AcademicYearId = 2, GroupId = 2 });
        context.MonthlyPayments.AddRange(
            Payment(1, 301, 1, 1, 5, 2026, 600, 200, 400, PaymentStatus.PartiallyPaid, new DateTime(2026, 5, 3)),
            Payment(2, 302, 1, 1, 5, 2026, 600, 0, 600, PaymentStatus.Unpaid, null),
            Payment(3, 303, 1, 1, 6, 2026, 650, 650, 0, PaymentStatus.Paid, new DateTime(2026, 6, 2)),
            Payment(4, 304, 2, 2, 5, 2026, 700, 700, 0, PaymentStatus.Paid, new DateTime(2026, 5, 1)),
            Payment(5, 305, 2, 2, 5, 2025, 750, 300, 450, PaymentStatus.PartiallyPaid, new DateTime(2025, 5, 4)));
        await context.SaveChangesAsync();
    }

    private static MonthlyPayment Payment(int id, int studentId, int groupId, int academicYearId, int month, int year, decimal required, decimal paid, decimal remaining, PaymentStatus status, DateTime? paymentDate) =>
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
            PaymentDate = paymentDate
        };
}
