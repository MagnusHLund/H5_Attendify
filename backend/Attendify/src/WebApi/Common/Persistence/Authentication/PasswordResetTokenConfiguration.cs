using Attendify.Common.Domain.Authentication;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class PasswordResetTokenConfiguration : AuditableConfiguration<PasswordResetToken>
{
    public override void PostConfigure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(token => token.Id);

        builder.Property(token => token.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();

        builder.Property(token => token.SecurityCodeHash)
            .HasMaxLength(PasswordResetToken.SecurityCodeHashLength)
            .IsRequired();

        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.FailedAttempts).IsRequired();

        builder.HasIndex(token => token.UserId).IsUnique();

        builder.HasOne(token => token.User)
            .WithMany(user => user.PasswordResetTokens)
            .HasForeignKey(token => token.UserId)
            .IsRequired();
    }
}
