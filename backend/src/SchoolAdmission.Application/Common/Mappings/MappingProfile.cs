using AutoMapper;
using SchoolAdmission.Application.Classes.Commands.CreateClass;
using SchoolAdmission.Application.Classes.Queries.GetClasses;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Application.Holds.Commands.ConfirmHold;
using SchoolAdmission.Application.Holds.Commands.CreateHold;
using SchoolAdmission.Application.Holds.Queries.GetHold;
using SchoolAdmission.Application.Holds.Queries.GetMyHolds;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Application.Common.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<HoldStatus, string>()
            .ConvertUsing(status => status.ToString().ToUpperInvariant());

        CreateMap<ClassRoom, ClassCreatedDto>();

        CreateMap<SeatSummary, ClassSeatDto>()
            .ForCtorParam(
                nameof(ClassSeatDto.AvailableSeats),
                options => options.MapFrom(summary =>
                    summary.TotalSeats - summary.HeldSeats - summary.ConfirmedSeats));

        CreateMap<Hold, HoldCreatedDto>()
            .ForCtorParam(nameof(HoldCreatedDto.HoldId), options => options.MapFrom(hold => hold.Id))
            .ForCtorParam(
                nameof(HoldCreatedDto.ExpiresAt),
                options => options.MapFrom(hold => hold.ExpiresAt.UtcDateTime));

        CreateMap<HoldSnapshot, HoldDto>()
            .ForCtorParam(
                nameof(HoldDto.ExpiresAt),
                options => options.MapFrom(snapshot => snapshot.ExpiresAt.UtcDateTime));

        CreateMap<HoldSnapshot, MyHoldDto>()
            .ForCtorParam(
                nameof(MyHoldDto.ExpiresAt),
                options => options.MapFrom(snapshot => snapshot.ExpiresAt.UtcDateTime));
    }
}
