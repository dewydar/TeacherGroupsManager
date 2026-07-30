using TeacherGroupsManager.Core.Common;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Core.Entities;

public class StudentSessionAttendance : BaseEntity
{
    public int GroupSessionId { get; set; }
    public GroupSession GroupSession { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public SessionAttendanceStatus AttendanceStatus { get; set; }
    public DateTime? CheckInTime { get; set; }
    public AttendanceMethod? CheckInMethod { get; set; }
    public int LateMinutes { get; set; }
    public DepartureStatus DepartureStatus { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public AttendanceMethod? CheckOutMethod { get; set; }
    public string? ExcuseReason { get; set; }
    public string? Notes { get; set; }
}
