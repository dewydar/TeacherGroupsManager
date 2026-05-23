using FluentValidation;
using TeacherGroupsManager.Dtos;

namespace TeacherGroupsManager.Services.Validation;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("اسم المستخدم مطلوب");
        RuleFor(x => x.Password).NotEmpty().WithMessage("كلمة المرور مطلوبة");
    }
}

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("اسم الموظف مطلوب");
        RuleFor(x => x.Mobile).NotEmpty().WithMessage("رقم الجوال مطلوب");
        RuleFor(x => x.Username).NotEmpty().WithMessage("اسم المستخدم مطلوب");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("كلمة المرور يجب ألا تقل عن 8 أحرف");
        RuleFor(x => x.RoleId).GreaterThan(0).WithMessage("يجب اختيار الدور");
    }
}

public class CreateAcademicYearDtoValidator : AbstractValidator<CreateAcademicYearDto>
{
    public CreateAcademicYearDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم السنة الدراسية مطلوب");
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("تاريخ النهاية يجب أن يكون بعد تاريخ البداية");
    }
}

public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
{
    public CreateGroupDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("اسم المجموعة مطلوب");
        RuleFor(x => x.AcademicYearId).GreaterThan(0).WithMessage("يجب اختيار السنة الدراسية");
        RuleFor(x => x.DefaultLessonPrice).GreaterThan(0).WithMessage("سعر الدرس يجب أن يكون أكبر من صفر");
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage("وقت النهاية يجب أن يكون بعد وقت البداية");
    }
}

public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("اسم الطالب مطلوب");
        RuleFor(x => x.AcademicYearId).GreaterThan(0).WithMessage("يجب اختيار السنة الدراسية");
        RuleFor(x => x.GroupId).GreaterThan(0).WithMessage("يجب اختيار المجموعة");
    }
}

public class CreateLessonDtoValidator : AbstractValidator<CreateLessonDto>
{
    public CreateLessonDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("عنوان الدرس مطلوب");
        RuleFor(x => x.GroupId).GreaterThan(0).WithMessage("يجب اختيار المجموعة");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("سعر الدرس يجب أن يكون أكبر من صفر");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("الشهر يجب أن يكون بين 1 و 12");
        RuleFor(x => x.Year).GreaterThan(2000).WithMessage("السنة غير صحيحة");
    }
}

public class CreateMonthlyPaymentDtoValidator : AbstractValidator<CreateMonthlyPaymentDto>
{
    public CreateMonthlyPaymentDtoValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0).WithMessage("يجب اختيار الطالب");
        RuleFor(x => x.GroupId).GreaterThan(0).WithMessage("يجب اختيار المجموعة");
        RuleFor(x => x.AcademicYearId).GreaterThan(0).WithMessage("يجب اختيار السنة الدراسية");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("الشهر يجب أن يكون بين 1 و 12");
        RuleFor(x => x.RequiredAmount).GreaterThan(0).WithMessage("المبلغ المطلوب يجب أن يكون أكبر من صفر");
        RuleFor(x => x.PaidAmount).LessThanOrEqualTo(x => x.RequiredAmount).WithMessage("المبلغ المدفوع لا يمكن أن يكون أكبر من المطلوب");
    }
}
