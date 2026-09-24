using Attendify.Common.Domain.Authentication;

namespace Attendify.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<AdminAccessCode> AdminAccessCodes => AggregateRootSet<AdminAccessCode>();

    public DbSet<RefreshToken> RefreshTokens => AggregateRootSet<RefreshToken>();
}
