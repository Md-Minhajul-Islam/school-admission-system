namespace SchoolAdmission.Application.Common.Options;

public sealed class HoldOptions
{
    public int HoldDurationSeconds { get; set; } = 600;

    public TimeSpan Duration => TimeSpan.FromSeconds(HoldDurationSeconds);
}
