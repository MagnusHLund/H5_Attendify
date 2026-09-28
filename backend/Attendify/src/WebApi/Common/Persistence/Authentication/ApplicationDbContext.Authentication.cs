using Attendify.Common.Domain.Authentication;

namespace Attendify.Common.Persistence;

public partial class ApplicationDbContext
{
    public DbSet<StudentAccessCode> StudentAccessCodes => AggregateRootSet<StudentAccessCode>();

    public DbSet<RefreshToken> RefreshTokens => AggregateRootSet<RefreshToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens =>
        AggregateRootSet<PasswordResetToken>();
}