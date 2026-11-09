using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Infrastructure.Persistence.Repositories;

public sealed class HoldRepository(AdmissionDbContext db) : IHoldRepository
{
    public async Task AddAsync(Hold hold, CancellationToken ct = default)
        => await db.Holds.AddAsync(hold, ct);

    public Task<DateTimeOffset> GetDatabaseTimeAsync(CancellationToken ct = default)
        => DatabaseTime.NowAsync(db, ct);

    public async Task<int> CountOccupiedSeatsAsync(string classId, CancellationToken ct = default)
    {
        var now = await DatabaseTime.NowAsync(db, ct);

        return await db.Holds.CountAsync(
            hold => hold.ClassId == classId &&
                (hold.Status == HoldStatus.Confirmed ||
                 (hold.Status == HoldStatus.Held && hold.ExpiresAt > now)),
            ct);
    }

    public async Task<bool> HasActiveOrConfirmedAsync(
        string classId,
        string studentId,
        CancellationToken ct = default)
    {
        var now = await DatabaseTime.NowAsync(db, ct);

        return await db.Holds.AnyAsync(
            hold => hold.ClassId == classId &&
                hold.StudentId == studentId &&
                (hold.Status == HoldStatus.Confirmed ||
                 (hold.Status == HoldStatus.Held && hold.ExpiresAt > now)),
            ct);
    }

    public Task<int> ExpireOverdueAsync(CancellationToken ct = default)
        => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE holds SET status = 'EXPIRED', closed_at = statement_timestamp() WHERE status = 'HELD' AND expires_at <= statement_timestamp()",
            ct);

    public Task<int> ExpireOverdueForStudentAsync(
        string classId,
        string studentId,
        CancellationToken ct = default)
        => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE holds SET status = 'EXPIRED', closed_at = statement_timestamp() WHERE class_id = {classId} AND student_id = {studentId} AND status = 'HELD' AND expires_at <= statement_timestamp()",
            ct);

    public async Task<bool> ConfirmIfActiveAsync(
        string holdId,
        string studentId,
        CancellationToken ct = default)
    {
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE holds SET status = 'CONFIRMED', confirmed_at = statement_timestamp() WHERE id = {holdId} AND student_id = {studentId} AND status = 'HELD' AND expires_at > statement_timestamp()",
            ct);

        return affected > 0;
    }

    public async Task<bool> CancelIfActiveAsync(
        string holdId,
        string studentId,
        CancellationToken ct = default)
    {
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE holds SET status = 'CANCELLED', closed_at = statement_timestamp() WHERE id = {holdId} AND student_id = {studentId} AND status = 'HELD' AND expires_at > statement_timestamp()",
            ct);

        return affected > 0;
    }

    public async Task<HoldSnapshot?> GetSnapshotAsync(
        string holdId,
        CancellationToken ct = default)
    {
        var now = await DatabaseTime.NowAsync(db, ct);

        return await db.Holds
            .AsNoTracking()
            .Where(hold => hold.Id == holdId)
            .Select(hold => new HoldSnapshot(
                hold.Id,
                hold.ClassId,
                hold.StudentId,
                hold.Status == HoldStatus.Held && hold.ExpiresAt <= now
                    ? HoldStatus.Expired
                    : hold.Status,
                hold.ExpiresAt,
                hold.ClassRoom.Name))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<HoldSnapshot>> GetSnapshotsByStudentAsync(
        string studentId,
        CancellationToken ct = default)
    {
        var now = await DatabaseTime.NowAsync(db, ct);

        return await db.Holds
            .AsNoTracking()
            .Where(hold => hold.StudentId == studentId)
            .OrderByDescending(hold => hold.CreatedAt)
            .Select(hold => new HoldSnapshot(
                hold.Id,
                hold.ClassId,
                hold.StudentId,
                hold.Status == HoldStatus.Held && hold.ExpiresAt <= now
                    ? HoldStatus.Expired
                    : hold.Status,
                hold.ExpiresAt,
                hold.ClassRoom.Name))
            .ToListAsync(ct);
    }
}
