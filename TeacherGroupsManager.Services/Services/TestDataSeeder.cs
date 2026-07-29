using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Security;

namespace TeacherGroupsManager.Services.Services;

public class TestDataSeeder(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) : ITestDataSeeder
{
    private const string TestPassword = "Test@12345";

    private static readonly (string Name, string Username, string Email, string Mobile)[] Teachers =
    [
        ("أحمد محمد علي", "teacher1", "teacher1@test.com", "01000000001"),
        ("محمد حسن إبراهيم", "teacher2", "teacher2@test.com", "01000000002"),
        ("محمود عبد الله", "teacher3", "teacher3@test.com", "01000000003"),
        ("خالد سعيد محمد", "teacher4", "teacher4@test.com", "01000000004"),
        ("يوسف مصطفى علي", "teacher5", "teacher5@test.com", "01000000005")
    ];

    private static readonly (string Name, string Username, string Email, string Mobile)[] AssistantTeachers =
    [
        ("عمر أحمد محمد", "assistant1", "assistant1@test.com", "01100000001"),
        ("علي محمود حسن", "assistant2", "assistant2@test.com", "01100000002"),
        ("كريم خالد علي", "assistant3", "assistant3@test.com", "01100000003"),
        ("زياد محمد أحمد", "assistant4", "assistant4@test.com", "01100000004"),
        ("حسن علي محمود", "assistant5", "assistant5@test.com", "01100000005")
    ];

    private static readonly (string Name, decimal MonthlyPrice)[] AcademicYears =
    [
        ("الصف الأول الإعدادي - 2025 / 2026", 300),
        ("الصف الثاني الإعدادي - 2025 / 2026", 350),
        ("الصف الثالث الإعدادي - 2025 / 2026", 400),
        ("الصف الأول الثانوي - 2025 / 2026", 500),
        ("الصف الثاني الثانوي - 2025 / 2026", 600),
        ("الصف الثالث الثانوي - 2025 / 2026", 700)
    ];

    private static readonly (string Name, DayOfWeek[] Days, TimeOnly Start, TimeOnly End, decimal? ExtraPrice)[] GroupTemplates =
    [
        ("مجموعة السبت والثلاثاء", [DayOfWeek.Saturday, DayOfWeek.Tuesday], new TimeOnly(17, 0), new TimeOnly(19, 0), null),
        ("مجموعة الأحد والأربعاء", [DayOfWeek.Sunday, DayOfWeek.Wednesday], new TimeOnly(18, 0), new TimeOnly(20, 0), 50),
        ("مجموعة الإثنين والخميس", [DayOfWeek.Monday, DayOfWeek.Thursday], new TimeOnly(19, 0), new TimeOnly(21, 0), 100)
    ];

    private static readonly string[] StudentNames =
    [
        "أحمد محمود", "محمد علي", "يوسف أحمد", "عمر خالد", "كريم حسن",
        "عبد الرحمن محمد", "مصطفى سعيد", "علي إبراهيم", "زياد طارق", "حسن ياسر",
        "مروان محمد", "سيف خالد", "حمزة أحمد", "مالك محمود", "ياسين علي"
    ];

    private static readonly string[] LessonTitles =
    [
        "شرح الدرس الأول",
        "حل تدريبات",
        "مراجعة شهرية",
        "اختبار قصير"
    ];

    private static readonly (int Month, int Year)[] PaymentMonths =
    [
        (9, 2025), (10, 2025), (11, 2025), (12, 2025),
        (1, 2026), (2, 2026), (3, 2026), (4, 2026), (5, 2026), (6, 2026)
    ];

    public async Task<TestDataSeedSummaryDto> SeedAsync(CancellationToken cancellationToken = default)
    {
        var summary = new TestDataSeedSummaryDto();

        await SeedEmployeesAsync(summary, cancellationToken);
        await SeedAcademicYearsAsync(summary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await SeedGroupsAsync(summary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await SeedStudentsAsync(summary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await SeedLessonsAsync(summary, cancellationToken);
        await SeedMonthlyPaymentsAsync(summary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return summary;
    }

    private async Task SeedEmployeesAsync(TestDataSeedSummaryDto summary, CancellationToken cancellationToken)
    {
        var teacherRole = await unitOfWork.Repository<Role>().Query().SingleAsync(x => x.Name == AppConstants.TeacherRole, cancellationToken);
        var assistantRole = await unitOfWork.Repository<Role>().Query().SingleAsync(x => x.Name == AppConstants.AssistantTeacherRole, cancellationToken);
        var existingUsernames = await unitOfWork.Repository<Employee>().Query()
            .Select(x => x.Username.ToLower())
            .ToListAsync(cancellationToken);
        var usernames = existingUsernames.ToHashSet();
        var passwordHash = passwordHasher.Hash(TestPassword);

        foreach (var teacher in Teachers)
        {
            if (usernames.Contains(teacher.Username.ToLower()))
            {
                summary.SkippedDuplicates++;
                continue;
            }

            await unitOfWork.Repository<Employee>().AddAsync(new Employee
            {
                FullName = teacher.Name,
                Username = teacher.Username,
                Email = teacher.Email,
                Mobile = teacher.Mobile,
                PasswordHash = passwordHash,
                RoleId = teacherRole.Id,
                IsActive = true
            }, cancellationToken);
            usernames.Add(teacher.Username.ToLower());
            summary.TeachersAdded++;
        }

        foreach (var assistant in AssistantTeachers)
        {
            if (usernames.Contains(assistant.Username.ToLower()))
            {
                summary.SkippedDuplicates++;
                continue;
            }

            await unitOfWork.Repository<Employee>().AddAsync(new Employee
            {
                FullName = assistant.Name,
                Username = assistant.Username,
                Email = assistant.Email,
                Mobile = assistant.Mobile,
                PasswordHash = passwordHash,
                RoleId = assistantRole.Id,
                IsActive = true
            }, cancellationToken);
            usernames.Add(assistant.Username.ToLower());
            summary.AssistantTeachersAdded++;
        }
    }

    private async Task SeedAcademicYearsAsync(TestDataSeedSummaryDto summary, CancellationToken cancellationToken)
    {
        var existingNames = await unitOfWork.Repository<AcademicYear>().Query()
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
        var names = existingNames.ToHashSet();

        foreach (var year in AcademicYears)
        {
            if (names.Contains(year.Name))
            {
                summary.SkippedDuplicates++;
                continue;
            }

            await unitOfWork.Repository<AcademicYear>().AddAsync(new AcademicYear
            {
                Name = year.Name,
                StartDate = new DateOnly(2025, 9, 1),
                EndDate = new DateOnly(2026, 6, 30),
                MonthlyPrice = year.MonthlyPrice,
                IsActive = true
            }, cancellationToken);
            names.Add(year.Name);
            summary.AcademicYearsAdded++;
        }
    }

    private async Task SeedGroupsAsync(TestDataSeedSummaryDto summary, CancellationToken cancellationToken)
    {
        var academicYears = await unitOfWork.Repository<AcademicYear>().Query()
            .Where(x => AcademicYears.Select(y => y.Name).Contains(x.Name))
            .OrderBy(x => x.MonthlyPrice)
            .ToListAsync(cancellationToken);

        var existingGroups = await unitOfWork.Repository<Group>().Query()
            .Select(x => new { x.AcademicYearId, x.Name })
            .ToListAsync(cancellationToken);
        var groupKeys = existingGroups.Select(x => $"{x.AcademicYearId}|{x.Name}").ToHashSet();

        foreach (var academicYear in academicYears)
        {
            foreach (var template in GroupTemplates)
            {
                var groupName = $"{template.Name} - {academicYear.Name}";
                var key = $"{academicYear.Id}|{groupName}";
                if (groupKeys.Contains(key))
                {
                    summary.SkippedDuplicates++;
                    continue;
                }

                var monthlyPrice = template.ExtraPrice.HasValue ? academicYear.MonthlyPrice + template.ExtraPrice.Value : (decimal?)null;
                await unitOfWork.Repository<Group>().AddAsync(new Group
                {
                    Name = groupName,
                    AcademicYearId = academicYear.Id,
                    GroupType = GroupType.Public,
                    DayOfWeek = template.Days[0],
                    StartTime = template.Start,
                    EndTime = template.End,
                    DefaultLessonPrice = monthlyPrice ?? academicYear.MonthlyPrice,
                    MonthlyPrice = monthlyPrice,
                    IsActive = true,
                    Schedules = template.Days
                        .Select(day => new GroupSchedule { DayOfWeek = day, StartTime = template.Start, EndTime = template.End })
                        .ToList()
                }, cancellationToken);
                groupKeys.Add(key);
                summary.GroupsAdded++;
            }
        }
    }

    private async Task SeedStudentsAsync(TestDataSeedSummaryDto summary, CancellationToken cancellationToken)
    {
        var groups = await unitOfWork.Repository<Group>().Query()
            .Where(x => x.GroupType == GroupType.Public && AcademicYears.Select(y => y.Name).Contains(x.AcademicYear.Name))
            .OrderBy(x => x.AcademicYear.MonthlyPrice)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
        var existingMobiles = await unitOfWork.Repository<Student>().Query()
            .Select(x => x.Mobile)
            .ToListAsync(cancellationToken);
        var mobiles = existingMobiles.ToHashSet();

        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            for (var studentIndex = 0; studentIndex < 10; studentIndex++)
            {
                var serial = groupIndex * 10 + studentIndex + 1;
                var mobile = $"012{serial:00000000}";
                if (mobiles.Contains(mobile))
                {
                    summary.SkippedDuplicates++;
                    continue;
                }

                await unitOfWork.Repository<Student>().AddAsync(new Student
                {
                    FullName = $"{StudentNames[studentIndex % StudentNames.Length]} {groupIndex + 1}",
                    Mobile = mobile,
                    ParentMobile = $"015{serial:00000000}",
                    AcademicYearId = group.AcademicYearId,
                    GroupId = group.Id,
                    Notes = studentIndex % 3 == 0 ? "بيانات تجريبية لاختبار النظام" : null,
                    IsActive = true
                }, cancellationToken);
                mobiles.Add(mobile);
                summary.StudentsAdded++;
            }
        }
    }

    private async Task SeedLessonsAsync(TestDataSeedSummaryDto summary, CancellationToken cancellationToken)
    {
        var groups = await unitOfWork.Repository<Group>().Query()
            .Include(x => x.AcademicYear)
            .Where(x => x.GroupType == GroupType.Public && AcademicYears.Select(y => y.Name).Contains(x.AcademicYear.Name))
            .ToListAsync(cancellationToken);

        var existingLessons = await unitOfWork.Repository<Lesson>().Query()
            .Select(x => new { x.GroupId, x.Month, x.Year, x.Title, x.LessonDate })
            .ToListAsync(cancellationToken);
        var lessonKeys = existingLessons.Select(x => $"{x.GroupId}|{x.Year}|{x.Month}|{x.Title}|{x.LessonDate:yyyyMMdd}").ToHashSet();

        foreach (var group in groups)
        {
            foreach (var (month, year) in PaymentMonths)
            {
                for (var lessonIndex = 0; lessonIndex < 2; lessonIndex++)
                {
                    var title = LessonTitles[(month + lessonIndex) % LessonTitles.Length];
                    var lessonDate = new DateTime(year, month, lessonIndex == 0 ? 7 : 21, group.StartTime.Hour, group.StartTime.Minute, 0);
                    var key = $"{group.Id}|{year}|{month}|{title}|{lessonDate:yyyyMMdd}";
                    if (lessonKeys.Contains(key))
                    {
                        summary.SkippedDuplicates++;
                        continue;
                    }

                    await unitOfWork.Repository<Lesson>().AddAsync(new Lesson
                    {
                        Title = title,
                        Description = "درس تجريبي لاختبار الحضور والتقارير",
                        GroupId = group.Id,
                        LessonType = LessonType.Group,
                        LessonDate = lessonDate,
                        Price = group.MonthlyPrice ?? group.AcademicYear.MonthlyPrice,
                        IsMonthlyPaymentRequired = true,
                        Month = month,
                        Year = year
                    }, cancellationToken);
                    lessonKeys.Add(key);
                    summary.LessonsAdded++;
                }
            }
        }
    }

    private async Task SeedMonthlyPaymentsAsync(TestDataSeedSummaryDto summary, CancellationToken cancellationToken)
    {
        var students = await unitOfWork.Repository<Student>().Query()
            .Include(x => x.Group)
            .Include(x => x.AcademicYear)
            .Where(x => x.IsActive && AcademicYears.Select(y => y.Name).Contains(x.AcademicYear.Name))
            .OrderBy(x => x.GroupId)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var existingPayments = await unitOfWork.Repository<MonthlyPayment>().Query()
            .Select(x => new { x.StudentId, x.GroupId, x.AcademicYearId, x.Month, x.Year })
            .ToListAsync(cancellationToken);
        var paymentKeys = existingPayments.Select(x => $"{x.StudentId}|{x.GroupId}|{x.AcademicYearId}|{x.Month}|{x.Year}").ToHashSet();

        foreach (var student in students)
        {
            foreach (var (month, year) in PaymentMonths)
            {
                var key = $"{student.Id}|{student.GroupId}|{student.AcademicYearId}|{month}|{year}";
                if (paymentKeys.Contains(key))
                {
                    summary.SkippedDuplicates++;
                    continue;
                }

                var requiredAmount = student.Group.MonthlyPrice ?? student.AcademicYear.MonthlyPrice;
                var statusBucket = (student.Id + month + year) % 20;
                var status = statusBucket < 12
                    ? PaymentStatus.Paid
                    : statusBucket < 17
                        ? PaymentStatus.Unpaid
                        : PaymentStatus.PartiallyPaid;
                var paidAmount = status switch
                {
                    PaymentStatus.Paid => requiredAmount,
                    PaymentStatus.PartiallyPaid => Math.Round(requiredAmount * (0.3m + ((student.Id + month) % 5 * 0.1m)), 2),
                    _ => 0
                };

                await unitOfWork.Repository<MonthlyPayment>().AddAsync(new MonthlyPayment
                {
                    StudentId = student.Id,
                    GroupId = student.GroupId,
                    AcademicYearId = student.AcademicYearId,
                    Month = month,
                    Year = year,
                    RequiredAmount = requiredAmount,
                    PaidAmount = paidAmount,
                    RemainingAmount = requiredAmount - paidAmount,
                    PaymentStatus = status,
                    PaymentDate = paidAmount > 0 ? new DateTime(year, month, ((student.Id + month) % 25) + 1) : null,
                    Notes = "بيان تجريبي"
                }, cancellationToken);
                paymentKeys.Add(key);
                summary.MonthlyPaymentsAdded++;
            }
        }
    }
}
