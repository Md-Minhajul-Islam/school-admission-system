using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SchoolAdmission.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API against a dedicated PostgreSQL database and exposes helpers for
/// sending the student identity header.
/// </summary>
public class AdmissionApiFactory : WebApplicationFactory<Program>
{
    private readonly bool _ownsDatabase;

    public AdmissionApiFactory()
        : this(TestDatabase.CreateUniqueName())
    {
    }

    public AdmissionApiFactory(
        string databaseName,
        int holdDurationSeconds = 600,
        bool createDatabase = true)
    {
        DatabaseName = databaseName;
        HoldDurationSeconds = holdDurationSeconds;
        ConnectionString = TestDatabase.WithDatabase(
            TestDatabase.BaseConnectionString,
            databaseName);
        _ownsDatabase = createDatabase;

        if (createDatabase)
        {
            TestDatabase.EnsureExists(ConnectionString);
        }

        // The application reads these values in top-level statements before the host is
        // built, so they must be visible to the default configuration providers.
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            ConnectionString);
        Environment.SetEnvironmentVariable(
            "HOLD_DURATION_SECONDS",
            holdDurationSeconds.ToString(CultureInfo.InvariantCulture));
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public int HoldDurationSeconds { get; }

    public HttpClient CreateApiClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Student-Id", "20152691234567890");
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["HOLD_DURATION_SECONDS"] =
                    HoldDurationSeconds.ToString(CultureInfo.InvariantCulture),
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning"
            }));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && _ownsDatabase)
        {
            TestDatabase.Drop(ConnectionString);
        }
    }
}
