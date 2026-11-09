using MediatR;

namespace SchoolAdmission.Application.Holds.Commands.ExpireHolds;

public sealed record ExpireHoldsCommand : IRequest<int>;
