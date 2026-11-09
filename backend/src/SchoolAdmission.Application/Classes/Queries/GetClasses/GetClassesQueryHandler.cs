using AutoMapper;
using MediatR;
using SchoolAdmission.Application.Common.Abstractions.Persistence;

namespace SchoolAdmission.Application.Classes.Queries.GetClasses;

public sealed class GetClassesQueryHandler(
    IClassRoomRepository classes,
    IMapper mapper)
    : IRequestHandler<GetClassesQuery, IReadOnlyList<ClassSeatDto>>
{
    public async Task<IReadOnlyList<ClassSeatDto>> Handle(
        GetClassesQuery request,
        CancellationToken cancellationToken)
    {
        var summaries = await classes.GetSeatSummariesAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<ClassSeatDto>>(summaries);
    }
}
