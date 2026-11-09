using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Infrastructure.Persistence;

public sealed class AdmissionDbContext(DbContextOptions<AdmissionDbContext> options)
    : DbContext(options)
{
    public DbSet<ClassRoom> Classes => Set<ClassRoom>();

    public DbSet<Hold> Holds => Set<Hold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdmissionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
