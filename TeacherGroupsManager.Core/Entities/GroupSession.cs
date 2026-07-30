using TeacherGroupsManager.Core.Common;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Core.Entities;

public class GroupSession : BaseEntity
{
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public DateOnly SessionDate { get; set; }
    public TimeOnly PlannedStartTime { get; set; }
    public TimeOnly PlannedEndTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public GroupSessionStatus Status { get; set; } = GroupSessionStatus.Scheduled;
    public string? Topic { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public ICollection<StudentSessionAttendance> Attendances { get; set; } = new List<StudentSessionAttendance>();
}
