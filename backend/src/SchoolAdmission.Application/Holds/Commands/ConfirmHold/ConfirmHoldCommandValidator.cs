using FluentValidation;
using SchoolAdmission.Application.Common.Validation;

namespace SchoolAdmission.Application.Holds.Commands.ConfirmHold;

public sealed class ConfirmHoldCommandValidator : AbstractValidator<ConfirmHoldCommand>
{
    public ConfirmHoldCommandValidator()
    {
        RuleFor(command => command.HoldId).NotEmpty();
        RuleFor(command => command.StudentId).ValidStudentId();
    }
}
