using Attendify.Common.Domain.Users;
using Attendify.Common.Domain.FacialRecognition;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class UserConfiguration : AuditableConfiguration<User>
{
    public override void PostConfigure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        builder.Property(user => user.Email)
            .HasMaxLength(User.EmailMaxLength)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(User.PasswordHashMaxLength)
            .IsRequired();

        builder.Property(user => user.EncryptedStudentId)
            .HasMaxLength(User.EncryptedStudentIdMaxLength)
            .IsRequired();

        builder.Property(user => user.AttendanceEnabled).IsRequired();

        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasOne(user => user.School)
            .WithMany(school => school.Users)
            .HasForeignKey(user => user.SchoolId)
            .IsRequired();

        builder.HasOne(user => user.FacialProfile)
            .WithOne(profile => profile.User)
            .HasForeignKey<FacialProfile>(profile => profile.UserId)
            .IsRequired();
    }
}
