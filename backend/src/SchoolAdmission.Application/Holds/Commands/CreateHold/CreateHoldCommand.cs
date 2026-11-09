using MediatR;

namespace SchoolAdmission.Application.Holds.Commands.CreateHold;

public sealed record CreateHoldCommand(string ClassId, string StudentId) : IRequest<HoldCreatedDto>;

public sealed record HoldCreatedDto(
    string HoldId,
    string ClassId,
    string StudentId,
    string Status,
    DateTime ExpiresAt);
