namespace SchoolAdmission.Api.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit class fixture wrapper. xUnit requires fixture types to expose exactly one public
/// constructor, while <see cref="AdmissionApiFactory"/> intentionally offers more.
/// </summary>
public sealed class AdmissionApiFixture : AdmissionApiFactory
{
    public AdmissionApiFixture()
    {
    }
}
