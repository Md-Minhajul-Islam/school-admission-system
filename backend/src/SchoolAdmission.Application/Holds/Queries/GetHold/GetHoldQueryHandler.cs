using AutoMapper;
using MediatR;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Exceptions;

namespace SchoolAdmission.Application.Holds.Queries.GetHold;

public sealed class GetHoldQueryHandler(
    IHoldRepository holds,
    IMapper mapper)
    : IRequestHandler<GetHoldQuery, HoldDto>
{
    public async Task<HoldDto> Handle(
        GetHoldQuery request,
        CancellationToken cancellationToken)
    {
        var snapshot = await holds.GetSnapshotAsync(request.HoldId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        return mapper.Map<HoldDto>(snapshot);
    }
}
