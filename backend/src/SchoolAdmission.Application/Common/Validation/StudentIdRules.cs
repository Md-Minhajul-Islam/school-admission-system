using FluentValidation;

namespace SchoolAdmission.Application.Common.Validation;

public static class StudentIdRules
{
    public static IRuleBuilderOptions<T, string> ValidStudentId<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .Matches("^[0-9]{17}$")
            .WithMessage("Student ID must be exactly 17 digits.");
    }
}
