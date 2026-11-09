using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Infrastructure.Persistence;
using SchoolAdmission.Infrastructure.Persistence.Repositories;

namespace SchoolAdmission.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AdmissionDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IRepository, Repository>();
        services.AddScoped<IClassRoomRepository, ClassRoomRepository>();
        services.AddScoped<IHoldRepository, HoldRepository>();

        return services;
    }
}
