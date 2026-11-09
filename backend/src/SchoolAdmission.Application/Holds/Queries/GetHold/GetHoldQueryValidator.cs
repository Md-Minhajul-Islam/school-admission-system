using FluentValidation;

namespace SchoolAdmission.Application.Holds.Queries.GetHold;

public sealed class GetHoldQueryValidator : AbstractValidator<GetHoldQuery>
{
    public GetHoldQueryValidator()
    {
        RuleFor(query => query.HoldId).NotEmpty();
    }
}
