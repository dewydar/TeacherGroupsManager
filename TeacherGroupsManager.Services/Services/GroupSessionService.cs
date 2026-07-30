using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class GroupSessionService(IUnitOfWork unitOfWork, IStringLocalizer<SharedResource> localizer) : IGroupSessionService
{
    private const int LateGraceMinutes = 10;
    private const int EarlyDepartureThresholdMinutes = 10;

    public async Task<IReadOnlyList<GroupSessionDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await SessionsQuery().OrderByDescending(x => x.SessionDate).ThenByDescending(x => x.PlannedStartTime)
            .Select(x => new GroupSessionDto(x.Id, x.GroupId, x.Group.Name, x.SessionDate, x.PlannedStartTime, x.PlannedEndTime, x.ActualStartTime, x.ActualEndTime, x.Status, x.Topic, x.Attendances.Count)).ToListAsync(cancellationToken);

    public async Task<GroupSessionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await SessionsQuery().Where(x => x.Id == id).Select(x => new GroupSessionDto(x.Id, x.GroupId, x.Group.Name, x.SessionDate, x.PlannedStartTime, x.PlannedEndTime, x.ActualStartTime, x.ActualEndTime, x.Status, x.Topic, x.Attendances.Count)).FirstOrDefaultAsync(cancellationToken);

    public async Task<SessionAttendanceDto?> GetAttendanceAsync(int id, CancellationToken cancellationToken = default)
    {
        var session = await SessionsQuery().Include(x => x.Attendances).ThenInclude(x => x.Student).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (session is null) return null;
        var paymentMonth = session.SessionDate.Month;
        var paymentYear = session.SessionDate.Year;
        var studentIds = session.Attendances.Select(x => x.StudentId).ToArray();
        var payments = await unitOfWork.Repository<MonthlyPayment>().Query().Where(x => studentIds.Contains(x.StudentId) && x.Month == paymentMonth && x.Year == paymentYear).ToDictionaryAsync(x => x.StudentId, x => (PaymentStatus?)x.PaymentStatus, cancellationToken);
        return new SessionAttendanceDto(session.Id, session.Group.Name, session.SessionDate, session.PlannedStartTime, session.PlannedEndTime, session.Status,
            session.Attendances.OrderBy(x => x.Student.FullName).Select(x => new SessionAttendanceStudentDto(x.Id, x.StudentId, x.Student.FullName, x.Student.Mobile, x.AttendanceStatus, x.CheckInTime, x.LateMinutes, x.DepartureStatus, x.CheckOutTime, payments.GetValueOrDefault(x.StudentId), x.ExcuseReason, x.Notes)).ToList());
    }

    public async Task<OperationResult> CreateAsync(CreateGroupSessionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.PlannedEndTime <= dto.PlannedStartTime) return OperationResult.Failure(localizer["EndTimeAfterStartTime"]);
        if (!await unitOfWork.Repository<Group>().AnyAsync(x => x.Id == dto.GroupId, cancellationToken)) return OperationResult.Failure(localizer["GroupNotFound"]);
        if (await unitOfWork.Repository<GroupSession>().AnyAsync(x => x.GroupId == dto.GroupId && x.SessionDate == dto.SessionDate && x.PlannedStartTime == dto.PlannedStartTime, cancellationToken)) return OperationResult.Failure(localizer["DuplicateSession"]);
        await unitOfWork.Repository<GroupSession>().AddAsync(new GroupSession { GroupId = dto.GroupId, SessionDate = dto.SessionDate, PlannedStartTime = dto.PlannedStartTime, PlannedEndTime = dto.PlannedEndTime, Topic = dto.Topic?.Trim(), Notes = dto.Notes?.Trim() }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["SessionSaved"]);
    }

    public async Task<OperationResult> StartAsync(int id, CancellationToken cancellationToken = default)
    {
        var session = await SessionsQuery().Include(x => x.Attendances).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (session is null) return OperationResult.Failure(localizer["SessionNotFound"]);
        if (session.Status != GroupSessionStatus.Scheduled) return OperationResult.Failure(localizer["SessionMustBeScheduled"]);
        var students = await unitOfWork.Repository<Student>().Query().Where(x => x.GroupId == session.GroupId && x.IsActive).ToListAsync(cancellationToken);
        var existing = session.Attendances.Select(x => x.StudentId).ToHashSet();
        foreach (var student in students.Where(x => !existing.Contains(x.Id))) session.Attendances.Add(new StudentSessionAttendance { StudentId = student.Id, AttendanceStatus = SessionAttendanceStatus.NotRecorded, DepartureStatus = DepartureStatus.NotRecorded });
        session.Status = GroupSessionStatus.Started;
        session.ActualStartTime = DateTime.Now;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["SessionStarted"]);
    }

    public async Task<OperationResult> CheckInAsync(int attendanceId, DateTime checkInTime, CancellationToken cancellationToken = default)
    {
        var attendance = await unitOfWork.Repository<StudentSessionAttendance>().Query().Include(x => x.GroupSession).FirstOrDefaultAsync(x => x.Id == attendanceId, cancellationToken);
        if (attendance is null) return OperationResult.Failure(localizer["AttendanceNotFound"]);
        if (attendance.GroupSession.Status != GroupSessionStatus.Started) return OperationResult.Failure(localizer["SessionNotStarted"]);
        var planned = attendance.GroupSession.SessionDate.ToDateTime(attendance.GroupSession.PlannedStartTime);
        attendance.CheckInTime = checkInTime;
        attendance.CheckInMethod = AttendanceMethod.Manual;
        attendance.LateMinutes = Math.Max(0, (int)Math.Floor((checkInTime - planned).TotalMinutes));
        attendance.AttendanceStatus = attendance.LateMinutes > LateGraceMinutes ? SessionAttendanceStatus.Late : SessionAttendanceStatus.Present;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["AttendanceUpdated"]);
    }

    public async Task<OperationResult> CheckOutAsync(int attendanceId, DateTime checkOutTime, CancellationToken cancellationToken = default)
    {
        var attendance = await unitOfWork.Repository<StudentSessionAttendance>().Query().Include(x => x.GroupSession).FirstOrDefaultAsync(x => x.Id == attendanceId, cancellationToken);
        if (attendance?.CheckInTime is null) return OperationResult.Failure(localizer["CheckInRequired"]);
        if (attendance.GroupSession.Status != GroupSessionStatus.Started) return OperationResult.Failure(localizer["SessionNotStarted"]);
        if (checkOutTime < attendance.CheckInTime) return OperationResult.Failure(localizer["CheckOutAfterCheckIn"]);
        var plannedEnd = attendance.GroupSession.SessionDate.ToDateTime(attendance.GroupSession.PlannedEndTime);
        attendance.CheckOutTime = checkOutTime;
        attendance.CheckOutMethod = AttendanceMethod.Manual;
        attendance.DepartureStatus = checkOutTime < plannedEnd.AddMinutes(-EarlyDepartureThresholdMinutes) ? DepartureStatus.LeftEarly : DepartureStatus.Normal;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["CheckOutSaved"]);
    }

    public async Task<OperationResult> MarkAsync(int sessionId, int[] studentIds, SessionAttendanceStatus status, string? excuseReason = null, CancellationToken cancellationToken = default)
    {
        if (status == SessionAttendanceStatus.Excused && string.IsNullOrWhiteSpace(excuseReason)) return OperationResult.Failure(localizer["ExcuseReasonRequired"]);
        var session = await unitOfWork.Repository<GroupSession>().Query().Include(x => x.Attendances).FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
        if (session is null) return OperationResult.Failure(localizer["SessionNotFound"]);
        if (session.Status != GroupSessionStatus.Started) return OperationResult.Failure(localizer["SessionNotStarted"]);
        foreach (var item in session.Attendances.Where(x => studentIds.Contains(x.StudentId))) { item.AttendanceStatus = status; item.ExcuseReason = status == SessionAttendanceStatus.Excused ? excuseReason?.Trim() : null; }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["AttendanceUpdated"]);
    }

    public async Task<OperationResult> CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var session = await unitOfWork.Repository<GroupSession>().Query().Include(x => x.Attendances).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (session is null) return OperationResult.Failure(localizer["SessionNotFound"]);
        if (session.Status != GroupSessionStatus.Started) return OperationResult.Failure(localizer["SessionMustBeStarted"]);
        foreach (var item in session.Attendances.Where(x => x.AttendanceStatus == SessionAttendanceStatus.NotRecorded)) item.AttendanceStatus = SessionAttendanceStatus.Absent;
        session.Status = GroupSessionStatus.Completed;
        session.ActualEndTime = DateTime.Now;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["SessionCompleted"]);
    }

    private IQueryable<GroupSession> SessionsQuery() => unitOfWork.Repository<GroupSession>().Query().Include(x => x.Group);
}
