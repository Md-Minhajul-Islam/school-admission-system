using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Infrastructure.Persistence;

internal static class DatabaseTime
{
    /// <summary>
    /// Reads the current time from PostgreSQL so that every process sharing the database
    /// compares timestamps against the same clock. Uses statement time rather than
    /// transaction time so callers that waited on a row lock do not read a stale timestamp.
    /// </summary>
    public static Task<DateTimeOffset> NowAsync(AdmissionDbContext db, CancellationToken ct)
        => db.Database
            .SqlQueryRaw<DateTimeOffset>("SELECT statement_timestamp() AS \"Value\"")
            .SingleAsync(ct);
}
