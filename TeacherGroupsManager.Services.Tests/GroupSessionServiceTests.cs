using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Services;

namespace TeacherGroupsManager.Services.Tests;

public class GroupSessionServiceTests : TestBase
{
    [Fact]
    public async Task StartAsync_Creates_Roster_And_Is_Idempotent()
    {
        var (context, _) = CreateContext();
        context.Students.AddRange(new Student { FullName = "One", GroupId = 1, AcademicYearId = 1 }, new Student { FullName = "Two", GroupId = 1, AcademicYearId = 1 });
        context.GroupSessions.Add(new GroupSession { GroupId = 1, SessionDate = new DateOnly(2026, 7, 1), PlannedStartTime = new TimeOnly(10, 0), PlannedEndTime = new TimeOnly(11, 0) });
        await context.SaveChangesAsync();
        var service = new GroupSessionService(new UnitOfWork(context), TestLocalizer.Instance);
        await service.StartAsync(1);
        var result = await service.StartAsync(1);
        Assert.False(result.Succeeded);
        Assert.Equal(2, await context.StudentSessionAttendances.CountAsync());
    }

    [Fact]
    public async Task CheckInAndCheckOut_Apply_Late_And_Early_Rules()
    {
        var (context, _) = CreateContext();
        context.Students.Add(new Student { FullName = "One", GroupId = 1, AcademicYearId = 1 });
        context.GroupSessions.Add(new GroupSession { GroupId = 1, SessionDate = new DateOnly(2026, 7, 1), PlannedStartTime = new TimeOnly(10, 0), PlannedEndTime = new TimeOnly(11, 0) });
        await context.SaveChangesAsync();
        var service = new GroupSessionService(new UnitOfWork(context), TestLocalizer.Instance);
        await service.StartAsync(1);
        var attendance = await context.StudentSessionAttendances.SingleAsync();
        var plannedStart = (await context.GroupSessions.SingleAsync()).SessionDate.ToDateTime((await context.GroupSessions.SingleAsync()).PlannedStartTime);
        var plannedEnd = (await context.GroupSessions.SingleAsync()).SessionDate.ToDateTime((await context.GroupSessions.SingleAsync()).PlannedEndTime);
        Assert.Equal(11, plannedEnd.Hour);
        Assert.True(plannedStart.AddMinutes(16) < plannedEnd.AddMinutes(-10));
        await service.CheckInAsync(attendance.Id, plannedStart.AddMinutes(15));
        await service.CheckOutAsync(attendance.Id, plannedStart.AddMinutes(16));
        var saved = await context.StudentSessionAttendances.SingleAsync();
        Assert.Equal(SessionAttendanceStatus.Late, saved.AttendanceStatus);
        Assert.Equal(15, saved.LateMinutes);
        Assert.Equal(DepartureStatus.LeftEarly, saved.DepartureStatus);
    }

    [Fact]
    public async Task CompleteAsync_Converts_Remaining_Records_To_Absent()
    {
        var (context, _) = CreateContext();
        context.Students.Add(new Student { FullName = "One", GroupId = 1, AcademicYearId = 1 });
        context.GroupSessions.Add(new GroupSession { GroupId = 1, SessionDate = new DateOnly(2026, 7, 1), PlannedStartTime = new TimeOnly(10, 0), PlannedEndTime = new TimeOnly(11, 0) });
        await context.SaveChangesAsync();
        var service = new GroupSessionService(new UnitOfWork(context), TestLocalizer.Instance);
        await service.StartAsync(1);
        var result = await service.CompleteAsync(1);
        Assert.True(result.Succeeded);
        Assert.Equal(SessionAttendanceStatus.Absent, (await context.StudentSessionAttendances.SingleAsync()).AttendanceStatus);
        Assert.Equal(GroupSessionStatus.Completed, (await context.GroupSessions.SingleAsync()).Status);
    }
}
