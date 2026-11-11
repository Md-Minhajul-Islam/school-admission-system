using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Exceptions;
using SchoolAdmission.Application.Common.Options;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Application.Holds.Commands.CreateHold;

// Start → lock the class → expire old holds → duplicate? reject → full? reject → create hold → save → done.
public sealed class CreateHoldCommandHandler(
    IClassRoomRepository classes,
    IHoldRepository holds,
    IRepository repository,
    IMapper mapper,
    IOptions<HoldOptions> holdOptions)
    : IRequestHandler<CreateHoldCommand, HoldCreatedDto>
{
    public Task<HoldCreatedDto> Handle(
        CreateHoldCommand request,
        CancellationToken cancellationToken)
    {
        return repository.ExecuteInTransactionAsync(async ct =>
        {
            var classRoom = await classes.GetByIdForUpdateAsync(request.ClassId, ct)
                ?? throw new NotFoundException("Class not found.");

            await holds.ExpireOverdueForStudentAsync(request.ClassId, request.StudentId, ct);

            if (await holds.HasActiveOrConfirmedAsync(request.ClassId, request.StudentId, ct))
            {
                throw new ConflictException(
                    "You already have an active application or admission in this class.");
            }

            var occupiedSeats = await holds.CountOccupiedSeatsAsync(request.ClassId, ct);
            if (occupiedSeats >= classRoom.TotalSeats)
            {
                throw new ConflictException("No seats are available in this class.");
            }

            var hold = Hold.Create(
                Ids.NewHoldId(),
                classRoom.Id,
                request.StudentId,
                await holds.GetDatabaseTimeAsync(ct),
                holdOptions.Value.Duration);

            await holds.AddAsync(hold, ct);

            return mapper.Map<HoldCreatedDto>(hold);
        }, cancellationToken);
    }
}
