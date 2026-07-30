using System.ComponentModel.DataAnnotations;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Dtos;

public record GroupSessionDto(int Id, int GroupId, string GroupName, DateOnly SessionDate, TimeOnly PlannedStartTime, TimeOnly PlannedEndTime, DateTime? ActualStartTime, DateTime? ActualEndTime, GroupSessionStatus Status, string? Topic, int AttendanceCount);
public record CreateGroupSessionDto(int GroupId, DateOnly SessionDate, TimeOnly PlannedStartTime, TimeOnly PlannedEndTime, [property: StringLength(AppConstants.MaxStringLength)] string? Topic, [property: StringLength(AppConstants.MaxStringLength)] string? Notes);
public record SessionAttendanceStudentDto(int AttendanceId, int StudentId, string StudentName, string Mobile, SessionAttendanceStatus AttendanceStatus, DateTime? CheckInTime, int LateMinutes, DepartureStatus DepartureStatus, DateTime? CheckOutTime, PaymentStatus? PaymentStatus, string? ExcuseReason, string? Notes);
public record SessionAttendanceDto(int SessionId, string GroupName, DateOnly SessionDate, TimeOnly PlannedStartTime, TimeOnly PlannedEndTime, GroupSessionStatus Status, IReadOnlyList<SessionAttendanceStudentDto> Students);
