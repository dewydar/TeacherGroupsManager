namespace TeacherGroupsManager.Core.Constants;

public static class AppConstants
{
    public const int MaxStringLength = 500;
    public const int MobileMaxLength = 11;
    public const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string ArabicCulture = "ar-EG";
    public const string AdminRole = "Admin";
    public const string SystemAdminUsername = "admin";
    public const string TeacherRole = "Teacher";
    public const string AssistantTeacherRole = "AssistantTeacher";
    public const string AuthCookieName = "TeacherGroupsManager.Auth";
}

public static class PermissionCodes
{
    public const string RolesManage = "Roles.Manage";
    public const string EmployeesManage = "Employees.Manage";
    public const string AcademicYearsManage = "AcademicYears.Manage";
    public const string GroupsManage = "Groups.Manage";
    public const string StudentsManage = "Students.Manage";
    public const string LessonsManage = "Lessons.Manage";
    public const string PaymentsManage = "Payments.Manage";
    public const string ReportsView = "Reports.View";
    public const string DashboardView = "Dashboard.View";
}
