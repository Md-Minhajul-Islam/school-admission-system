using SchoolAdmission.Domain.Enums;
using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Domain.Entities;

public sealed class Hold
{
    private Hold()
    {
    }

    public string Id { get; private set; } = null!;

    public string ClassId { get; private set; } = null!;

    public string StudentId { get; private set; } = null!;

    public HoldStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? ConfirmedAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public ClassRoom ClassRoom { get; private set; } = null!;

    public static Hold Create(
        string id,
        string classId,
        string studentId,
        DateTimeOffset now,
        TimeSpan holdDuration)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new DomainException("Hold id is required.");
        }

        if (string.IsNullOrWhiteSpace(classId))
        {
            throw new DomainException("Class id is required.");
        }

        if (studentId.Length != 17 || !studentId.All(char.IsAsciiDigit))
        {
            throw new DomainException("Student id must be exactly 17 digits.");
        }

        if (holdDuration <= TimeSpan.Zero)
        {
            throw new DomainException("Hold duration must be greater than zero.");
        }

        return new Hold
        {
            Id = id,
            ClassId = classId,
            StudentId = studentId,
            Status = HoldStatus.Held,
            CreatedAt = now,
            ExpiresAt = now.Add(holdDuration)
        };
    }

    public bool IsExpired(DateTimeOffset now) => Status == HoldStatus.Held && ExpiresAt <= now;

    public HoldStatus EffectiveStatus(DateTimeOffset now) => IsExpired(now) ? HoldStatus.Expired : Status;
}
