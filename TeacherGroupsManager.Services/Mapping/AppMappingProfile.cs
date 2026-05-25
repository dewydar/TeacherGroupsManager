using AutoMapper;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Dtos;

namespace TeacherGroupsManager.Services.Mapping;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        CreateMap<Role, RoleDto>()
            .ForCtorParam("CreatedByEmployeeName", opt => opt.MapFrom(src => src.CreatedByEmployee == null ? null : src.CreatedByEmployee.FullName))
            .ForCtorParam("UpdatedByEmployeeName", opt => opt.MapFrom(src => src.UpdatedByEmployee == null ? null : src.UpdatedByEmployee.FullName));
        CreateMap<Permission, PermissionDto>()
            .ForCtorParam("CreatedByEmployeeName", opt => opt.MapFrom(src => src.CreatedByEmployee == null ? null : src.CreatedByEmployee.FullName))
            .ForCtorParam("UpdatedByEmployeeName", opt => opt.MapFrom(src => src.UpdatedByEmployee == null ? null : src.UpdatedByEmployee.FullName));
        CreateMap<Employee, EmployeeDto>()
            .ForCtorParam("RoleName", opt => opt.MapFrom(src => src.Role.Name))
            .ForCtorParam("RoleArabicName", opt => opt.MapFrom(src => src.Role.ArabicName))
            .ForCtorParam("Permissions", opt => opt.MapFrom(src => src.Role.RolePermissions.Select(rp => rp.Permission.Code).ToList()))
            .ForCtorParam("CreatedByEmployeeName", opt => opt.MapFrom(src => src.CreatedByEmployee == null ? null : src.CreatedByEmployee.FullName))
            .ForCtorParam("UpdatedByEmployeeName", opt => opt.MapFrom(src => src.UpdatedByEmployee == null ? null : src.UpdatedByEmployee.FullName));
        CreateMap<AcademicYear, AcademicYearDto>()
            .ForCtorParam("CreatedByEmployeeName", opt => opt.MapFrom(src => src.CreatedByEmployee == null ? null : src.CreatedByEmployee.FullName))
            .ForCtorParam("UpdatedByEmployeeName", opt => opt.MapFrom(src => src.UpdatedByEmployee == null ? null : src.UpdatedByEmployee.FullName));
        CreateMap<Group, GroupDto>()
            .ForCtorParam("AcademicYearName", opt => opt.MapFrom(src => src.AcademicYear.Name))
            .ForCtorParam("CreatedByEmployeeName", opt => opt.MapFrom(src => src.CreatedByEmployee == null ? null : src.CreatedByEmployee.FullName))
            .ForCtorParam("UpdatedByEmployeeName", opt => opt.MapFrom(src => src.UpdatedByEmployee == null ? null : src.UpdatedByEmployee.FullName));
        CreateMap<Student, StudentDto>()
            .ForCtorParam("AcademicYearName", opt => opt.MapFrom(src => src.AcademicYear.Name))
            .ForCtorParam("GroupName", opt => opt.MapFrom(src => src.Group.Name))
            .ForCtorParam("CreatedByEmployeeName", opt => opt.MapFrom(src => src.CreatedByEmployee == null ? null : src.CreatedByEmployee.FullName))
            .ForCtorParam("UpdatedByEmployeeName", opt => opt.MapFrom(src => src.UpdatedByEmployee == null ? null : src.UpdatedByEmployee.FullName));
        CreateMap<Lesson, LessonDto>()
            .ForCtorParam("GroupName", opt => opt.MapFrom(src => src.Group.Name))
            .ForCtorParam("CreatedByEmployeeName", opt => opt.MapFrom(src => src.CreatedByEmployee == null ? null : src.CreatedByEmployee.FullName))
            .ForCtorParam("UpdatedByEmployeeName", opt => opt.MapFrom(src => src.UpdatedByEmployee == null ? null : src.UpdatedByEmployee.FullName));
        CreateMap<MonthlyPayment, MonthlyPaymentDto>()
            .ForCtorParam("StudentName", opt => opt.MapFrom(src => src.Student.FullName))
            .ForCtorParam("GroupName", opt => opt.MapFrom(src => src.Group.Name))
            .ForCtorParam("AcademicYearName", opt => opt.MapFrom(src => src.AcademicYear.Name))
            .ForCtorParam("CreatedByEmployeeName", opt => opt.MapFrom(src => src.CreatedByEmployee == null ? null : src.CreatedByEmployee.FullName))
            .ForCtorParam("UpdatedByEmployeeName", opt => opt.MapFrom(src => src.UpdatedByEmployee == null ? null : src.UpdatedByEmployee.FullName));
    }
}
