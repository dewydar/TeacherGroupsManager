using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Common;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Core.Enums;

namespace TeacherGroupsManager.Data.Context;

public class TeacherGroupsDbContext(DbContextOptions<TeacherGroupsDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupSchedule> GroupSchedules => Set<GroupSchedule>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonStudent> LessonStudents => Set<LessonStudent>();
    public DbSet<MonthlyPayment> MonthlyPayments => Set<MonthlyPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
        modelBuilder.Entity<LessonStudent>().HasKey(x => new { x.LessonId, x.StudentId });

        modelBuilder.Entity<Group>().Property(x => x.DefaultLessonPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<GroupSchedule>().HasIndex(x => new { x.GroupId, x.DayOfWeek, x.StartTime });
        modelBuilder.Entity<Lesson>().Property(x => x.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<LessonStudent>().Property(x => x.AttendanceStatus).HasDefaultValue(AttendanceStatus.Present);
        modelBuilder.Entity<MonthlyPayment>().Property(x => x.RequiredAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<MonthlyPayment>().Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<MonthlyPayment>().Property(x => x.RemainingAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Employee>().HasIndex(x => x.Username).IsUnique();

        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties())
            .Where(property => property.ClrType == typeof(string)))
        {
            property.SetMaxLength(AppConstants.MaxStringLength);
        }

        modelBuilder.Entity<Employee>().Property(x => x.Mobile).HasMaxLength(AppConstants.MobileMaxLength);
        modelBuilder.Entity<Employee>().Property(x => x.Username).HasMaxLength(450);
        modelBuilder.Entity<Student>().Property(x => x.Mobile).HasMaxLength(AppConstants.MobileMaxLength);
        modelBuilder.Entity<Student>().Property(x => x.ParentMobile).HasMaxLength(AppConstants.MobileMaxLength);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(x => typeof(IAuditableEntity).IsAssignableFrom(x.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .HasOne(typeof(Employee), nameof(IAuditableEntity.CreatedByEmployee))
                .WithMany()
                .HasForeignKey(nameof(IAuditableEntity.CreatedByEmployeeId))
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity(entityType.ClrType)
                .HasOne(typeof(Employee), nameof(IAuditableEntity.UpdatedByEmployee))
                .WithMany()
                .HasForeignKey(nameof(IAuditableEntity.UpdatedByEmployeeId))
                .OnDelete(DeleteBehavior.Restrict);
        }

        modelBuilder.Entity<GroupSchedule>()
            .HasOne(x => x.Group)
            .WithMany(x => x.Schedules)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Student>()
            .HasOne(x => x.AcademicYear)
            .WithMany()
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Student>()
            .HasOne(x => x.Group)
            .WithMany(x => x.Students)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Lesson>()
            .HasOne(x => x.Group)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MonthlyPayment>()
            .HasOne(x => x.Student)
            .WithMany(x => x.MonthlyPayments)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MonthlyPayment>()
            .HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MonthlyPayment>()
            .HasOne(x => x.AcademicYear)
            .WithMany()
            .HasForeignKey(x => x.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = AppConstants.AdminRole, ArabicName = "أدمن", IsActive = true },
            new Role { Id = 2, Name = AppConstants.TeacherRole, ArabicName = "مدرس", IsActive = true },
            new Role { Id = 3, Name = AppConstants.AssistantTeacherRole, ArabicName = "مساعد مدرس", IsActive = true });

        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, Name = "Manage Roles", ArabicName = "إدارة الأدوار", Code = PermissionCodes.RolesManage, ModuleName = "Roles" },
            new Permission { Id = 2, Name = "Manage Employees", ArabicName = "إدارة الموظفين", Code = PermissionCodes.EmployeesManage, ModuleName = "Employees" },
            new Permission { Id = 3, Name = "Manage Academic Years", ArabicName = "إدارة السنوات الدراسية", Code = PermissionCodes.AcademicYearsManage, ModuleName = "AcademicYears" },
            new Permission { Id = 4, Name = "Manage Groups", ArabicName = "إدارة المجموعات", Code = PermissionCodes.GroupsManage, ModuleName = "Groups" },
            new Permission { Id = 5, Name = "Manage Students", ArabicName = "إدارة الطلاب", Code = PermissionCodes.StudentsManage, ModuleName = "Students" },
            new Permission { Id = 6, Name = "Manage Lessons", ArabicName = "إدارة الدروس", Code = PermissionCodes.LessonsManage, ModuleName = "Lessons" },
            new Permission { Id = 7, Name = "Manage Payments", ArabicName = "إدارة المدفوعات الشهرية", Code = PermissionCodes.PaymentsManage, ModuleName = "Payments" },
            new Permission { Id = 8, Name = "View Reports", ArabicName = "عرض التقارير", Code = PermissionCodes.ReportsView, ModuleName = "Reports" },
            new Permission { Id = 9, Name = "View Dashboard", ArabicName = "عرض لوحة التحكم", Code = PermissionCodes.DashboardView, ModuleName = "Dashboard" });

        var rolePermissions = Enumerable.Range(1, 9)
            .Select(permissionId => new RolePermission { RoleId = 1, PermissionId = permissionId })
            .Concat(Enumerable.Range(3, 7).Select(permissionId => new RolePermission { RoleId = 2, PermissionId = permissionId }))
            .ToArray();
        modelBuilder.Entity<RolePermission>().HasData(rolePermissions);

        modelBuilder.Entity<AcademicYear>().HasData(
            new AcademicYear { Id = 1, Name = "الصف الأول الثانوي", StartDate = new DateOnly(2025, 9, 1), EndDate = new DateOnly(2026, 6, 30), IsActive = true },
            new AcademicYear { Id = 2, Name = "الصف الثاني الثانوي", StartDate = new DateOnly(2025, 9, 1), EndDate = new DateOnly(2026, 6, 30), IsActive = true });

        modelBuilder.Entity<Group>().HasData(
            new Group { Id = 1, Name = "مجموعة السبت مساء", AcademicYearId = 1, GroupType = GroupType.Public, DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(20, 0), DefaultLessonPrice = 150, IsActive = true },
            new Group { Id = 2, Name = "درس خاص الأحد", AcademicYearId = 2, GroupType = GroupType.Private, DayOfWeek = DayOfWeek.Sunday, StartTime = new TimeOnly(19, 0), EndTime = new TimeOnly(20, 30), DefaultLessonPrice = 300, IsActive = true });

    }
}
