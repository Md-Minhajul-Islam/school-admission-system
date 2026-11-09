using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SchoolAdmission.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    private const long AdvisoryLockKey = 748392011;

    public static async Task MigrateAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken ct = default)
    {
        const int maxAttempts = 30;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();

                await db.Database.OpenConnectionAsync(ct);
                try
                {
                    await db.Database.ExecuteSqlRawAsync(
                        $"SELECT pg_advisory_lock({AdvisoryLockKey})",
                        ct);

                    await db.Database.MigrateAsync(ct);
                }
                finally
                {
                    await db.Database.ExecuteSqlRawAsync(
                        $"SELECT pg_advisory_unlock({AdvisoryLockKey})",
                        CancellationToken.None);
                    await db.Database.CloseConnectionAsync();
                }

                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Database migration attempt {Attempt}/{MaxAttempts} failed; retrying in {DelaySeconds} seconds.",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, ct);
            }
        }
    }
}
