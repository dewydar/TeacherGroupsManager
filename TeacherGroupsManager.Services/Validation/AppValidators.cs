using FluentValidation;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Shared.Localization;

namespace TeacherGroupsManager.Services.Validation;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage(localizer["RequiredUsername"]);
        RuleFor(x => x.Password).NotEmpty().WithMessage(localizer["RequiredPassword"]);
    }
}

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage(localizer["RequiredEmployeeName"]);
        RuleFor(x => x.Mobile).NotEmpty().WithMessage(localizer["RequiredMobile"]);
        RuleFor(x => x.Username).NotEmpty().WithMessage(localizer["RequiredUsername"]);
        RuleFor(x => x.Password).MinimumLength(8).WithMessage(localizer["PasswordMinLength"]);
        RuleFor(x => x.RoleId).GreaterThan(0).WithMessage(localizer["RequiredRole"]);
    }
}

public class CreateAcademicYearDtoValidator : AbstractValidator<CreateAcademicYearDto>
{
    public CreateAcademicYearDtoValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizer["RequiredAcademicYearName"]);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage(localizer["EndDateAfterStartDate"]);
    }
}

public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
{
    public CreateGroupDtoValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizer["RequiredGroupName"]);
        RuleFor(x => x.AcademicYearId).GreaterThan(0).WithMessage(localizer["RequiredAcademicYear"]);
        RuleFor(x => x.DefaultLessonPrice).GreaterThan(0).WithMessage(localizer["PriceGreaterThanZero"]);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage(localizer["EndTimeAfterStartTime"]);
        RuleForEach(x => x.Schedules).ChildRules(schedule =>
        {
            schedule.RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage(localizer["EndTimeAfterStartTime"]);
        });
    }
}

public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentDtoValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage(localizer["RequiredStudentName"]);
        RuleFor(x => x.AcademicYearId).GreaterThan(0).WithMessage(localizer["RequiredAcademicYear"]);
        RuleFor(x => x.GroupId).GreaterThan(0).WithMessage(localizer["RequiredGroup"]);
    }
}

public class CreateLessonDtoValidator : AbstractValidator<CreateLessonDto>
{
    public CreateLessonDtoValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage(localizer["RequiredLessonTitle"]);
        RuleFor(x => x.GroupId).GreaterThan(0).WithMessage(localizer["RequiredGroup"]);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage(localizer["PriceGreaterThanZero"]);
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage(localizer["MonthBetween1And12"]);
        RuleFor(x => x.Year).GreaterThan(2000).WithMessage(localizer["InvalidYear"]);
    }
}

public class CreateMonthlyPaymentDtoValidator : AbstractValidator<CreateMonthlyPaymentDto>
{
    public CreateMonthlyPaymentDtoValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.StudentId).GreaterThan(0).WithMessage(localizer["RequiredStudent"]);
        RuleFor(x => x.GroupId).GreaterThan(0).WithMessage(localizer["RequiredGroup"]);
        RuleFor(x => x.AcademicYearId).GreaterThan(0).WithMessage(localizer["RequiredAcademicYear"]);
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage(localizer["MonthBetween1And12"]);
        RuleFor(x => x.RequiredAmount).GreaterThan(0).WithMessage(localizer["RequiredAmountGreaterThanZero"]);
        RuleFor(x => x.PaidAmount).LessThanOrEqualTo(x => x.RequiredAmount).WithMessage(localizer["PaidAmountCannotExceedRequired"]);
    }
}
