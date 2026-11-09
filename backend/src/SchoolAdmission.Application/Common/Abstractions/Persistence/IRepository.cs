namespace SchoolAdmission.Application.Common.Abstractions.Persistence;

public interface IRepository
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs <paramref name="operation"/> inside a database transaction, saves pending changes,
    /// then commits. Rolls back if the operation throws.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct = default);
}
