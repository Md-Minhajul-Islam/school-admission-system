using System.Net;
using SchoolAdmission.Api.IntegrationTests.Infrastructure;

namespace SchoolAdmission.Api.IntegrationTests;

public sealed class ConcurrencyApiTests
{
    [Fact]
    public async Task Concurrent_applications_from_two_hosts_never_exceed_total_seats()
    {
        var databaseName = TestDatabase.CreateUniqueName();
        using var firstHost = new AdmissionApiFactory(databaseName);
        using var secondHost = new AdmissionApiFactory(
            databaseName,
            createDatabase: false);

        using var firstClient = firstHost.CreateClient();
        using var secondClient = secondHost.CreateClient();

        const int totalSeats = 5;
        var classId = await ApiAssert.CreateClassAsync(
            firstClient,
            "Class 10 - Concurrency",
            totalSeats);

        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var requests = Enumerable.Range(0, 40).Select(index => Task.Run(async () =>
        {
            await start.Task;

            var client = index % 2 == 0 ? firstClient : secondClient;
            var studentId = "201526912" + index.ToString("D8");
            using var request = ApiAssert.Request(
                HttpMethod.Post,
                $"/classes/{classId}/holds",
                studentId);
            using var response = await client.SendAsync(request);

            return response.StatusCode;
        })).ToArray();

        start.SetResult();
        var statuses = await Task.WhenAll(requests);

        Assert.Equal(totalSeats, statuses.Count(status => status == HttpStatusCode.Created));
        Assert.Equal(
            statuses.Length - totalSeats,
            statuses.Count(status => status == HttpStatusCode.Conflict));
        Assert.DoesNotContain(statuses, status => (int)status >= 500);

        var firstView = await ApiAssert.ReadAsync<ApiAssert.ClassSeats>(
            await firstClient.GetAsync($"/classes/{classId}"));
        var secondView = await ApiAssert.ReadAsync<ApiAssert.ClassSeats>(
            await secondClient.GetAsync($"/classes/{classId}"));

        foreach (var view in new[] { firstView, secondView })
        {
            Assert.Equal(totalSeats, view.TotalSeats);
            Assert.Equal(totalSeats, view.HeldSeats);
            Assert.Equal(0, view.ConfirmedSeats);
            Assert.Equal(0, view.AvailableSeats);
            Assert.True(view.HeldSeats + view.ConfirmedSeats <= view.TotalSeats);
        }
    }
}
