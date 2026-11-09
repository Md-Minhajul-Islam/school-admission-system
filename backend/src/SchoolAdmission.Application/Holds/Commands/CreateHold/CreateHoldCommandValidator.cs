using FluentValidation;
using SchoolAdmission.Application.Common.Validation;

namespace SchoolAdmission.Application.Holds.Commands.CreateHold;

public sealed class CreateHoldCommandValidator : AbstractValidator<CreateHoldCommand>
{
    public CreateHoldCommandValidator()
    {
        RuleFor(command => command.ClassId).NotEmpty();

        RuleFor(command => command.StudentId).ValidStudentId();
    }
}
