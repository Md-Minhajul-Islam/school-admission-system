using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.IntegrationTests.Infrastructure;
using SchoolAdmission.Domain.Enums;
using SchoolAdmission.Infrastructure.Persistence;

namespace SchoolAdmission.Api.IntegrationTests;

public sealed class ShortHoldApiFactory : AdmissionApiFactory
{
    public ShortHoldApiFactory()
        : base(TestDatabase.CreateUniqueName(), holdDurationSeconds: 1)
    {
    }
}

public sealed class ExpiryApiTests(ShortHoldApiFactory factory)
    : IClassFixture<ShortHoldApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Expired_holds_are_released_automatically_and_can_no_longer_be_paid()
    {
        var classId = await ApiAssert.CreateClassAsync(_client, totalSeats: 1);
        const string studentId = "20152691234000020";

        var response = await _client.SendAsync(
            ApiAssert.Request(HttpMethod.Post, $"/classes/{classId}/holds", studentId));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var hold = await ApiAssert.ReadAsync<ApiAssert.CreatedHold>(response);

        await WaitUntilAsync(
            async () => await StoredStatusAsync(factory.ConnectionString, hold.HoldId)
                == HoldStatus.Expired,
            TimeSpan.FromSeconds(15));

        var status = await ApiAssert.ReadAsync<ApiAssert.HoldStatus>(
            await _client.GetAsync($"/holds/{hold.HoldId}"));
        Assert.Equal("EXPIRED", status.Status);
        Assert.InRange((status.ExpiresAt - hold.ExpiresAt).Duration(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

        var seats = await ApiAssert.ReadAsync<ApiAssert.ClassSeats>(
            await _client.GetAsync($"/classes/{classId}"));
        Assert.Equal(0, seats.HeldSeats);
        Assert.Equal(0, seats.ConfirmedSeats);
        Assert.Equal(1, seats.AvailableSeats);

        var confirm = await _client.SendAsync(
            ApiAssert.Request(HttpMethod.Post, $"/holds/{hold.HoldId}/confirm", studentId));
        Assert.Equal(HttpStatusCode.Conflict, confirm.StatusCode);

        var cancel = await _client.SendAsync(
            ApiAssert.Request(HttpMethod.Delete, $"/holds/{hold.HoldId}", studentId));
        Assert.Equal(HttpStatusCode.Conflict, cancel.StatusCode);

        var retry = await _client.SendAsync(
            ApiAssert.Request(
                HttpMethod.Post,
                $"/classes/{classId}/holds",
                "20152691234000021"));
        Assert.Equal(HttpStatusCode.Created, retry.StatusCode);
    }

    private static async Task<HoldStatus> StoredStatusAsync(
        string connectionString,
        string holdId)
    {
        var options = new DbContextOptionsBuilder<AdmissionDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var db = new AdmissionDbContext(options);
        return await db.Holds
            .AsNoTracking()
            .Where(hold => hold.Id == holdId)
            .Select(hold => hold.Status)
            .SingleAsync();
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }

        throw new TimeoutException("The condition was not met before the timeout.");
    }
}
