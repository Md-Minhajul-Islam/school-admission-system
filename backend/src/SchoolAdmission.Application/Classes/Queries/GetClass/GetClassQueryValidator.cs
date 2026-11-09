using FluentValidation;

namespace SchoolAdmission.Application.Classes.Queries.GetClass;

public sealed class GetClassQueryValidator : AbstractValidator<GetClassQuery>
{
    public GetClassQueryValidator()
    {
        RuleFor(query => query.ClassId).NotEmpty();
    }
}
