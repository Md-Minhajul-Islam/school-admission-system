using Npgsql;

namespace SchoolAdmission.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Creates and drops throwaway PostgreSQL databases. The server is supplied through
/// <c>TEST_DATABASE_URL</c>; the value must point at an existing database on that server
/// (normally the <c>postgres</c> maintenance database).
/// </summary>
internal static class TestDatabase
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";

    public static string BaseConnectionString =>
        Environment.GetEnvironmentVariable("TEST_DATABASE_URL") ?? DefaultConnectionString;

    public static string CreateUniqueName()
        => "school_admission_test_" + Guid.NewGuid().ToString("N");

    public static string WithDatabase(string connectionString, string database)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = database
        };

        return builder.ConnectionString;
    }

    public static void EnsureExists(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var database = builder.Database
            ?? throw new InvalidOperationException("The test connection string has no database.");

        builder.Database = "postgres";
        using var connection = new NpgsqlConnection(builder.ConnectionString);
        connection.Open();

        using (var exists = new NpgsqlCommand(
                   "SELECT 1 FROM pg_database WHERE datname = @name",
                   connection))
        {
            exists.Parameters.AddWithValue("name", database);
            if (exists.ExecuteScalar() is not null)
            {
                return;
            }
        }

        using var create = new NpgsqlCommand(
            $"CREATE DATABASE \"{Escape(database)}\"",
            connection);
        create.ExecuteNonQuery();
    }

    public static void Drop(string connectionString)
    {
        NpgsqlConnection.ClearAllPools();

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var database = builder.Database;
        if (string.IsNullOrWhiteSpace(database))
        {
            return;
        }

        builder.Database = "postgres";
        using var connection = new NpgsqlConnection(builder.ConnectionString);
        connection.Open();

        using (var terminate = new NpgsqlCommand(
                   "SELECT pg_terminate_backend(pid) FROM pg_stat_activity " +
                   "WHERE datname = @name AND pid <> pg_backend_pid()",
                   connection))
        {
            terminate.Parameters.AddWithValue("name", database);
            terminate.ExecuteNonQuery();
        }

        using var drop = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{Escape(database)}\"",
            connection);
        drop.ExecuteNonQuery();
    }

    private static string Escape(string identifier) => identifier.Replace("\"", "\"\"");
}
