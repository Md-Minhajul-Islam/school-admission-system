using Microsoft.EntityFrameworkCore;
using Npgsql;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Exceptions;

namespace SchoolAdmission.Infrastructure.Persistence;

public sealed class Repository(AdmissionDbContext db) : IRepository
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => SaveChangesCoreAsync(ct);

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var result = await operation(ct);
            await SaveChangesCoreAsync(ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task<int> SaveChangesCoreAsync(CancellationToken ct)
    {
        try
        {
            return await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException
            {
                SqlState: "23505"
            } postgresException)
        {
            var message = postgresException.ConstraintName == "ux_holds_active_student_class"
                ? "You already have an active application or admission in this class."
                : "The request conflicts with an existing record.";

            throw new ConflictException(message);
        }
    }
}
