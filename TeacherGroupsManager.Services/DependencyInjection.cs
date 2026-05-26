using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Services.Security;
using TeacherGroupsManager.Services.Services;
using TeacherGroupsManager.Services.Validation;

namespace TeacherGroupsManager.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddTeacherGroupsServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<AppMapper>();
        services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
