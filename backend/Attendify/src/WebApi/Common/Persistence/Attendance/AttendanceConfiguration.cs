using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Attendify.Common.Domain.Attendance;

namespace Attendify.Common.Persistence;

public sealed class AttendanceConfiguration : AuditableConfiguration<Attendance>
{
    public override void PostConfigure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(attendance => attendance.Id);

        builder.Property(attendance => attendance.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder.Property(attendance => attendance.UserId)
            .IsRequired();

        builder.Property(attendance => attendance.AttendanceDate)
            .IsRequired();

        builder.Property(attendance => attendance.ArrivedAt)
            .IsRequired();

        builder.Property(attendance => attendance.Classroom)
            .HasMaxLength(Attendance.ClassroomMaxLength)
            .IsRequired();

        builder.Property(attendance => attendance.Status)
            .IsRequired();
    }
}