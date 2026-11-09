using AutoMapper;
using MediatR;
using SchoolAdmission.Application.Classes.Queries.GetClasses;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Exceptions;

namespace SchoolAdmission.Application.Classes.Queries.GetClass;

public sealed class GetClassQueryHandler(
    IClassRoomRepository classes,
    IMapper mapper)
    : IRequestHandler<GetClassQuery, ClassSeatDto>
{
    public async Task<ClassSeatDto> Handle(
        GetClassQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await classes.GetSeatSummaryAsync(request.ClassId, cancellationToken)
            ?? throw new NotFoundException("Class not found.");

        return mapper.Map<ClassSeatDto>(summary);
    }
}
