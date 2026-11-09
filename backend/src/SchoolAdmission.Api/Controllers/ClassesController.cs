using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Application.Classes.Commands.CreateClass;
using SchoolAdmission.Application.Classes.Queries.GetClass;
using SchoolAdmission.Application.Classes.Queries.GetClasses;
using SchoolAdmission.Application.Holds.Commands.CreateHold;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public sealed class ClassesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ClassCreatedDto>> Create(
        [FromBody] CreateClassCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/classes/{result.Id}", result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClassSeatDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetClassesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{classId}")]
    public async Task<ActionResult<ClassSeatDto>> Get(
        string classId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetClassQuery(classId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{classId}/holds")]
    public async Task<ActionResult<HoldCreatedDto>> CreateHold(
        string classId,
        CancellationToken cancellationToken)
    {
        var command = new CreateHoldCommand(classId, StudentIdHeader.Read(Request));
        var result = await sender.Send(command, cancellationToken);
        return Created($"/holds/{result.HoldId}", result);
    }
}
