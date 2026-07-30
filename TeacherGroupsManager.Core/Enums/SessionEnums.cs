namespace TeacherGroupsManager.Core.Enums;

public enum GroupSessionStatus { Scheduled = 1, Started = 2, Completed = 3, Cancelled = 4 }
public enum SessionAttendanceStatus { NotRecorded = 0, Present = 1, Late = 2, Absent = 3, Excused = 4 }
public enum DepartureStatus { NotRecorded = 0, Normal = 1, LeftEarly = 2 }
public enum AttendanceMethod { Manual = 1, QR = 2, StudentCode = 3 }
