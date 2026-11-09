using MediatR;
using SchoolAdmission.Application.Common.Abstractions.Persistence;

namespace SchoolAdmission.Application.Holds.Commands.ExpireHolds;

public sealed class ExpireHoldsCommandHandler(IHoldRepository holds)
    : IRequestHandler<ExpireHoldsCommand, int>
{
    public Task<int> Handle(ExpireHoldsCommand request, CancellationToken cancellationToken)
        => holds.ExpireOverdueAsync(cancellationToken);
}
