using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Infrastructure.Persistence.Configurations;

public sealed class ClassRoomConfiguration : IEntityTypeConfiguration<ClassRoom>
{
    public void Configure(EntityTypeBuilder<ClassRoom> builder)
    {
        builder.ToTable("classes", table =>
            table.HasCheckConstraint("ck_classes_total_seats", "total_seats > 0"));

        builder.HasKey(classRoom => classRoom.Id);

        builder.Property(classRoom => classRoom.Id)
            .HasColumnName("id")
            .HasMaxLength(40);

        builder.Property(classRoom => classRoom.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(classRoom => classRoom.TotalSeats)
            .HasColumnName("total_seats")
            .IsRequired();

        builder.Property(classRoom => classRoom.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(classRoom => classRoom.CreatedAt);
    }
}
