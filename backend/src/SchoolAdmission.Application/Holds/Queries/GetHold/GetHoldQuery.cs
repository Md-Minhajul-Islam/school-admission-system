using MediatR;

namespace SchoolAdmission.Application.Holds.Queries.GetHold;

public sealed record GetHoldQuery(string HoldId) : IRequest<HoldDto>;

public sealed record HoldDto(
    string HoldId,
    string ClassId,
    string Status,
    DateTime ExpiresAt);
