using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Extensions;

namespace TeacherGroupsManager.Services.Services;

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
            g.Students.Count(s => s.MonthlyPayments.Any(p => p.Month == now.Month && p.Year == now.Year && p.PaymentStatus == PaymentStatus.Unpaid)),
            g.Students.Count(s => s.MonthlyPayments.Any(p => p.Month == now.Month && p.Year == now.Year && p.PaymentStatus == PaymentStatus.PartiallyPaid)),
            g.Students.SelectMany(s => s.MonthlyPayments).Where(p => p.Month == now.Month && p.Year == now.Year).Sum(p => p.PaidAmount),
            g.Students.SelectMany(s => s.MonthlyPayments).Where(p => p.Month == now.Month && p.Year == now.Year).Sum(p => p.RemainingAmount),
            $"/Groups/Details/{g.Id}")).ToListAsync(cancellationToken);

        var groupsByDay = await unitOfWork.Repository<GroupSchedule>().Query()
            .GroupBy(x => x.DayOfWeek)
            .Select(x => new GroupDayDto(x.Key, x.Key.DayToArabic(), x.Select(schedule => schedule.GroupId).Distinct().Count(), x.Select(schedule => schedule.Group.Name).Distinct().ToList()))
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
            await payments.CountAsync(x => x.PaymentStatus == PaymentStatus.PartiallyPaid, cancellationToken),
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
