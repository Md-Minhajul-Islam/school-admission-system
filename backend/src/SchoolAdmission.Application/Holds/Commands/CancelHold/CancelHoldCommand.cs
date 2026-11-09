using MediatR;

namespace SchoolAdmission.Application.Holds.Commands.CancelHold;

public sealed record CancelHoldCommand(string HoldId, string StudentId) : IRequest<Unit>;
