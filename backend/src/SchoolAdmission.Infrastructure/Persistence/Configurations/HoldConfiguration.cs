using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Domain.Entities;
using SchoolAdmission.Domain.Enums;

namespace SchoolAdmission.Infrastructure.Persistence.Configurations;

public sealed class HoldConfiguration : IEntityTypeConfiguration<Hold>
{
    public void Configure(EntityTypeBuilder<Hold> builder)
    {
        builder.ToTable("holds", table =>
        {
            table.HasCheckConstraint(
                "ck_holds_status",
                "status IN ('HELD','CONFIRMED','EXPIRED','CANCELLED')");
            table.HasCheckConstraint(
                "ck_holds_student_id",
                "student_id ~ '^[0-9]{17}$'");
        });

        builder.HasKey(hold => hold.Id);

        builder.Property(hold => hold.Id)
            .HasColumnName("id")
            .HasMaxLength(40);

        builder.Property(hold => hold.ClassId)
            .HasColumnName("class_id")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(hold => hold.StudentId)
            .HasColumnName("student_id")
            .HasMaxLength(17)
            .IsRequired();

        builder.Property(hold => hold.Status)
            .HasColumnName("status")
            .HasMaxLength(16)
            .HasConversion(
                status => status.ToString().ToUpperInvariant(),
                value => Enum.Parse<HoldStatus>(value, true))
            .IsRequired();

        builder.Property(hold => hold.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(hold => hold.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(hold => hold.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(hold => hold.ClosedAt)
            .HasColumnName("closed_at");

        builder.HasOne(hold => hold.ClassRoom)
            .WithMany(classRoom => classRoom.Holds)
            .HasForeignKey(hold => hold.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(hold => new { hold.ClassId, hold.Status, hold.ExpiresAt });
        builder.HasIndex(hold => new { hold.StudentId, hold.Status, hold.ExpiresAt });
        builder.HasIndex(hold => new { hold.Status, hold.ExpiresAt });

        builder.HasIndex(hold => new { hold.ClassId, hold.StudentId })
            .IsUnique()
            .HasFilter("status IN ('HELD','CONFIRMED')")
            .HasDatabaseName("ux_holds_active_student_class");
    }
}
