using NSubstitute;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Exceptions;
using SchoolAdmission.Application.Holds.Commands.CancelHold;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.UnitTests.Application.Holds.Commands.CancelHold;

public sealed class CancelHoldCommandHandlerTests
{
    private const string StudentId = "20152691234567890";

    private readonly IHoldRepository _holds = Substitute.For<IHoldRepository>();

    [Fact]
    public async Task Cancel_is_idempotent_for_an_already_cancelled_application()
    {
        _holds.CancelIfActiveAsync("h_1", StudentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _holds.GetSnapshotAsync("h_1", Arg.Any<CancellationToken>())
            .Returns(Snapshot(HoldStatus.Cancelled));

        var handler = new CancelHoldCommandHandler(_holds);

        await handler.Handle(new CancelHoldCommand("h_1", StudentId), CancellationToken.None);
    }

    [Fact]
    public async Task Cancel_rejects_a_confirmed_admission()
    {
        _holds.CancelIfActiveAsync("h_1", StudentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _holds.GetSnapshotAsync("h_1", Arg.Any<CancellationToken>())
            .Returns(Snapshot(HoldStatus.Confirmed));

        var handler = new CancelHoldCommandHandler(_holds);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new CancelHoldCommand("h_1", StudentId),
            CancellationToken.None));
    }

    private static HoldSnapshot Snapshot(HoldStatus status, string studentId = StudentId)
        => new(
            "h_1",
            "c_1",
            studentId,
            status,
            DateTimeOffset.UtcNow.AddMinutes(5),
            "Class 6");
}
