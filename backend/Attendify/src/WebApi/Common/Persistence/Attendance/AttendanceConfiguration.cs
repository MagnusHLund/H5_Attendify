using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Attendify.Common.Domain.Attendance;

namespace Attendify.Common.Persistence;

public class AttendanceConfiguration : AuditableConfiguration<Attendance>
{
    public override void PostConfigure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(t => t.Id);

        builder.HasKey(attendance => attendance.Id);

        builder.Property(attendance => attendance.UserId)
            .IsRequired();

        builder.Property(attendance => attendance.AttendanceDate)
            .IsRequired();

        builder.Property(attendance => attendance.ArrivedAt)
            .IsRequired();

        builder.Property(attendance => attendance.DepartedAt)
            .IsRequired(false);

        builder.Property(attendance => attendance.Classroom)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(attendance => attendance.Status)
            .IsRequired();
    }
}