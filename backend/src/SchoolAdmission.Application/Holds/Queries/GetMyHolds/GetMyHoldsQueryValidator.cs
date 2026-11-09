using FluentValidation;
using SchoolAdmission.Application.Common.Validation;

namespace SchoolAdmission.Application.Holds.Queries.GetMyHolds;

public sealed class GetMyHoldsQueryValidator : AbstractValidator<GetMyHoldsQuery>
{
    public GetMyHoldsQueryValidator()
    {
        RuleFor(query => query.StudentId).ValidStudentId();
    }
}
