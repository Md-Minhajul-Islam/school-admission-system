using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.Common.Abstractions.Persistence;

public interface IHoldRepository
{
    Task AddAsync(Hold hold, CancellationToken ct = default);

    Task<int> CountOccupiedSeatsAsync(string classId, CancellationToken ct = default);

    Task<bool> HasActiveOrConfirmedAsync(string classId, string studentId, CancellationToken ct = default);

    Task<int> ExpireOverdueAsync(CancellationToken ct = default);

    Task<int> ExpireOverdueForStudentAsync(string classId, string studentId, CancellationToken ct = default);

    Task<bool> ConfirmIfActiveAsync(string holdId, string studentId, CancellationToken ct = default);

    Task<bool> CancelIfActiveAsync(string holdId, string studentId, CancellationToken ct = default);

    Task<HoldSnapshot?> GetSnapshotAsync(string holdId, CancellationToken ct = default);

    Task<IReadOnlyList<HoldSnapshot>> GetSnapshotsByStudentAsync(string studentId, CancellationToken ct = default);
}

public sealed record HoldSnapshot(
    string HoldId,
    string ClassId,
    string StudentId,
    HoldStatus Status,
    DateTimeOffset ExpiresAt,
    string ClassName);
