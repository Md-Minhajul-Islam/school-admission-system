using MediatR;

namespace SchoolAdmission.Application.Classes.Commands.CreateClass;

public sealed record CreateClassCommand(string Name, int TotalSeats) : IRequest<ClassCreatedDto>;

public sealed record ClassCreatedDto(string Id, string Name, int TotalSeats);
