using NSubstitute;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Exceptions;
using SchoolAdmission.Application.Holds.Commands.ConfirmHold;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.UnitTests.Application.Holds.Commands.ConfirmHold;

public sealed class ConfirmHoldCommandHandlerTests
{
    private const string StudentId = "20152691234567890";
    private const string OtherStudentId = "20152691234567891";

    private readonly IHoldRepository _holds = Substitute.For<IHoldRepository>();

    [Fact]
    public async Task Confirm_returns_confirmed_when_the_repository_updates_the_hold()
    {
        _holds.ConfirmIfActiveAsync("h_1", StudentId, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new ConfirmHoldCommandHandler(_holds);
        var result = await handler.Handle(
            new ConfirmHoldCommand("h_1", StudentId),
            CancellationToken.None);

        Assert.Equal("CONFIRMED", result.Status);
        Assert.Equal("h_1", result.HoldId);
    }

    [Fact]
    public async Task Confirm_is_idempotent_for_an_already_confirmed_application()
    {
        _holds.ConfirmIfActiveAsync("h_1", StudentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _holds.GetSnapshotAsync("h_1", Arg.Any<CancellationToken>())
            .Returns(Snapshot(HoldStatus.Confirmed));

        var handler = new ConfirmHoldCommandHandler(_holds);
        var result = await handler.Handle(
            new ConfirmHoldCommand("h_1", StudentId),
            CancellationToken.None);

        Assert.Equal("CONFIRMED", result.Status);
    }

    [Fact]
    public async Task Confirm_rejects_another_students_application()
    {
        _holds.ConfirmIfActiveAsync("h_1", OtherStudentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _holds.GetSnapshotAsync("h_1", Arg.Any<CancellationToken>())
            .Returns(Snapshot(HoldStatus.Held));

        var handler = new ConfirmHoldCommandHandler(_holds);

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(
            new ConfirmHoldCommand("h_1", OtherStudentId),
            CancellationToken.None));
    }

    [Fact]
    public async Task Confirm_returns_conflict_for_an_expired_application()
    {
        _holds.ConfirmIfActiveAsync("h_1", StudentId, Arg.Any<CancellationToken>())
            .Returns(false);
        _holds.GetSnapshotAsync("h_1", Arg.Any<CancellationToken>())
            .Returns(Snapshot(HoldStatus.Expired));

        var handler = new ConfirmHoldCommandHandler(_holds);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new ConfirmHoldCommand("h_1", StudentId),
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
