using FluentValidation;
using SchoolAdmission.Application.Common.Validation;

namespace SchoolAdmission.Application.Holds.Commands.CancelHold;

public sealed class CancelHoldCommandValidator : AbstractValidator<CancelHoldCommand>
{
    public CancelHoldCommandValidator()
    {
        RuleFor(command => command.HoldId).NotEmpty();
        RuleFor(command => command.StudentId).ValidStudentId();
    }
}
