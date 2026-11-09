using FluentValidation;

namespace SchoolAdmission.Application.Classes.Commands.CreateClass;

public sealed class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Class name is required and must be 200 characters or fewer.");

        RuleFor(command => command.TotalSeats)
            .GreaterThan(0)
            .LessThanOrEqualTo(10_000)
            .WithMessage("Total seats must be between 1 and 10000.");
    }
}
