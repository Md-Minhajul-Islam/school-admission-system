using MediatR;

namespace SchoolAdmission.Application.Holds.Queries.GetMyHolds;

public sealed record GetMyHoldsQuery(string StudentId) : IRequest<IReadOnlyList<MyHoldDto>>;

public sealed record MyHoldDto(
    string HoldId,
    string ClassId,
    string ClassName,
    string Status,
    DateTime ExpiresAt);
