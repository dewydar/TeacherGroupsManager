using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class TestDataSeederTests : TestBase
{
    [Fact]
    public async Task SeedAsync_CreatesArabicTestData()
    {
        var (context, _) = CreateContext();
        var seeder = CreateSeeder(context);

        var summary = await seeder.SeedAsync();

        Assert.Equal(5, summary.TeachersAdded);
        Assert.Equal(5, summary.AssistantTeachersAdded);
        Assert.Equal(6, summary.AcademicYearsAdded);
        Assert.Equal(18, summary.GroupsAdded);
        Assert.Equal(180, summary.StudentsAdded);
        Assert.Equal(360, summary.LessonsAdded);
        Assert.Equal(1800, summary.MonthlyPaymentsAdded);
        Assert.Equal("تم توليد البيانات التجريبية بنجاح", summary.Message);
        Assert.Equal(5, await context.Employees.CountAsync(x => x.Role.Name == AppConstants.TeacherRole && x.Username.StartsWith("teacher")));
        Assert.Equal(5, await context.Employees.CountAsync(x => x.Role.Name == AppConstants.AssistantTeacherRole && x.Username.StartsWith("assistant")));
        Assert.Equal(18, await context.Groups.CountAsync(x => x.GroupType == GroupType.Public && x.Name.Contains("2025 / 2026")));
        Assert.Contains(await context.MonthlyPayments.Select(x => x.PaymentStatus).Distinct().ToListAsync(), x => x == PaymentStatus.Paid);
        Assert.Contains(await context.MonthlyPayments.Select(x => x.PaymentStatus).Distinct().ToListAsync(), x => x == PaymentStatus.Unpaid);
        Assert.Contains(await context.MonthlyPayments.Select(x => x.PaymentStatus).Distinct().ToListAsync(), x => x == PaymentStatus.PartiallyPaid);
    }

    [Fact]
    public async Task SeedAsync_WhenRunTwice_DoesNotDuplicateData()
    {
        var (context, _) = CreateContext();
        var seeder = CreateSeeder(context);

        await seeder.SeedAsync();
        var secondSummary = await seeder.SeedAsync();

        Assert.Equal(0, secondSummary.TeachersAdded);
        Assert.Equal(0, secondSummary.AssistantTeachersAdded);
        Assert.Equal(0, secondSummary.AcademicYearsAdded);
        Assert.Equal(0, secondSummary.GroupsAdded);
        Assert.Equal(0, secondSummary.StudentsAdded);
        Assert.Equal(0, secondSummary.LessonsAdded);
        Assert.Equal(0, secondSummary.MonthlyPaymentsAdded);
        Assert.Equal(1800, await context.MonthlyPayments.CountAsync());
        Assert.Equal(
            await context.MonthlyPayments.CountAsync(),
            await context.MonthlyPayments
                .Select(x => new { x.StudentId, x.GroupId, x.AcademicYearId, x.Month, x.Year })
                .Distinct()
                .CountAsync());
    }

    private static TestDataSeeder CreateSeeder(TeacherGroupsManager.Data.Context.TeacherGroupsDbContext context) =>
        new(new UnitOfWork(context), new Pbkdf2PasswordHasher());
}
