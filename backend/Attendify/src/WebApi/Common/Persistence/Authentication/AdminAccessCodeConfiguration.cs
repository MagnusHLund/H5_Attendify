using Attendify.Common.Domain.Authentication;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class AdminAccessCodeConfiguration : AuditableConfiguration<AdminAccessCode>
{
    public override void PostConfigure(EntityTypeBuilder<AdminAccessCode> builder)
    {
        builder.HasKey(accessCode => accessCode.Id);

        builder
            .Property(accessCode => accessCode.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder
            .Property(accessCode => accessCode.EncryptedAccessCode)
            .HasMaxLength(AdminAccessCode.CodeMaxLength)
            .IsRequired();

        builder.Property(accessCode => accessCode.GeneratedAt).IsRequired();
        builder.Property(accessCode => accessCode.ExpiresAt).IsRequired();

        builder
            .HasOne(accessCode => accessCode.User)
            .WithMany(user => user.AdminAccessCodes)
            .HasForeignKey(accessCode => accessCode.UserId)
            .IsRequired();
    }
}
