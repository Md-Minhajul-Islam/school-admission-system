using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.UnitTests.Domain;

public sealed class HoldTests
{
    private const string StudentId = "20152691234567890";
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 4, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_returns_held_application_with_expiry()
    {
        var hold = Hold.Create("h_1", "c_1", StudentId, Now, TimeSpan.FromMinutes(10));

        Assert.Equal(HoldStatus.Held, hold.Status);
        Assert.Equal(StudentId, hold.StudentId);
        Assert.Equal(Now.AddMinutes(10), hold.ExpiresAt);
        Assert.Equal(HoldStatus.Held, hold.EffectiveStatus(Now.AddMinutes(9)));
    }

    [Fact]
    public void EffectiveStatus_reports_expired_after_the_deadline_without_being_persisted()
    {
        var hold = Hold.Create("h_1", "c_1", StudentId, Now, TimeSpan.FromMinutes(10));

        Assert.True(hold.IsExpired(Now.AddMinutes(10)));
        Assert.Equal(HoldStatus.Expired, hold.EffectiveStatus(Now.AddMinutes(10)));
        Assert.Equal(HoldStatus.Held, hold.Status);
    }
}
