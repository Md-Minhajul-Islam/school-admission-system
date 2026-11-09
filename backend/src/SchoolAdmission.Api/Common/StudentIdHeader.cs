namespace SchoolAdmission.Api.Common;

public static class StudentIdHeader
{
    public const string Name = "X-Student-Id";

    public static string Read(HttpRequest request)
        => request.Headers.TryGetValue(Name, out var value)
            ? value.ToString().Trim()
            : string.Empty;
}
