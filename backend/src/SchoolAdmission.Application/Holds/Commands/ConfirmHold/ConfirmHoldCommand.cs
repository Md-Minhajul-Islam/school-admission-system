using MediatR;

namespace SchoolAdmission.Application.Holds.Commands.ConfirmHold;

public sealed record ConfirmHoldCommand(string HoldId, string StudentId) : IRequest<ConfirmHoldDto>;

public sealed record ConfirmHoldDto(string HoldId, string Status);
