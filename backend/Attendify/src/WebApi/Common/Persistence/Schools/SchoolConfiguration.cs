using Attendify.Common.Domain.Schools;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Attendify.Common.Persistence;

public sealed class SchoolConfiguration : AuditableConfiguration<School>
{
    public override void PostConfigure(EntityTypeBuilder<School> builder)
    {
        builder.HasKey(school => school.Id);

        builder.Property(school => school.Id)
            .ValueGeneratedOnAdd();

        builder.Property(school => school.Name)
            .HasMaxLength(School.NameMaxLength)
            .IsRequired();

        builder.HasIndex(school => school.Name).IsUnique();
    }
}
