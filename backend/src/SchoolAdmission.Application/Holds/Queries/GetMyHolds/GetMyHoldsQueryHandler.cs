using AutoMapper;
using MediatR;
using SchoolAdmission.Application.Common.Abstractions.Persistence;

namespace SchoolAdmission.Application.Holds.Queries.GetMyHolds;

public sealed class GetMyHoldsQueryHandler(
    IHoldRepository holds,
    IMapper mapper)
    : IRequestHandler<GetMyHoldsQuery, IReadOnlyList<MyHoldDto>>
{
    public async Task<IReadOnlyList<MyHoldDto>> Handle(
        GetMyHoldsQuery request,
        CancellationToken cancellationToken)
    {
        var snapshots = await holds.GetSnapshotsByStudentAsync(request.StudentId, cancellationToken);
        return mapper.Map<IReadOnlyList<MyHoldDto>>(snapshots);
    }
}
