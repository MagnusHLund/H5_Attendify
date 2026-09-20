using Attendify.Common.Domain.Users;

namespace Attendify.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<User> Users => AggregateRootSet<User>();
}
