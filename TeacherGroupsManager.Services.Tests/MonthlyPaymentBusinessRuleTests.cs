using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class MonthlyPaymentBusinessRuleTests : TestBase
{
    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldCreatePaymentsForActiveStudentsOnly()
    {
        var (context, mapper) = CreateContext();
        SeedStudents(context);
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(1, 1, 5, 2026);

        Assert.True(result.Succeeded);
        Assert.Equal("MonthlyPaymentsGenerated", result.Message);
        var payments = await context.MonthlyPayments.OrderBy(x => x.StudentId).ToListAsync();
        Assert.Equal(2, payments.Count);
        Assert.DoesNotContain(payments, x => x.StudentId == 103);
        Assert.All(payments, payment =>
        {
            Assert.Equal(1, payment.GroupId);
            Assert.Equal(1, payment.AcademicYearId);
            Assert.Equal(5, payment.Month);
            Assert.Equal(2026, payment.Year);
            Assert.Equal(600, payment.RequiredAmount);
            Assert.Equal(0, payment.PaidAmount);
            Assert.Equal(payment.RequiredAmount, payment.RemainingAmount);
            Assert.Equal(PaymentStatus.Unpaid, payment.PaymentStatus);
            Assert.Null(payment.PaymentDate);
            Assert.NotNull(payment.CreatedAt);
        });
    }

    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldNotCreateDuplicatePaymentsForSameStudentMonth()
    {
        var (context, mapper) = CreateContext();
        SeedStudents(context);
        var service = CreateService(context, mapper);

        var first = await service.GenerateMonthlyPaymentsAsync(1, 1, 5, 2026);
        var second = await service.GenerateMonthlyPaymentsAsync(1, 1, 5, 2026);

        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.Equal(2, await context.MonthlyPayments.CountAsync(x => x.GroupId == 1 && x.AcademicYearId == 1 && x.Month == 5 && x.Year == 2026));
    }

    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldSkipExistingPaymentAndCreateMissingOnes()
    {
        var (context, mapper) = CreateContext();
        SeedStudents(context);
        context.MonthlyPayments.Add(new MonthlyPayment
        {
            StudentId = 101,
            GroupId = 1,
            AcademicYearId = 1,
            Month = 5,
            Year = 2026,
            RequiredAmount = 600,
            PaidAmount = 0,
            RemainingAmount = 600,
            PaymentStatus = PaymentStatus.Unpaid
        });
        await context.SaveChangesAsync();
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(1, 1, 5, 2026);

        Assert.True(result.Succeeded);
        Assert.Equal(2, await context.MonthlyPayments.CountAsync(x => x.Month == 5 && x.Year == 2026));
        Assert.True(await context.MonthlyPayments.AnyAsync(x => x.StudentId == 102));
    }

    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldUseGroupMonthlyPrice_WhenGroupPriceExists()
    {
        var (context, mapper) = CreateContext();
        SeedStudents(context);
        context.Groups.Find(1)!.MonthlyPrice = 450;
        await context.SaveChangesAsync();
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(1, 1, 5, 2026);

        Assert.True(result.Succeeded);
        Assert.All(await context.MonthlyPayments.ToListAsync(), payment =>
        {
            Assert.Equal(450, payment.RequiredAmount);
            Assert.Equal(450, payment.RemainingAmount);
        });
    }

    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldUseAcademicYearMonthlyPrice_WhenGroupPriceIsNull()
    {
        var (context, mapper) = CreateContext();
        SeedStudents(context);
        context.AcademicYears.Find(1)!.MonthlyPrice = 725;
        context.Groups.Find(1)!.MonthlyPrice = null;
        await context.SaveChangesAsync();
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(1, 1, 5, 2026);

        Assert.True(result.Succeeded);
        Assert.All(await context.MonthlyPayments.ToListAsync(), payment => Assert.Equal(725, payment.RequiredAmount));
    }

    [Fact]
    public async Task MarkAsPaidAsync_ShouldSetPaymentAsPaid()
    {
        var (context, mapper) = CreateContext();
        var payment = await SeedPaymentAsync(context, PaymentStatus.Unpaid, paidAmount: 0, remainingAmount: 600);
        var service = CreateService(context, mapper);

        var result = await service.MarkAsPaidAsync(payment.Id);

        Assert.True(result.Succeeded);
        Assert.Equal("PaymentMarkedPaid", result.Message);
        payment = await context.MonthlyPayments.SingleAsync(x => x.Id == payment.Id);
        Assert.Equal(payment.RequiredAmount, payment.PaidAmount);
        Assert.Equal(0, payment.RemainingAmount);
        Assert.Equal(PaymentStatus.Paid, payment.PaymentStatus);
        Assert.NotNull(payment.PaymentDate);
        Assert.NotNull(payment.UpdatedAt);
    }

    [Fact]
    public async Task MarkAsUnpaidAsync_ShouldResetPaymentToUnpaid()
    {
        var (context, mapper) = CreateContext();
        var payment = await SeedPaymentAsync(context, PaymentStatus.Paid, paidAmount: 600, remainingAmount: 0, paymentDate: DateTime.Today);
        var service = CreateService(context, mapper);

        var result = await service.MarkAsUnpaidAsync(payment.Id);

        Assert.True(result.Succeeded);
        Assert.Equal("PaymentMarkedUnpaid", result.Message);
        payment = await context.MonthlyPayments.SingleAsync(x => x.Id == payment.Id);
        Assert.Equal(0, payment.PaidAmount);
        Assert.Equal(payment.RequiredAmount, payment.RemainingAmount);
        Assert.Equal(PaymentStatus.Unpaid, payment.PaymentStatus);
        Assert.Null(payment.PaymentDate);
        Assert.NotNull(payment.UpdatedAt);
    }

    [Theory]
    [InlineData(0, PaymentStatus.Unpaid, 0, 600, false)]
    [InlineData(200, PaymentStatus.PartiallyPaid, 200, 400, true)]
    [InlineData(600, PaymentStatus.Paid, 600, 0, true)]
    public async Task UpdatePaidAmountAsync_ShouldSetStatusFromAmount(decimal amount, PaymentStatus status, decimal paid, decimal remaining, bool hasPaymentDate)
    {
        var (context, mapper) = CreateContext();
        var payment = await SeedPaymentAsync(context, PaymentStatus.Unpaid, paidAmount: 0, remainingAmount: 600);
        var service = CreateService(context, mapper);

        var result = await service.UpdatePaidAmountAsync(payment.Id, amount);

        Assert.True(result.Succeeded);
        payment = await context.MonthlyPayments.SingleAsync(x => x.Id == payment.Id);
        Assert.Equal(status, payment.PaymentStatus);
        Assert.Equal(paid, payment.PaidAmount);
        Assert.Equal(remaining, payment.RemainingAmount);
        Assert.Equal(hasPaymentDate, payment.PaymentDate is not null);
    }

    [Fact]
    public async Task UpdatePaidAmountAsync_ShouldFail_WhenPaidAmountGreaterThanRequired()
    {
        var (context, mapper) = CreateContext();
        var payment = await SeedPaymentAsync(context, PaymentStatus.Unpaid, paidAmount: 0, remainingAmount: 600);
        var service = CreateService(context, mapper);

        var result = await service.UpdatePaidAmountAsync(payment.Id, 601);

        Assert.False(result.Succeeded);
        Assert.Contains("PaidAmountCannotExceedRequired", result.Errors);
    }

    [Theory]
    [InlineData(0, 2026, "MonthBetween1And12")]
    [InlineData(13, 2026, "MonthBetween1And12")]
    [InlineData(5, 0, "InvalidYear")]
    public async Task GenerateMonthlyPaymentsAsync_ShouldFail_WhenMonthOrYearInvalid(int month, int year, string expectedError)
    {
        var (context, mapper) = CreateContext();
        SeedStudents(context);
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(1, 1, month, year);

        Assert.False(result.Succeeded);
        Assert.Contains(expectedError, result.Errors);
    }

    [Fact]
    public async Task UpdatePaidAmountAsync_ShouldFail_WhenPaidAmountIsNegative()
    {
        var (context, mapper) = CreateContext();
        var payment = await SeedPaymentAsync(context, PaymentStatus.Unpaid, paidAmount: 0, remainingAmount: 600);
        var service = CreateService(context, mapper);

        var result = await service.UpdatePaidAmountAsync(payment.Id, -10);

        Assert.False(result.Succeeded);
        Assert.Contains("PaidAmountCannotBeNegative", result.Errors);
    }

    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldFail_WhenAcademicYearMonthlyPriceIsNegative()
    {
        var (context, mapper) = CreateContext();
        SeedStudents(context);
        context.AcademicYears.Find(1)!.MonthlyPrice = -1;
        await context.SaveChangesAsync();
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(1, 1, 5, 2026);

        Assert.False(result.Succeeded);
        Assert.Contains("MonthlyPriceCannotBeNegative", result.Errors);
    }

    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldFail_WhenGroupMonthlyPriceIsNegative()
    {
        var (context, mapper) = CreateContext();
        SeedStudents(context);
        context.Groups.Find(1)!.MonthlyPrice = -1;
        await context.SaveChangesAsync();
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(1, 1, 5, 2026);

        Assert.False(result.Succeeded);
        Assert.Contains("MonthlyPriceCannotBeNegative", result.Errors);
    }

    [Theory]
    [InlineData("paid")]
    [InlineData("unpaid")]
    [InlineData("update")]
    public async Task PaymentStatusMethods_ShouldFail_WhenPaymentDoesNotExist(string action)
    {
        var (context, mapper) = CreateContext();
        var service = CreateService(context, mapper);

        var result = action switch
        {
            "paid" => await service.MarkAsPaidAsync(999),
            "unpaid" => await service.MarkAsUnpaidAsync(999),
            _ => await service.UpdatePaidAmountAsync(999, 100)
        };

        Assert.False(result.Succeeded);
        Assert.Contains("PaymentNotFound", result.Errors);
    }

    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldFail_WhenAcademicYearDoesNotExist()
    {
        var (context, mapper) = CreateContext();
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(999, 1, 5, 2026);

        Assert.False(result.Succeeded);
        Assert.Contains("AcademicYearNotFound", result.Errors);
    }

    [Fact]
    public async Task GenerateMonthlyPaymentsAsync_ShouldFail_WhenGroupDoesNotExist()
    {
        var (context, mapper) = CreateContext();
        var service = CreateService(context, mapper);

        var result = await service.GenerateMonthlyPaymentsAsync(1, 999, 5, 2026);

        Assert.False(result.Succeeded);
        Assert.Contains("GroupNotFound", result.Errors);
    }

    private static PaymentService CreateService(TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context, AppMapper mapper) =>
        new(new UnitOfWork(context), mapper, TestLocalizer.Instance);

    private static void SeedStudents(TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context)
    {
        context.AcademicYears.Find(1)!.MonthlyPrice = 600;
        context.Students.AddRange(
            new Student { Id = 101, FullName = "أحمد محمد علي", Mobile = "01000000001", AcademicYearId = 1, GroupId = 1, IsActive = true },
            new Student { Id = 102, FullName = "منى أحمد حسن", Mobile = "01000000002", AcademicYearId = 1, GroupId = 1, IsActive = true },
            new Student { Id = 103, FullName = "طالب غير نشط", Mobile = "01000000003", AcademicYearId = 1, GroupId = 1, IsActive = false },
            new Student { Id = 104, FullName = "طالب مجموعة أخرى", Mobile = "01000000004", AcademicYearId = 2, GroupId = 2, IsActive = true });
        context.SaveChanges();
    }

    private static async Task<MonthlyPayment> SeedPaymentAsync(
        TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context,
        PaymentStatus status,
        decimal paidAmount,
        decimal remainingAmount,
        DateTime? paymentDate = null)
    {
        context.AcademicYears.Find(1)!.MonthlyPrice = 600;
        context.Students.Add(new Student { Id = 201, FullName = "أحمد محمد علي", Mobile = "01000000111", AcademicYearId = 1, GroupId = 1, IsActive = true });
        var payment = new MonthlyPayment
        {
            StudentId = 201,
            GroupId = 1,
            AcademicYearId = 1,
            Month = 5,
            Year = 2026,
            RequiredAmount = 600,
            PaidAmount = paidAmount,
            RemainingAmount = remainingAmount,
            PaymentStatus = status,
            PaymentDate = paymentDate
        };
        context.MonthlyPayments.Add(payment);
        await context.SaveChangesAsync();
        return payment;
    }
}
