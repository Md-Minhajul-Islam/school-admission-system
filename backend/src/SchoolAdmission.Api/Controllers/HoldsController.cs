using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Application.Holds.Commands.CancelHold;
using SchoolAdmission.Application.Holds.Commands.ConfirmHold;
using SchoolAdmission.Application.Holds.Queries.GetHold;
using SchoolAdmission.Application.Holds.Queries.GetMyHolds;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("holds")]
public sealed class HoldsController(ISender sender) : ControllerBase
{
    [HttpGet("{holdId}")]
    public async Task<ActionResult<HoldDto>> Get(
        string holdId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetHoldQuery(holdId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{holdId}/confirm")]
    public async Task<ActionResult<ConfirmHoldDto>> Confirm(
        string holdId,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmHoldCommand(holdId, StudentIdHeader.Read(Request));
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{holdId}")]
    public async Task<IActionResult> Cancel(
        string holdId,
        CancellationToken cancellationToken)
    {
        var command = new CancelHoldCommand(holdId, StudentIdHeader.Read(Request));
        await sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("/me/holds")]
    public async Task<ActionResult<IReadOnlyList<MyHoldDto>>> Mine(
        CancellationToken cancellationToken)
    {
        var query = new GetMyHoldsQuery(StudentIdHeader.Read(Request));
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
