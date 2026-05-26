using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class PaymentServiceTests : TestBase
{
    [Fact]
    public async Task CreateAsync_Calculates_Remaining_And_Status()
    {
        var (context, mapper) = CreateContext();
        context.Students.Add(new Student { Id = 10, FullName = "Student", Mobile = "1", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();
        var service = new PaymentService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateMonthlyPaymentDto(10, 1, 1, 5, 2026, 300, 100, PaymentStatus.PartiallyPaid, DateTime.Today, null, null));

        Assert.True(result.Succeeded);
        var payment = await context.MonthlyPayments.FirstAsync();
        Assert.Equal(200, payment.RemainingAmount);
        Assert.Equal(PaymentStatus.PartiallyPaid, payment.PaymentStatus);
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Student_Does_Not_Exist()
    {
        var (context, mapper) = CreateContext();
        var service = new PaymentService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var result = await service.CreateAsync(new CreateMonthlyPaymentDto(999, 1, 1, 5, 2026, 300, 100, PaymentStatus.PartiallyPaid, DateTime.Today, null, null));

        Assert.False(result.Succeeded);
        Assert.Empty(context.MonthlyPayments);
    }

    [Fact]
    public async Task CreateAsync_Fails_When_Student_Month_And_Year_Already_Exist()
    {
        var (context, mapper) = CreateContext();
        context.Students.Add(new Student { Id = 10, FullName = "Student", Mobile = "1", AcademicYearId = 1, GroupId = 1 });
        await context.SaveChangesAsync();
        var service = new PaymentService(new UnitOfWork(context), mapper, TestLocalizer.Instance);

        var first = await service.CreateAsync(new CreateMonthlyPaymentDto(10, 1, 1, 5, 2026, 300, 100, PaymentStatus.PartiallyPaid, DateTime.Today, null, null));
        var duplicate = await service.CreateAsync(new CreateMonthlyPaymentDto(10, 1, 1, 5, 2026, 300, 300, PaymentStatus.Paid, DateTime.Today, null, null));

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Single(context.MonthlyPayments);
    }

    [Fact]
    public async Task UpdateAsync_Recalculates_Remaining_And_Status()
    {
        var (context, mapper) = CreateContext();
        context.Students.Add(new Student { Id = 20, FullName = "Payment Student", Mobile = "1", AcademicYearId = 1, GroupId = 1 });
        context.MonthlyPayments.Add(new MonthlyPayment
        {
            Id = 30,
            StudentId = 20,
            GroupId = 1,
            AcademicYearId = 1,
            Month = 5,
            Year = 2026,
            RequiredAmount = 300,
            PaidAmount = 0,
            RemainingAmount = 300,
            PaymentStatus = PaymentStatus.Unpaid
        });
        await context.SaveChangesAsync();
        var service = new PaymentService(new UnitOfWork(context, new TestCurrentUserContext(1)), mapper, TestLocalizer.Instance);

        var result = await service.UpdateAsync(new EditMonthlyPaymentDto(30, 20, 1, 1, 5, 2026, 300, 300, PaymentStatus.Unpaid, DateTime.Today, "Paid", null));

        Assert.True(result.Succeeded);
        var payment = await context.MonthlyPayments.SingleAsync(x => x.Id == 30);
        Assert.Equal(0, payment.RemainingAmount);
        Assert.Equal(PaymentStatus.Paid, payment.PaymentStatus);
        Assert.Equal(1, payment.UpdatedByEmployeeId);
    }
}


