using Attendify.Common.Domain.Attendance;

namespace Attendify.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<Attendance> Attendances => AggregateRootSet<Attendance>();
}