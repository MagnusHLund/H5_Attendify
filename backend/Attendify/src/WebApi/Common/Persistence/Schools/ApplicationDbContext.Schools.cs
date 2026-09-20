using Attendify.Common.Domain.Schools;

namespace Attendify.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<School> Schools => AggregateRootSet<School>();
}
