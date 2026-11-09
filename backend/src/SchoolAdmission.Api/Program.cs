using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.BackgroundServices;
using SchoolAdmission.Api.Middleware;
using SchoolAdmission.Application;
using SchoolAdmission.Application.Common.Options;
using SchoolAdmission.Infrastructure;
using SchoolAdmission.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings__DefaultConnection is not configured.");

var holdDurationSeconds = builder.Configuration.GetValue<int?>("HOLD_DURATION_SECONDS") ?? 600;
if (holdDurationSeconds <= 0)
{
    throw new InvalidOperationException("HOLD_DURATION_SECONDS must be greater than zero.");
}

var corsOrigins = (builder.Configuration["CORS_ORIGINS"]
        ?? "http://localhost:3000,http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddOptions<HoldOptions>()
    .Configure(options => options.HoldDurationSeconds = holdDurationSeconds)
    .Validate(options => options.HoldDurationSeconds > 0, "HOLD_DURATION_SECONDS must be greater than zero.")
    .ValidateOnStart();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("X-Correlation-Id")));

builder.Services.AddHostedService<HoldExpiryWorker>();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.MapControllers();

app.MapGet("/health", async (AdmissionDbContext db, CancellationToken cancellationToken) =>
    await db.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ok" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

await DatabaseMigrator.MigrateAsync(app.Services, app.Logger);

app.Run();

public partial class Program;
