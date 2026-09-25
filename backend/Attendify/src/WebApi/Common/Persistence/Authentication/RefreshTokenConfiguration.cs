using Attendify.Common.Domain.Authentication;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class RefreshTokenConfiguration : AuditableConfiguration<RefreshToken>
{
    public override void PostConfigure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(token => token.Id);

        builder.Property(token => token.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();

        builder
            .Property(token => token.TokenHash)
            .HasMaxLength(RefreshToken.TokenHashLength)
            .IsRequired();

        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.TokenFamilyId).IsRequired();

        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.TokenFamilyId);

        builder
            .HasOne(token => token.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(token => token.UserId)
            .IsRequired();
    }
}
