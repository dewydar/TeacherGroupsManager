using TeacherGroupsManager.Core.Constants;

namespace TeacherGroupsManager.WebUI.Security;

public static class PermissionPolicy
{
    public static readonly string[] All =
    [
        PermissionCodes.RolesManage,
        PermissionCodes.EmployeesManage,
        PermissionCodes.AcademicYearsManage,
        PermissionCodes.GroupsManage,
        PermissionCodes.StudentsManage,
        PermissionCodes.LessonsManage,
        PermissionCodes.PaymentsManage,
        PermissionCodes.ReportsView,
        PermissionCodes.DashboardView
    ];

    public static IReadOnlyList<string> ForRole(string roleName) => roleName switch
    {
        AppConstants.AdminRole => All,
        AppConstants.TeacherRole =>
        [
            PermissionCodes.EmployeesManage,
            PermissionCodes.AcademicYearsManage,
            PermissionCodes.GroupsManage,
            PermissionCodes.StudentsManage,
            PermissionCodes.LessonsManage,
            PermissionCodes.PaymentsManage,
            PermissionCodes.ReportsView,
            PermissionCodes.DashboardView
        ],
        _ => [PermissionCodes.DashboardView]
    };
}
