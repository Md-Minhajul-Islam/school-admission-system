using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Application.Common.Abstractions.Persistence;

public interface IClassRoomRepository
{
    Task AddAsync(ClassRoom classRoom, CancellationToken ct = default);

    /// <summary>
    /// Locks the class row for the duration of the current transaction. Call only from
    /// <see cref="IRepository.ExecuteInTransactionAsync{T}"/>.
    /// </summary>
    Task<ClassRoom?> GetByIdForUpdateAsync(string classId, CancellationToken ct = default);

    Task<IReadOnlyList<SeatSummary>> GetSeatSummariesAsync(CancellationToken ct = default);

    Task<SeatSummary?> GetSeatSummaryAsync(string classId, CancellationToken ct = default);
}

public sealed record SeatSummary(
    string Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats);
