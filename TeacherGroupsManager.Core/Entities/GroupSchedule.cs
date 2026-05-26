using TeacherGroupsManager.Core.Common;

namespace TeacherGroupsManager.Core.Entities;

public class GroupSchedule : BaseEntity
{
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
