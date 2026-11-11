using System.Net;
using System.Net.Http.Json;
using SchoolAdmission.Api.IntegrationTests.Infrastructure;

namespace SchoolAdmission.Api.IntegrationTests.Controllers;

public sealed class ClassesApiTests(AdmissionApiFixture factory) : IClassFixture<AdmissionApiFixture>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_class_returns_the_contract_and_zero_seat_counts()
    {
        var response = await _client.PostAsJsonAsync(
            "/classes",
            new { name = "Class 7 - Day Shift", totalSeats = 40 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await ApiAssert.ReadAsync<ApiAssert.CreatedClass>(response);
        Assert.StartsWith("c_", created.Id);
        Assert.Equal("Class 7 - Day Shift", created.Name);
        Assert.Equal(40, created.TotalSeats);

        var seats = await ApiAssert.ReadAsync<ApiAssert.ClassSeats>(
            await _client.GetAsync($"/classes/{created.Id}"));

        Assert.Equal(40, seats.TotalSeats);
        Assert.Equal(0, seats.HeldSeats);
        Assert.Equal(0, seats.ConfirmedSeats);
        Assert.Equal(40, seats.AvailableSeats);
    }

    [Fact]
    public async Task Class_list_contains_the_created_class()
    {
        var classId = await ApiAssert.CreateClassAsync(
            _client,
            "Class 8 - Morning Shift",
            totalSeats: 25);

        var classes = await ApiAssert.ReadAsync<List<ApiAssert.ClassSeats>>(
            await _client.GetAsync("/classes"));

        var created = Assert.Single(classes, item => item.Id == classId);
        Assert.Equal(25, created.TotalSeats);
        Assert.Equal(0, created.HeldSeats);
        Assert.Equal(25, created.AvailableSeats);
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData("Class 9", 0)]
    [InlineData("Class 9", -5)]
    public async Task Create_class_rejects_invalid_input(string name, int totalSeats)
    {
        var response = await _client.PostAsJsonAsync(
            "/classes",
            new { name, totalSeats });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var problem = await ApiAssert.ReadJsonAsync(response);
        Assert.True(problem.RootElement.TryGetProperty("detail", out _));
    }

    [Fact]
    public async Task Create_class_rejects_a_duplicate_name()
    {
        const string name = "Class 2 - Day Shift";

        var first = await _client.PostAsJsonAsync(
            "/classes",
            new { name, totalSeats = 30 });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        foreach (var variant in new[] { name, name.ToUpperInvariant() })
        {
            var duplicate = await _client.PostAsJsonAsync(
                "/classes",
                new { name = variant, totalSeats = 50 });

            Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
            Assert.Equal(
                "application/problem+json",
                duplicate.Content.Headers.ContentType?.MediaType);

            using var problem = await ApiAssert.ReadJsonAsync(duplicate);
            Assert.Contains(
                "already exists",
                problem.RootElement.GetProperty("detail").GetString());
        }

        var onlyClass = Assert.Single(
            await ApiAssert.ReadAsync<List<ApiAssert.ClassSeats>>(
                await _client.GetAsync("/classes")),
            item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(name, onlyClass.Name);
    }

    [Fact]
    public async Task Cors_exposes_the_date_header_used_for_clock_sync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/classes");
        request.Headers.Add("Origin", "http://localhost:3000");

        var response = await _client.SendAsync(request);

        Assert.True(
            response.Headers.TryGetValues("Access-Control-Expose-Headers", out var exposed));
        var names = exposed
            .SelectMany(value => value.Split(',', StringSplitOptions.TrimEntries))
            .ToArray();

        // Date is not a CORS-safelisted response header, so the browser can only read it
        // when it is listed explicitly. The UI needs it to estimate the server clock.
        Assert.Contains("Date", names, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("X-Correlation-Id", names, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Unknown_class_returns_404_and_keeps_cors_headers()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/classes/c_does_not_exist");
        request.Headers.Add("Origin", "http://localhost:3000");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(
            response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Contains("http://localhost:3000", origins);
    }
}
