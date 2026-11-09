using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Infrastructure.Persistence.Repositories;

public sealed class ClassRoomRepository(AdmissionDbContext db) : IClassRoomRepository
{
    public async Task AddAsync(ClassRoom classRoom, CancellationToken ct = default)
        => await db.Classes.AddAsync(classRoom, ct);

    public async Task<ClassRoom?> GetByIdForUpdateAsync(string classId, CancellationToken ct = default)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "GetByIdForUpdateAsync must be called inside a transaction.");
        }

        var rows = await db.Classes
            .FromSqlInterpolated($"SELECT * FROM classes WHERE id = {classId} FOR UPDATE")
            .ToListAsync(ct);

        return rows.SingleOrDefault();
    }

    public async Task<IReadOnlyList<SeatSummary>> GetSeatSummariesAsync(CancellationToken ct = default)
    {
        var now = await DatabaseTime.NowAsync(db, ct);

        return await db.Classes
            .AsNoTracking()
            .OrderBy(classRoom => classRoom.CreatedAt)
            .Select(classRoom => new SeatSummary(
                classRoom.Id,
                classRoom.Name,
                classRoom.TotalSeats,
                classRoom.Holds.Count(hold =>
                    hold.Status == HoldStatus.Held && hold.ExpiresAt > now),
                classRoom.Holds.Count(hold => hold.Status == HoldStatus.Confirmed)))
            .ToListAsync(ct);
    }

    public async Task<SeatSummary?> GetSeatSummaryAsync(string classId, CancellationToken ct = default)
    {
        var now = await DatabaseTime.NowAsync(db, ct);

        return await db.Classes
            .AsNoTracking()
            .Where(classRoom => classRoom.Id == classId)
            .Select(classRoom => new SeatSummary(
                classRoom.Id,
                classRoom.Name,
                classRoom.TotalSeats,
                classRoom.Holds.Count(hold =>
                    hold.Status == HoldStatus.Held && hold.ExpiresAt > now),
                classRoom.Holds.Count(hold => hold.Status == HoldStatus.Confirmed)))
            .SingleOrDefaultAsync(ct);
    }
}
