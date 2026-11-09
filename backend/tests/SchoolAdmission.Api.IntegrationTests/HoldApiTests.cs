using System.Net;
using System.Net.Http.Json;
using SchoolAdmission.Api.IntegrationTests.Infrastructure;

namespace SchoolAdmission.Api.IntegrationTests;

public sealed class HoldApiTests(AdmissionApiFixture factory) : IClassFixture<AdmissionApiFixture>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Application_can_be_held_confirmed_and_confirmed_again_idempotently()
    {
        var classId = await ApiAssert.CreateClassAsync(_client, totalSeats: 2);
        const string studentId = "20152691234000001";

        var hold = await ApplyAsync(classId, studentId);

        Assert.Equal("HELD", hold.Status);
        Assert.Equal(classId, hold.ClassId);
        Assert.Equal(studentId, hold.StudentId);
        Assert.Equal(DateTimeKind.Utc, hold.ExpiresAt.Kind);

        var afterHold = await GetClassAsync(classId);
        Assert.Equal(1, afterHold.HeldSeats);
        Assert.Equal(0, afterHold.ConfirmedSeats);
        Assert.Equal(1, afterHold.AvailableSeats);

        var status = await ApiAssert.ReadAsync<ApiAssert.HoldStatus>(
            await _client.GetAsync($"/holds/{hold.HoldId}"));
        Assert.Equal("HELD", status.Status);

        var confirm = await ConfirmAsync(hold.HoldId, studentId);
        Assert.Equal("CONFIRMED", confirm.Status);

        var confirmAgain = await ConfirmAsync(hold.HoldId, studentId);
        Assert.Equal("CONFIRMED", confirmAgain.Status);

        var afterConfirm = await GetClassAsync(classId);
        Assert.Equal(0, afterConfirm.HeldSeats);
        Assert.Equal(1, afterConfirm.ConfirmedSeats);
        Assert.Equal(1, afterConfirm.AvailableSeats);

        var mine = await ApiAssert.ReadAsync<List<ApiAssert.MyHold>>(
            await SendAsync(HttpMethod.Get, "/me/holds", studentId));

        var myHold = Assert.Single(mine, item => item.HoldId == hold.HoldId);
        Assert.Equal("CONFIRMED", myHold.Status);
        Assert.Equal("Class 6 - Morning Shift", myHold.ClassName);
    }

    [Fact]
    public async Task Only_the_owner_can_confirm_a_hold()
    {
        var classId = await ApiAssert.CreateClassAsync(
            _client,
            "Class 6 - Owner Confirm",
            totalSeats: 1);
        var hold = await ApplyAsync(classId, "20152691234000002");

        var forbidden = await SendAsync(
            HttpMethod.Post,
            $"/holds/{hold.HoldId}/confirm",
            "20152691234000003");

        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var confirmed = await ConfirmAsync(hold.HoldId, "20152691234000002");
        Assert.Equal("CONFIRMED", confirmed.Status);
    }

    [Fact]
    public async Task Cancel_releases_the_seat_and_is_idempotent()
    {
        var classId = await ApiAssert.CreateClassAsync(
            _client,
            "Class 6 - Cancel",
            totalSeats: 1);
        var hold = await ApplyAsync(classId, "20152691234000004");

        var first = await SendAsync(
            HttpMethod.Delete,
            $"/holds/{hold.HoldId}",
            "20152691234000004");
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var second = await SendAsync(
            HttpMethod.Delete,
            $"/holds/{hold.HoldId}",
            "20152691234000004");
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);

        var seats = await GetClassAsync(classId);
        Assert.Equal(0, seats.HeldSeats);
        Assert.Equal(1, seats.AvailableSeats);

        var status = await ApiAssert.ReadAsync<ApiAssert.HoldStatus>(
            await _client.GetAsync($"/holds/{hold.HoldId}"));
        Assert.Equal("CANCELLED", status.Status);
    }

    [Fact]
    public async Task Another_student_cannot_cancel_a_hold()
    {
        var classId = await ApiAssert.CreateClassAsync(
            _client,
            "Class 6 - Foreign Cancel",
            totalSeats: 1);
        var hold = await ApplyAsync(classId, "20152691234000005");

        var forbidden = await SendAsync(
            HttpMethod.Delete,
            $"/holds/{hold.HoldId}",
            "20152691234000006");

        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task A_class_cannot_be_overbooked_and_one_student_cannot_apply_twice()
    {
        var classId = await ApiAssert.CreateClassAsync(
            _client,
            "Class 6 - Overbooking",
            totalSeats: 1);
        var hold = await ApplyAsync(classId, "20152691234000007");

        var duplicate = await SendAsync(
            HttpMethod.Post,
            $"/classes/{classId}/holds",
            "20152691234000007");
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var full = await SendAsync(
            HttpMethod.Post,
            $"/classes/{classId}/holds",
            "20152691234000008");
        Assert.Equal(HttpStatusCode.Conflict, full.StatusCode);

        var seats = await GetClassAsync(classId);
        Assert.Equal(1, seats.HeldSeats);
        Assert.Equal(0, seats.AvailableSeats);
        Assert.True(seats.HeldSeats + seats.ConfirmedSeats <= seats.TotalSeats);

        var cancelled = await SendAsync(
            HttpMethod.Delete,
            $"/holds/{hold.HoldId}",
            "20152691234000007");
        Assert.Equal(HttpStatusCode.NoContent, cancelled.StatusCode);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("2015269123400000X")]
    [InlineData("201526912340000001")]
    public async Task Invalid_student_ids_are_rejected(string studentId)
    {
        var classId = await ApiAssert.CreateClassAsync(
            _client,
            $"Class 6 - Invalid Id {studentId}",
            totalSeats: 1);

        var response = await SendAsync(
            HttpMethod.Post,
            $"/classes/{classId}/holds",
            studentId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Missing_student_header_is_rejected_for_writes()
    {
        var classId = await ApiAssert.CreateClassAsync(
            _client,
            "Class 6 - Missing Header",
            totalSeats: 1);

        var response = await _client.PostAsync($"/classes/{classId}/holds", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_hold_returns_404()
    {
        var response = await _client.GetAsync("/holds/h_missing");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<ApiAssert.CreatedHold> ApplyAsync(string classId, string studentId)
    {
        var response = await SendAsync(
            HttpMethod.Post,
            $"/classes/{classId}/holds",
            studentId);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ApiAssert.ReadAsync<ApiAssert.CreatedHold>(response);
    }

    private async Task<ApiAssert.ClassSeats> GetClassAsync(string classId)
        => await ApiAssert.ReadAsync<ApiAssert.ClassSeats>(
            await _client.GetAsync($"/classes/{classId}"));

    private async Task<ConfirmedHold> ConfirmAsync(string holdId, string studentId)
        => await ApiAssert.ReadAsync<ConfirmedHold>(
            await SendAsync(HttpMethod.Post, $"/holds/{holdId}/confirm", studentId));

    private Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        string studentId)
        => _client.SendAsync(ApiAssert.Request(method, path, studentId));

    private sealed record ConfirmedHold(string HoldId, string Status);
}
