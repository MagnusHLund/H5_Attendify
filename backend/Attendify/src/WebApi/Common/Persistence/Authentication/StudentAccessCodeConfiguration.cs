using Attendify.Common.Domain.Authentication;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class StudentAccessCodeConfiguration : AuditableConfiguration<StudentAccessCode>
{
    public override void PostConfigure(EntityTypeBuilder<StudentAccessCode> builder)
    {
        builder.HasKey(accessCode => accessCode.Id);

        builder
            .Property(accessCode => accessCode.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder
            .Property(accessCode => accessCode.AccessCodeHash)
            .HasMaxLength(StudentAccessCode.CodeMaxLength)
            .IsRequired();

        builder.Property(accessCode => accessCode.GeneratedAt).IsRequired();
        builder.Property(accessCode => accessCode.ExpiresAt).IsRequired();

        builder
           .HasIndex(accessCode => new
           {
               accessCode.UserId,
               accessCode.GenerationDate
           })
           .IsUnique();

        builder
            .HasOne(accessCode => accessCode.User)
            .WithMany(user => user.StudentAccessCodes)
            .HasForeignKey(accessCode => accessCode.UserId)
            .IsRequired();
    }
}