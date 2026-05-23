using AutoMapper;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Dtos;

namespace TeacherGroupsManager.Services.Mapping;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        CreateMap<Role, RoleDto>();
        CreateMap<Permission, PermissionDto>();
        CreateMap<Employee, EmployeeDto>()
            .ForCtorParam("RoleName", opt => opt.MapFrom(src => src.Role.Name))
            .ForCtorParam("RoleArabicName", opt => opt.MapFrom(src => src.Role.ArabicName));
        CreateMap<AcademicYear, AcademicYearDto>();
        CreateMap<Group, GroupDto>()
            .ForCtorParam("AcademicYearName", opt => opt.MapFrom(src => src.AcademicYear.Name));
        CreateMap<Student, StudentDto>()
            .ForCtorParam("AcademicYearName", opt => opt.MapFrom(src => src.AcademicYear.Name))
            .ForCtorParam("GroupName", opt => opt.MapFrom(src => src.Group.Name));
        CreateMap<Lesson, LessonDto>()
            .ForCtorParam("GroupName", opt => opt.MapFrom(src => src.Group.Name));
        CreateMap<MonthlyPayment, MonthlyPaymentDto>()
            .ForCtorParam("StudentName", opt => opt.MapFrom(src => src.Student.FullName))
            .ForCtorParam("GroupName", opt => opt.MapFrom(src => src.Group.Name))
            .ForCtorParam("AcademicYearName", opt => opt.MapFrom(src => src.AcademicYear.Name));
    }
}
