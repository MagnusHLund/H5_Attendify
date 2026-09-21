using Attendify.Common.Domain.Attendance;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class AttendanceDetectionConfiguration : AuditableConfiguration<AttendanceDetection>
{
    public override void PostConfigure(EntityTypeBuilder<AttendanceDetection> builder)
    {
        builder.HasKey(detection => detection.Id);

        builder.Property(detection => detection.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder.Property(detection => detection.Classroom)
            .HasMaxLength(AttendanceDetection.ClassroomMaxLength)
            .IsRequired();

        builder.Property(detection => detection.DetectedAt).IsRequired();

        builder.HasOne(detection => detection.User)
            .WithMany(user => user.AttendanceDetections)
            .HasForeignKey(detection => detection.UserId)
            .IsRequired();
    }
}
