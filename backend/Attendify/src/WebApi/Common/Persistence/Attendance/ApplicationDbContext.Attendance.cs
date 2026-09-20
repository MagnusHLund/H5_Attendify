using Attendify.Common.Domain.Attendance;

namespace Attendify.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<Attendance> Attendances => AggregateRootSet<Attendance>();

    public DbSet<AttendanceDetection> AttendanceDetections =>
        AggregateRootSet<AttendanceDetection>();

    public DbSet<AttendanceRecord> AttendanceRecords =>
        AggregateRootSet<AttendanceRecord>();
}