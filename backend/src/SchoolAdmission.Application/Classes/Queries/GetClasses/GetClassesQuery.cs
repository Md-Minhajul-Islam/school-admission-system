using MediatR;

namespace SchoolAdmission.Application.Classes.Queries.GetClasses;

public sealed record GetClassesQuery : IRequest<IReadOnlyList<ClassSeatDto>>;

public sealed record ClassSeatDto(
    string Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats);
