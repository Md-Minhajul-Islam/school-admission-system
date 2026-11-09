using AutoMapper;
using MediatR;
using SchoolAdmission.Application.Common;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Application.Classes.Commands.CreateClass;

public sealed class CreateClassCommandHandler(
    IClassRoomRepository classes,
    IRepository repository,
    IMapper mapper,
    TimeProvider timeProvider)
    : IRequestHandler<CreateClassCommand, ClassCreatedDto>
{
    public async Task<ClassCreatedDto> Handle(
        CreateClassCommand request,
        CancellationToken cancellationToken)
    {
        var classRoom = new ClassRoom(
            Ids.NewClassId(),
            request.Name,
            request.TotalSeats,
            timeProvider.GetUtcNow());

        await classes.AddAsync(classRoom, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return mapper.Map<ClassCreatedDto>(classRoom);
    }
}
