using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Infrastructure.Persistence;

internal static class DatabaseTime
{
    /// <summary>
    /// Reads the current time from PostgreSQL so that every process sharing the database
    /// compares timestamps against the same clock.
    /// </summary>
    public static Task<DateTimeOffset> NowAsync(AdmissionDbContext db, CancellationToken ct)
        => db.Database
            .SqlQueryRaw<DateTimeOffset>("SELECT now() AS \"Value\"")
            .SingleAsync(ct);
}
