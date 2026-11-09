using MediatR;
using SchoolAdmission.Application.Classes.Queries.GetClasses;

namespace SchoolAdmission.Application.Classes.Queries.GetClass;

public sealed record GetClassQuery(string ClassId) : IRequest<ClassSeatDto>;
