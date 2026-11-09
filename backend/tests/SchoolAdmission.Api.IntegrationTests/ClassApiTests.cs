using System.Net;
using System.Net.Http.Json;
using SchoolAdmission.Api.IntegrationTests.Infrastructure;

namespace SchoolAdmission.Api.IntegrationTests;

public sealed class ClassApiTests(AdmissionApiFixture factory) : IClassFixture<AdmissionApiFixture>
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
    public async Task Unknown_class_returns_404()
    {
        var response = await _client.GetAsync("/classes/c_does_not_exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
