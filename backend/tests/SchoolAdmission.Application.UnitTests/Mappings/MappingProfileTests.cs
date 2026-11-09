using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Common.Mappings;
using SchoolAdmission.Application.Holds.Commands.CreateHold;
using SchoolAdmission.Application.Holds.Queries.GetHold;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.UnitTests.Mappings;

public sealed class MappingProfileTests
{
    private static readonly IMapper Mapper = new MapperConfiguration(
            configuration => configuration.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance)
        .CreateMapper();

    [Fact]
    public void Mapping_configuration_is_valid()
    {
        var configuration = new MapperConfiguration(
            config => config.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Hold_maps_to_the_contract_with_utc_expiry_and_uppercase_status()
    {
        var now = new DateTimeOffset(2026, 11, 1, 4, 0, 0, TimeSpan.Zero);
        var hold = Hold.Create("h_1", "c_1", "20152691234567890", now, TimeSpan.FromMinutes(10));

        var dto = Mapper.Map<HoldCreatedDto>(hold);

        Assert.Equal("h_1", dto.HoldId);
        Assert.Equal("HELD", dto.Status);
        Assert.Equal(DateTimeKind.Utc, dto.ExpiresAt.Kind);
        Assert.Equal(now.AddMinutes(10), dto.ExpiresAt);
    }

    [Fact]
    public void Hold_snapshot_uses_the_effective_status_provided_by_the_repository()
    {
        var snapshot = new HoldSnapshot(
            "h_1",
            "c_1",
            "20152691234567890",
            HoldStatus.Expired,
            new DateTimeOffset(2026, 11, 1, 4, 10, 0, TimeSpan.Zero),
            "Class 6 - Morning Shift");

        var dto = Mapper.Map<HoldDto>(snapshot);

        Assert.Equal("EXPIRED", dto.Status);
        Assert.Equal("c_1", dto.ClassId);
    }
}
