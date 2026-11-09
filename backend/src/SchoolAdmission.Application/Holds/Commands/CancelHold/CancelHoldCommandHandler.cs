using MediatR;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Exceptions;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.Holds.Commands.CancelHold;

public sealed class CancelHoldCommandHandler(IHoldRepository holds)
    : IRequestHandler<CancelHoldCommand, Unit>
{
    public async Task<Unit> Handle(
        CancelHoldCommand request,
        CancellationToken cancellationToken)
    {
        var cancelled = await holds.CancelIfActiveAsync(
            request.HoldId,
            request.StudentId,
            cancellationToken);

        if (cancelled)
        {
            return Unit.Value;
        }

        var snapshot = await holds.GetSnapshotAsync(request.HoldId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        if (!string.Equals(snapshot.StudentId, request.StudentId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("This application belongs to another student.");
        }

        if (snapshot.Status == HoldStatus.Cancelled)
        {
            return Unit.Value;
        }

        throw new ConflictException(snapshot.Status == HoldStatus.Expired
            ? "This application has expired and can no longer be cancelled."
            : "A confirmed admission cannot be cancelled.");
    }
}
