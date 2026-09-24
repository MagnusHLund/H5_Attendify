using Attendify.Common.Domain.Attendance;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class AttendanceRecordConfiguration : AuditableConfiguration<AttendanceRecord>
{
    public override void PostConfigure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder.Property(record => record.Classroom)
            .HasMaxLength(AttendanceRecord.ClassroomMaxLength)
            .IsRequired();

        builder.Property(record => record.ArrivalTime).IsRequired();
        builder.Property(record => record.DepartureTime).IsRequired();
        builder.Property(record => record.DepartureKnown).IsRequired();
        builder.Property(record => record.AttendanceDate).IsRequired();

        builder.HasOne(record => record.User)
            .WithMany(user => user.AttendanceRecords)
            .HasForeignKey(record => record.UserId)
            .IsRequired();
    }
}
