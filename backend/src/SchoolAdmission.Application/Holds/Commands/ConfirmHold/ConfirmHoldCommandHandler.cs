using MediatR;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Exceptions;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.Holds.Commands.ConfirmHold;

public sealed class ConfirmHoldCommandHandler(IHoldRepository holds)
    : IRequestHandler<ConfirmHoldCommand, ConfirmHoldDto>
{
    public async Task<ConfirmHoldDto> Handle(
        ConfirmHoldCommand request,
        CancellationToken cancellationToken)
    {
        var confirmed = await holds.ConfirmIfActiveAsync(
            request.HoldId,
            request.StudentId,
            cancellationToken);

        if (confirmed)
        {
            return new ConfirmHoldDto(request.HoldId, "CONFIRMED");
        }

        var snapshot = await holds.GetSnapshotAsync(request.HoldId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        if (!string.Equals(snapshot.StudentId, request.StudentId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("This application belongs to another student.");
        }

        if (snapshot.Status == HoldStatus.Confirmed)
        {
            return new ConfirmHoldDto(request.HoldId, "CONFIRMED");
        }

        throw new ConflictException(snapshot.Status == HoldStatus.Expired
            ? "This application has expired. Please apply again."
            : "This application can no longer be confirmed.");
    }
}
