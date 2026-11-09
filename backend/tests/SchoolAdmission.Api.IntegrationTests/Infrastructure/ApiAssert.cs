using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SchoolAdmission.Api.IntegrationTests.Infrastructure;

internal static class ApiAssert
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.IsSuccessStatusCode,
            $"Expected success but got {(int)response.StatusCode}: {body}");

        return JsonSerializer.Deserialize<T>(body, Json)
            ?? throw new InvalidOperationException($"Could not deserialize: {body}");
    }

    public static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body);
    }

    public static HttpRequestMessage Request(
        HttpMethod method,
        string path,
        string? studentId = null,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, path);

        if (studentId is not null)
        {
            request.Headers.Add("X-Student-Id", studentId);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    public static async Task<string> CreateClassAsync(
        HttpClient client,
        string name = "Class 6 - Morning Shift",
        int totalSeats = 120)
    {
        var response = await client.PostAsJsonAsync(
            "/classes",
            new { name, totalSeats });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<CreatedClass>(response);
        return created.Id;
    }

    public sealed record CreatedClass(string Id, string Name, int TotalSeats);

    public sealed record ClassSeats(
        string Id,
        string Name,
        int TotalSeats,
        int HeldSeats,
        int ConfirmedSeats,
        int AvailableSeats);

    public sealed record CreatedHold(
        string HoldId,
        string ClassId,
        string StudentId,
        string Status,
        DateTime ExpiresAt);

    public sealed record HoldStatus(
        string HoldId,
        string ClassId,
        string Status,
        DateTime ExpiresAt);

    public sealed record MyHold(
        string HoldId,
        string ClassId,
        string ClassName,
        string Status,
        DateTime ExpiresAt);
}
